using Aurila.Components.Navigation;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Aurila.Services.Navigation;

internal sealed class PageParametersCache
{
    private static readonly JsonSerializerOptions _stateOptions = new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<Type, IReadOnlyList<PageParameterInfo>> _cache = new();

    public IReadOnlyList<PageParameterInfo> GetParameters(Type pageType)
        => _cache.GetOrAdd(pageType, BuildParameterMap);

    public Dictionary<string, object?> GetAvailableParameters(
        Type pageType,
        RouteParameters routeParameters,
        object? routeArgument,
        JsonElement? entryState,
        IReadOnlyDictionary<string, object?> memoryState)
    {
        var available = new Dictionary<string, object?>();

        foreach (var parameter in GetParameters(pageType))
        {
            if (parameter.IsHolder)
            {
                continue;
            }

            available[parameter.Property.Name] = Resolve(
                parameter, routeParameters, routeArgument, entryState, memoryState);
        }

        return available;
    }

    /// <summary>
    /// Reads a page's declared state back off it, ready to be stored on its history entry.
    /// </summary>
    public (Dictionary<string, object?> Durable, Dictionary<string, object?> Memory) CaptureState(object page)
    {
        Dictionary<string, object?> durable = [];
        Dictionary<string, object?> memory = [];

        foreach (var parameter in GetParameters(page.GetType()))
        {
            if (parameter.Source != PageParameterSource.State)
            {
                continue;
            }

            var target = parameter.Scope == StateScope.Entry ? durable : memory;

            target[parameter.ExternalName] = parameter.Property.GetValue(page);
        }

        return (durable, memory);
    }

    /// <summary>
    /// Splits a value handed to a navigation into the tiers its destination declares, matching by type.
    /// </summary>
    public (Dictionary<string, object?> Durable, Dictionary<string, object?> Memory) SplitState(
        Type? pageType,
        object? state)
    {
        Dictionary<string, object?> durable = [];
        Dictionary<string, object?> memory = [];

        if (pageType is null || state is null)
        {
            return (durable, memory);
        }

