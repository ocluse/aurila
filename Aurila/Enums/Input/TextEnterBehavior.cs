namespace Aurila.Enums.Input;

/// <summary>
/// Determines how a multiline text field maps Enter and modifier+Enter.
/// </summary>
public enum TextEnterBehavior
{
    /// <summary>Enter always keeps its native line-break behavior.</summary>
    NewLine,

    /// <summary>Enter submits, while the configured modifier plus Enter inserts a line break.</summary>
    SubmitUnlessModified,

    /// <summary>Enter inserts a line break, while the configured modifier plus Enter submits.</summary>
    SubmitWhenModified
}
