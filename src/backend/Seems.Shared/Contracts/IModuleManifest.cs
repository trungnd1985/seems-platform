namespace Seems.Shared.Contracts;

public interface IModuleManifest
{
    string ModuleKey { get; }
    string Name { get; }
    string Version { get; }
    IReadOnlyList<SlotDescriptor> Slots { get; }
}
