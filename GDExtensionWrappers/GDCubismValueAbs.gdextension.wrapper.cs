using Godot;

namespace GDExtension.Wrappers;

public partial class GDCubismValueAbs : Resource
{
    public string Id
    {
        get => (string)Get("id");
        set => Set("id", Variant.From(value));
    }
    public double Value
    {
        get => (double)Get("value");
        set => Set("value", Variant.From(value));
    }
}