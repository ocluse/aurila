using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;

public class UpdateRouteArgs
{
    public required string Url { get; init; }

    public string? SerializedState { get; init; }

    public required NavigationType NavigationType { get; init; }
}
