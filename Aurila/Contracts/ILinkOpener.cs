namespace Aurila.Contracts;

public interface ILinkOpener
{
    ValueTask OpenInNewTabAsync(string url);
}
