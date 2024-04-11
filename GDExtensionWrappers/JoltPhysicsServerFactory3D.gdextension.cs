using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltPhysicsServerFactory3D : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltPhysicsServerFactory3D");

    protected readonly RefCounted _backing;

    public JoltPhysicsServerFactory3D() => _backing = Construct();

    public void Dispose() => _backing.Dispose();

}