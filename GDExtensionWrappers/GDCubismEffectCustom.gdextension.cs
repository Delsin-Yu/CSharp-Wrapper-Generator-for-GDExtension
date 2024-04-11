using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismEffectCustom : GDCubismEffect
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismEffectCustom");

}