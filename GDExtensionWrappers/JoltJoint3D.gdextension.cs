using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltJoint3D : Node3D
{
    public Godot.NodePath NodeA
    {
        get => (Godot.NodePath)Get("node_a");
        set => Set("node_a", Variant.From(value));
    }

    public Godot.NodePath NodeB
    {
        get => (Godot.NodePath)Get("node_b");
        set => Set("node_b", Variant.From(value));
    }

    public bool Enabled
    {
        get => (bool)Get("enabled");
        set => Set("enabled", Variant.From(value));
    }

    public bool ExcludeNodesFromCollision
    {
        get => (bool)Get("exclude_nodes_from_collision");
        set => Set("exclude_nodes_from_collision", Variant.From(value));
    }

    public int SolverVelocityIterations
    {
        get => (int)Get("solver_velocity_iterations");
        set => Set("solver_velocity_iterations", Variant.From(value));
    }

    public int SolverPositionIterations
    {
        get => (int)Get("solver_position_iterations");
        set => Set("solver_position_iterations", Variant.From(value));
    }

}