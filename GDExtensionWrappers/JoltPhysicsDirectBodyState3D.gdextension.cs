using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltPhysicsDirectBodyState3D : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltPhysicsDirectBodyState3D");

    protected readonly RefCounted _backing;

    public JoltPhysicsDirectBodyState3D() => _backing = Construct();

    public void Dispose() => _backing.Dispose();

}