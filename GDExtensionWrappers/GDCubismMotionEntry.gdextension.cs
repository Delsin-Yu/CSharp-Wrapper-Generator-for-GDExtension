using Godot;

namespace GDExtension.ResourcesWrappers;

public class GDCubismMotionEntry
{
    protected readonly Resource _backing;

    public GDCubismMotionEntry(Resource backing)
    {
        _backing = backing;
    }

}