namespace Aurila.Models.Navigation;

public class RouteInfoChangedEventArgs(RouteInfo Info) : EventArgs
{
    public RouteInfo Info { get; } = Info;
}
