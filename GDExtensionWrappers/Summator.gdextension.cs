using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class Summator : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("Summator");

    protected readonly RefCounted _backing;

    public Summator() => _backing = Construct();

    public void Dispose() => _backing.Dispose();

}