namespace Aurila.Contracts;

public interface IImageLoader
{
    public Task<string?> LoadAsync(string? imageUrl);
}
