using System.Text.Json.Serialization;

namespace Aurila.Enums.Navigation;

/// <summary>
/// The platform's classification of a navigation, as reported by <c>NavigateEvent.navigationType</c>.
/// </summary>
/// <remarks>
/// This is deliberately the browser's vocabulary rather than Aurila's. The framework's own reading of
/// what a navigation <em>means</em> — back versus forward versus a parameter rebind — is derived from
/// this plus the destination index, and is expressed by <see cref="NavIntent"/>.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<NavKind>))]
public enum NavKind
{
    [JsonStringEnumMemberName("push")] Push,
    [JsonStringEnumMemberName("replace")] Replace,
    [JsonStringEnumMemberName("reload")] Reload,
    [JsonStringEnumMemberName("traverse")] Traverse
}
