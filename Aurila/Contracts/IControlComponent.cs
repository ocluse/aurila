namespace Aurila.Contracts;

public interface IControlComponent
{
    IEnumerable<KeyValuePair<string, object>> GetAppliedAttributes();

    Task CallStateHasChangedOnContextAsync();

    void CallStateHasChanged();
}
