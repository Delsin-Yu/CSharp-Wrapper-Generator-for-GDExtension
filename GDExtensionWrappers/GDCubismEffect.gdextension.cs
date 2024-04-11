using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismEffect : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismEffect");

    public GDCubismEffect Construct(RefCounted backing) =>
        new GDCubismEffect(backing);

    protected readonly RefCounted _backing;

    public GDCubismEffect() => _backing = Construct();

    private GDCubismEffect(RefCounted backing) => _backing = backing;

    public void Dispose() => _backing.Dispose();

    public bool Active
    {
        get => (bool)_backing.Get("active");
        set => _backing.Set("active", Variant.From(value));
    }

}