using Godot;

namespace GDExtension.Wrappers;

public partial class GDCubismEffect : Node
{
    public bool Active
    {
        get => (bool)Get("active");
        set => Set("active", Variant.From(value));
    }
}