        var candidates = GetParameters(pageType)
            .Where(p => p.Source == PageParameterSource.State)
            .Where(p => p.PropertyType.IsInstanceOfType(state))
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"'{pageType.FullName}' has no [NavigationState] property that accepts " +
                $"'{state.GetType().FullName}'.");
        }

        if (candidates.Count > 1)
        {
            throw new InvalidOperationException(
                $"'{pageType.FullName}' has more than one [NavigationState] property that accepts " +
                $"'{state.GetType().FullName}': {string.Join(", ", candidates.Select(c => c.Property.Name))}.");
        }

        var match = candidates[0];
        var target = match.Scope == StateScope.Entry ? durable : memory;

        target[match.ExternalName] = state;

        return (durable, memory);
    }

    public void BindHolders(object page, RouteParameters routeParameters, IQueryParamWriter? writer)
    {
        foreach (var parameter in GetParameters(page.GetType()))
        {
            if (parameter.IsHolder && parameter.Property.GetValue(page) is IQueryParam holder)
            {
                holder.Bind(parameter.ExternalName, writer);
                holder.ReadFrom(routeParameters);
            }
        }
    }

    public void RefreshHolders(object page, RouteParameters routeParameters)
    {
        foreach (var parameter in GetParameters(page.GetType()))
        {
            if (parameter.IsHolder && parameter.Property.GetValue(page) is IQueryParam holder)
            {
                holder.ReadFrom(routeParameters);
            }
        }
    }

    private static object? Resolve(
        PageParameterInfo parameter,
        RouteParameters routeParameters,
        object? routeArgument,
        JsonElement? entryState,
        IReadOnlyDictionary<string, object?> memoryState)
    {
        switch (parameter.Source)
        {
            case PageParameterSource.RoutePath:
                return routeParameters.TryGetFromPath(parameter.PropertyType, parameter.ExternalName, out var path)
                    ? path
                    : DefaultOf(parameter.PropertyType);

            case PageParameterSource.Query:
                return routeParameters.TryGetFromQuery(parameter.PropertyType, parameter.ExternalName, out var query)
                    ? query
                    : DefaultOf(parameter.PropertyType);

            case PageParameterSource.RouteArgument:
                return parameter.PropertyType.IsInstanceOfType(routeArgument)
                    ? routeArgument
                    : DefaultOf(parameter.PropertyType);

            case PageParameterSource.State when parameter.Scope == StateScope.Memory:
                return memoryState.TryGetValue(parameter.ExternalName, out var kept)
                    && parameter.PropertyType.IsInstanceOfType(kept)
                    ? kept
                    : DefaultOf(parameter.PropertyType);

            case PageParameterSource.State:
                return ReadFromEntryState(parameter, entryState);

            default:
                return DefaultOf(parameter.PropertyType);
        }
    }

    private static object? ReadFromEntryState(PageParameterInfo parameter, JsonElement? entryState)
    {
        if (entryState is not { ValueKind: JsonValueKind.Object } state
            || !TryReadProperty(state, parameter.ExternalName, out var value)
            || value.ValueKind == JsonValueKind.Null)
        {
            return DefaultOf(parameter.PropertyType);
        }

        try
        {
            return value.Deserialize(parameter.PropertyType, _stateOptions);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            return DefaultOf(parameter.PropertyType);
        }
    }

    private static bool TryReadProperty(JsonElement state, string name, out JsonElement value)
    {
        if (state.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (var property in state.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static object? DefaultOf(Type type)
        => type.IsValueType && Nullable.GetUnderlyingType(type) is null
            ? Activator.CreateInstance(type)
            : null;

    private static IReadOnlyList<PageParameterInfo> BuildParameterMap(Type pageType)
    {
        List<PageParameterInfo> map = [];

        foreach (var property in pageType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var info = Describe(property);

            if (info is not null)
            {
                map.Add(info);
            }
        }

        return map;
    }

    private static PageParameterInfo? Describe(PropertyInfo property)
    {
        bool isHolder = property.PropertyType.IsGenericType
            && property.PropertyType.GetGenericTypeDefinition() == typeof(QueryParam<>);

        if (property.GetCustomAttribute<RouteParameterAttribute>(inherit: true) is { } route)
        {
            return Build(property, PageParameterSource.RoutePath, route.Name, false);
        }

        if (property.GetCustomAttribute<QueryParameterAttribute>(inherit: true) is { } query)
        {
            return Build(property, PageParameterSource.Query, query.Name, isHolder);
        }

        if (property.GetCustomAttribute<RouteArgumentAttribute>(inherit: true) is not null)
        {
            return Build(property, PageParameterSource.RouteArgument, null, false);
        }

        if (property.GetCustomAttribute<NavigationStateAttribute>(inherit: true) is { } state)
        {
            return Build(property, PageParameterSource.State, state.Name, false) with { Scope = state.Scope };
        }

        return null;
    }

    private static PageParameterInfo Build(
        PropertyInfo property,
        PageParameterSource source,
        string? name,
        bool isHolder)
    {
        if (!isHolder && !property.CanWrite)
        {
            throw new InvalidOperationException(
                $"'{property.DeclaringType?.FullName}.{property.Name}' is bound from navigation but has no setter.");
        }

        if (!isHolder && property.GetCustomAttribute<ParameterAttribute>(inherit: true) is null)
        {
            throw new InvalidOperationException(
                $"'{property.DeclaringType?.FullName}.{property.Name}' is bound from navigation, so it must " +
                "also be marked [Parameter] — navigation inputs are supplied as ordinary component parameters.");
        }

        return new PageParameterInfo
        {
            Source = source,
            ExternalName = string.IsNullOrWhiteSpace(name) ? property.Name : name.Trim(),
            Property = property,
            IsHolder = isHolder
        };
    }
}
