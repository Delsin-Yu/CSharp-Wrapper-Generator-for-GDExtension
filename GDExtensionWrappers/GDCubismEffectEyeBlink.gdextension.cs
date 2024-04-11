using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismEffectEyeBlink : GDCubismEffect
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismEffectEyeBlink");

}