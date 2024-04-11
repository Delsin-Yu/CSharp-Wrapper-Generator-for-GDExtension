using Godot;

namespace GDExtension.Wrappers;

public partial class GDCubismParameter : GDCubismValueAbs
{
    public double MinimumValue
    {
        get => (double)Get("minimum_value");
        set => Set("minimum_value", Variant.From(value));
    }
    public double MaximumValue
    {
        get => (double)Get("maximum_value");
        set => Set("maximum_value", Variant.From(value));
    }
    public double DefaultValue
    {
        get => (double)Get("default_value");
        set => Set("default_value", Variant.From(value));
    }
}