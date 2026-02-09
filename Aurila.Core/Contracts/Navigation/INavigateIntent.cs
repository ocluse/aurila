namespace Aurila.Contracts.Navigation;

public interface INavigateIntent
{
    
}

public interface INavigateToPageIntent : INavigateIntent
{
    Type Page { get; }
    object? Data { get; }
    bool Replace { get; }
}

public interface INavigateBackIntent : INavigateIntent
{
}