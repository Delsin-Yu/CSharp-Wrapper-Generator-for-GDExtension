using Godot;

namespace GDExtension.ResourcesWrappers;

public class GDCubismValueAbs
{
    protected readonly Resource _backing;

    public GDCubismValueAbs(Resource backing)
    {
        _backing = backing;
    }

    public string Id
    {
        get => (string)_backing.Get("id");
        set => _backing.Set("id", Variant.From(value));
    }

    public float Value
    {
        get => (float)_backing.Get("value");
        set => _backing.Set("value", Variant.From(value));
    }

}