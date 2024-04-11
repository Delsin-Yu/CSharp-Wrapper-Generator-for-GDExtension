using Godot;

namespace GDExtension.NodeWrappers;

public partial class GDCubismEffectHitArea : GDCubismEffect
{
    public bool Monitoring
    {
        get => (bool)Get("monitoring");
        set => Set("monitoring", Variant.From(value));
    }

}