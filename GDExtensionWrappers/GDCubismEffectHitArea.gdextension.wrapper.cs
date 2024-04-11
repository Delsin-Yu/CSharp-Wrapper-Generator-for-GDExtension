using Godot;

namespace GDExtension.Wrappers;

public partial class GDCubismEffectHitArea : GDCubismEffect
{
    public bool Monitoring
    {
        get => (bool)Get("monitoring");
        set => Set("monitoring", Variant.From(value));
    }
}