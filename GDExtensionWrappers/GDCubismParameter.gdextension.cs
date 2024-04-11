using Godot;

namespace GDExtension.ResourceWrappers;

public class GDCubismParameter : GDCubismValueAbs
{

    public GDCubismParameter(Resource backing) : base(backing) { }

    public double MinimumValue
    {
        get => (double)_backing.Get("minimum_value");
        set => _backing.Set("minimum_value", Variant.From(value));
    }

    public double MaximumValue
    {
        get => (double)_backing.Get("maximum_value");
        set => _backing.Set("maximum_value", Variant.From(value));
    }

    public double DefaultValue
    {
        get => (double)_backing.Get("default_value");
        set => _backing.Set("default_value", Variant.From(value));
    }

}