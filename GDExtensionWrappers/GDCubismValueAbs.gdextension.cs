using Godot;

namespace GDExtension.ResourceWrappers;

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

    public double Value
    {
        get => (double)_backing.Get("value");
        set => _backing.Set("value", Variant.From(value));
    }

}