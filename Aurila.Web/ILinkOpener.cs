namespace Aurila.Web;

public interface ILinkOpener
{
    ValueTask OpenInNewTabAsync(string url);
}
