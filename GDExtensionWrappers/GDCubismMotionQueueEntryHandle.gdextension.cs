using Godot;

namespace GDExtension.ResourceWrappers;

public class GDCubismMotionQueueEntryHandle
{
    protected readonly Resource _backing;

    public GDCubismMotionQueueEntryHandle(Resource backing)
    {
        _backing = backing;
    }

    public long Error
    {
        get => (long)_backing.Get("error");
        set => _backing.Set("error", Variant.From(value));
    }

}