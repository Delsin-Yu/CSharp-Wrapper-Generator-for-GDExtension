using Godot;

namespace GDExtension.Wrappers;

public partial class GDCubismMotionQueueEntryHandle : Resource
{
    public long Error
    {
        get => (long)Get("error");
        set => Set("error", Variant.From(value));
    }
}