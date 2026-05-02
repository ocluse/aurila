namespace Aurila.Models.Navigation;

public record NavEntry(string Id, string Url, string? SerializedState = null);