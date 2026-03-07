namespace Aurila.Contracts;

public interface IImageLoader
{
    Task<string?> LoadAsync(ImageSource source, CancellationToken cancellationToken = default);
}
