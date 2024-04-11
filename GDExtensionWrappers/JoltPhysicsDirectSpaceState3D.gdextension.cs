using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltPhysicsDirectSpaceState3D : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltPhysicsDirectSpaceState3D");

    protected readonly RefCounted _backing;

    public JoltPhysicsDirectSpaceState3D() => _backing = Construct();

    public void Dispose() => _backing.Dispose();

}