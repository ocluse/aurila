using Aurila.Enums.Navigation;
using System.Text.RegularExpressions;

namespace Aurila.Models;

public sealed record RouteParameter(string Name, RouteParameterType Type);

public sealed record RouteParameterValue(RouteParameter Parameter, string Value);

public sealed partial class RouteTemplate
{
    public string Template { get; }

    public IReadOnlyList<RouteParameter> Parameters { get; }

    private readonly Regex _regex;

    public bool HasTemplates => Parameters.Count > 0;

    public RouteTemplate(string template)
    {
        Template = template ?? throw new ArgumentNullException(nameof(template));
        Parameters = ParseParameters(template);
        _regex = BuildRegex();
    }

    public bool HasParameter(string name)
        => Parameters.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    public RouteParameter? GetParameter(string name)
        => Parameters.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

    public bool TryMatch(string path, out Dictionary<string, string> parameters)
    {
        var match = _regex.Match(path);

        if (!match.Success)
        {
            parameters = Empty;
            return false;
        }

        parameters = ExtractParameters(match);
        return true;
    }

    public bool TryMatchParameters(string path, out RouteParameterValue[] parameterValues)
    {
        var match = _regex.Match(path);
        if (!match.Success)
        {
            parameterValues = [];
            return false;
        }
        var dict = ExtractParameters(match);
        parameterValues = [.. Parameters
            .Where(p => dict.ContainsKey(p.Name))
            .Select(p => new RouteParameterValue(p, dict[p.Name]))];
        return true;
    }

    public override string ToString() => Template;

    private static List<RouteParameter> ParseParameters(string template)
    {
        var list = new List<RouteParameter>();

        foreach (Match match in TemplateParserRegex().Matches(template))
        {
            if (!match.Groups[1].Success)
                continue;

            var token = match.Groups[1].Value; // e.g. "id:int"
            var parts = token.Split(':', 2);

            var name = parts[0];
            var type = parts.Length > 1
                ? ParseType(parts[1])
                : RouteParameterType.String;

            list.Add(new RouteParameter(name, type));
        }

        return list;
    }

    private static RouteParameterType ParseType(string constraint)
        => constraint.ToLowerInvariant() switch
        {
            "int" => RouteParameterType.Int,
            "guid" => RouteParameterType.Guid,
            "bool" => RouteParameterType.Bool,
            "date" => RouteParameterType.Date,
            "time" => RouteParameterType.Time,
            "datetime" => RouteParameterType.DateTime,
            _ => RouteParameterType.String
        };

    private Regex BuildRegex()
    {
        var patternStr = TemplateParserRegex().Replace(Template, match =>
        {
            if (match.Groups[1].Success)
            {
                var token = match.Groups[1].Value;
                var parts = token.Split(':', 2);

                var name = parts[0];
                var type = parts.Length > 1
                    ? ParseType(parts[1])
                    : RouteParameterType.String;

                var regexPart = type switch
                {
                    RouteParameterType.Int => @"\d+",
                    RouteParameterType.Guid => @"[0-9a-fA-F\-]{36}",
                    RouteParameterType.Bool => @"true|false",
                    RouteParameterType.Date => @"\d{4}-\d{2}-\d{2}",
                    RouteParameterType.Time => @"\d{2}(:|%3[aA])\d{2}((:|%3[aA])\d{2})?(?:\.\d+)?",
                    RouteParameterType.DateTime => @"\d{4}-\d{2}-\d{2}[T ]\d{2}(:|%3[aA])\d{2}[^/]*",
                    _ => @"[^/]+"
                };

                return $"(?<{name}>{regexPart})";
            }

            return Regex.Escape(match.Value);
        });

        return new Regex($"^{patternStr}/?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private static Dictionary<string, string> ExtractParameters(Match match)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Group group in match.Groups.Cast<Group>())
        {
            if (group.Success && !int.TryParse(group.Name, out _))
            {
                dict[group.Name] = Uri.UnescapeDataString(group.Value);
            }
        }

        return dict;
    }

    private static readonly Dictionary<string, string> Empty = new(StringComparer.OrdinalIgnoreCase);

    // (reuse ParseParameters + ParseType from previous message)

    [GeneratedRegex(@"\{([^}]+)\}|([^{]+)")]
    private static partial Regex TemplateParserRegex();
}
