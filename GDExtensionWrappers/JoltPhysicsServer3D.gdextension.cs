using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltPhysicsServer3D : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltPhysicsServer3D");

    protected readonly RefCounted _backing;

    public JoltPhysicsServer3D() => _backing = Construct();

    public void Dispose() => _backing.Dispose();

}