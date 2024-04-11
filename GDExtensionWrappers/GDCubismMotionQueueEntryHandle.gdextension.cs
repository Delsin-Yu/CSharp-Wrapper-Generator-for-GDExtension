using Godot;

namespace GDExtension.ResourcesWrappers;

public class GDCubismMotionQueueEntryHandle
{
    protected readonly Resource _backing;

    public GDCubismMotionQueueEntryHandle(Resource backing)
    {
        _backing = backing;
    }

    public int Error
    {
        get => (int)_backing.Get("error");
        set => _backing.Set("error", Variant.From(value));
    }

}