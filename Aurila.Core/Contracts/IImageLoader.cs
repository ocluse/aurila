namespace Aurila.Contracts;

public interface IImageLoader
{
    public Task<string?> LoadAsync(ImageSource source);
}
