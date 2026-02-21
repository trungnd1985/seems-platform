namespace Seems.Application.Common.Interfaces;

public interface IPreviewContext
{
    /// <summary>
    /// True when the current request carries a valid X-Preview-Token header.
    /// Handlers use this to decide whether to include draft content.
    /// </summary>
    bool IsPreview { get; }
}
