namespace Seems.Shared.Contracts;

public record SlotDescriptor(
    string Key,
    string Label,
    string[]? AllowedTypes = null,
    int? MaxItems = null
);
