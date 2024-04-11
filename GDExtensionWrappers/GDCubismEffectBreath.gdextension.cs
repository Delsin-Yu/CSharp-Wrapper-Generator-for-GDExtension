using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismEffectBreath : GDCubismEffect
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismEffectBreath");

}