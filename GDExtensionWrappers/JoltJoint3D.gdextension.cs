using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltJoint3D : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltJoint3D");

    public JoltJoint3D Construct(RefCounted backing) =>
        new JoltJoint3D(backing);

    protected readonly RefCounted _backing;

    public JoltJoint3D() => _backing = Construct();

    private JoltJoint3D(RefCounted backing) => _backing = backing;

    public void Dispose() => _backing.Dispose();

    public NodePath NodeA
    {
        get => (NodePath)_backing.Get("node_a");
        set => _backing.Set("node_a", Variant.From(value));
    }

    public NodePath NodeB
    {
        get => (NodePath)_backing.Get("node_b");
        set => _backing.Set("node_b", Variant.From(value));
    }

    public bool Enabled
    {
        get => (bool)_backing.Get("enabled");
        set => _backing.Set("enabled", Variant.From(value));
    }

    public bool ExcludeNodesFromCollision
    {
        get => (bool)_backing.Get("exclude_nodes_from_collision");
        set => _backing.Set("exclude_nodes_from_collision", Variant.From(value));
    }

    public int SolverVelocityIterations
    {
        get => (int)_backing.Get("solver_velocity_iterations");
        set => _backing.Set("solver_velocity_iterations", Variant.From(value));
    }

    public int SolverPositionIterations
    {
        get => (int)_backing.Get("solver_position_iterations");
        set => _backing.Set("solver_position_iterations", Variant.From(value));
    }

    public void BodyExitingTree() => _backing.Call("body_exiting_tree");

}