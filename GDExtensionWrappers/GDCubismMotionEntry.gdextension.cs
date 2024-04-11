using Godot;

namespace GDExtension.ResourceWrappers;

public class GDCubismMotionEntry
{
    protected readonly Resource _backing;

    public GDCubismMotionEntry(Resource backing)
    {
        _backing = backing;
    }

}