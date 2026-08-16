using Aurila.Components.Navigation;
using Aurila.Models.Navigation;
using System.Collections.Concurrent;
using System.Reflection;

namespace Aurila.Services.Navigation;

internal sealed class PageParametersCache
{
    private readonly ConcurrentDictionary<Type, IReadOnlyList<PageParameterInfo>> _cache = new();

    public IReadOnlyList<PageParameterInfo> GetParameters(Type pageType)
        => _cache.GetOrAdd(pageType, BuildParameterMap);

    /// <summary>
    /// The values to supply as component parameters when the page is rendered.
    /// </summary>
    public Dictionary<string, object?> GetAvailableParameters(Type pageType, RouteParameters routeParameters)
    {
        var available = new Dictionary<string, object?>();

        foreach (var parameter in GetParameters(pageType))
        {
            if (parameter.IsHolder)
            {
                continue;
            }

            available[parameter.Property.Name] =
                routeParameters.TryGet(parameter.PropertyType, parameter.ExternalName, out var value)
                    ? value
                    : Default(parameter.PropertyType);
        }

        return available;
    }

    /// <summary>
    /// Connects a page's two-way query parameters to the URL and pushes the current values into them.
    /// </summary>
    private static object? Default(Type type)
        => type.IsValueType && Nullable.GetUnderlyingType(type) is null
            ? Activator.CreateInstance(type)
            : null;

    public void BindHolders(object page, RouteParameters routeParameters, IQueryParamWriter writer)
    {
        foreach (var parameter in GetParameters(page.GetType()))
        {
            if (!parameter.IsHolder)
            {
                continue;
            }

            if (parameter.Property.GetValue(page) is not IQueryParam holder)
            {
                continue;
            }

            holder.Bind(parameter.ExternalName, writer);
            holder.ReadFrom(routeParameters);
        }
    }

    /// <summary>
    /// Pushes the current URL values into a page's two-way query parameters without rebinding.
    /// </summary>
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

    private static IReadOnlyList<PageParameterInfo> BuildParameterMap(Type pageType)
    {
        List<PageParameterInfo> map = [];

        foreach (var property in pageType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            bool isHolder = IsQueryParamHolder(property.PropertyType);

            if (!isHolder && (!property.CanWrite || property.GetCustomAttribute<ParameterAttribute>(inherit: true) is null))
            {
                continue;
            }

            var queryAttribute = property.GetCustomAttribute<QueryParameterAttribute>(inherit: true);

            if (isHolder && queryAttribute is null)
            {
                continue;
            }

            map.Add(new PageParameterInfo
            {
                ExternalName = string.IsNullOrWhiteSpace(queryAttribute?.Name)
                    ? property.Name
                    : queryAttribute.Name.Trim(),
                Property = property,
                IsHolder = isHolder
            });
        }

        return map;
    }

    private static bool IsQueryParamHolder(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(QueryParam<>);
}
