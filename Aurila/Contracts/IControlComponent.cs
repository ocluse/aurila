namespace Aurila.Contracts;

public interface IControlComponent
{
    IEnumerable<KeyValuePair<string, object>> GetAppliedAttributes();
}
