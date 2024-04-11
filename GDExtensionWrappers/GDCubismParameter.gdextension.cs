using Godot;

namespace GDExtension.ResourcesWrappers;

public class GDCubismParameter : GDCubismValueAbs
{

    public GDCubismParameter(Resource backing) : base(backing) { }

    public float MinimumValue
    {
        get => (float)_backing.Get("minimum_value");
        set => _backing.Set("minimum_value", Variant.From(value));
    }

    public float MaximumValue
    {
        get => (float)_backing.Get("maximum_value");
        set => _backing.Set("maximum_value", Variant.From(value));
    }

    public float DefaultValue
    {
        get => (float)_backing.Get("default_value");
        set => _backing.Set("default_value", Variant.From(value));
    }

}