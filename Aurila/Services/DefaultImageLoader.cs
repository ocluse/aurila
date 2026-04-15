namespace Aurila.Services;

public class DefaultImageLoader : IImageLoader
{
    public async Task<string?> LoadAsync(ImageSource source, CancellationToken cancellationToken = default)
    {
        if (source is UrlImageSource url)
        {
            return url.Url;
        }
        else if (source is ByteArrayImageSource byteArray)
        {
            return SrcFromBytes(byteArray.Data, byteArray.Format);

        }
        else if (source is StreamImageSource stream)
        {
            using MemoryStream ms = new();
            await stream.Data.CopyToAsync(ms, cancellationToken);
            return SrcFromBytes(ms.ToArray(), stream.Format);
        }
        else
        {
            throw new NotSupportedException($"Unsupported image source type: {source.GetType().Name}");
        }
    }

    public static string SrcFromBytes(byte[] data, ImageFormat imageFormat)
    {
        var base64 = Convert.ToBase64String(data);
        var format = imageFormat switch
        {
            ImageFormat.Jpeg => "jpeg",
            ImageFormat.Png => "png",
            ImageFormat.Gif => "gif",
            ImageFormat.Webp => "webp",
            ImageFormat.Svg => "svg+xml",
            _ => throw new NotSupportedException($"Unsupported image format: {imageFormat}")
        };
        return $"data:image/{format};base64,{base64}";
    }
}