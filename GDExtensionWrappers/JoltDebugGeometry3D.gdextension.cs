using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltDebugGeometry3D : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltDebugGeometry3D");

    public JoltDebugGeometry3D Construct(RefCounted backing) =>
        new JoltDebugGeometry3D(backing);

    protected readonly RefCounted _backing;

    public JoltDebugGeometry3D() => _backing = Construct();

    private JoltDebugGeometry3D(RefCounted backing) => _backing = backing;

    public void Dispose() => _backing.Dispose();

    public bool DrawBodies
    {
        get => (bool)_backing.Get("draw_bodies");
        set => _backing.Set("draw_bodies", Variant.From(value));
    }

    public bool DrawShapes
    {
        get => (bool)_backing.Get("draw_shapes");
        set => _backing.Set("draw_shapes", Variant.From(value));
    }

    public bool DrawConstraints
    {
        get => (bool)_backing.Get("draw_constraints");
        set => _backing.Set("draw_constraints", Variant.From(value));
    }

    public bool DrawBoundingBoxes
    {
        get => (bool)_backing.Get("draw_bounding_boxes");
        set => _backing.Set("draw_bounding_boxes", Variant.From(value));
    }

    public bool DrawCentersOfMass
    {
        get => (bool)_backing.Get("draw_centers_of_mass");
        set => _backing.Set("draw_centers_of_mass", Variant.From(value));
    }

    public bool DrawTransforms
    {
        get => (bool)_backing.Get("draw_transforms");
        set => _backing.Set("draw_transforms", Variant.From(value));
    }

    public bool DrawVelocities
    {
        get => (bool)_backing.Get("draw_velocities");
        set => _backing.Set("draw_velocities", Variant.From(value));
    }

    public bool DrawTriangleOutlines
    {
        get => (bool)_backing.Get("draw_triangle_outlines");
        set => _backing.Set("draw_triangle_outlines", Variant.From(value));
    }

    public bool DrawConstraintReferenceFrames
    {
        get => (bool)_backing.Get("draw_constraint_reference_frames");
        set => _backing.Set("draw_constraint_reference_frames", Variant.From(value));
    }

    public bool DrawConstraintLimits
    {
        get => (bool)_backing.Get("draw_constraint_limits");
        set => _backing.Set("draw_constraint_limits", Variant.From(value));
    }

    public bool DrawAsWireframe
    {
        get => (bool)_backing.Get("draw_as_wireframe");
        set => _backing.Set("draw_as_wireframe", Variant.From(value));
    }

    public int DrawWithColorScheme
    {
        get => (int)_backing.Get("draw_with_color_scheme");
        set => _backing.Set("draw_with_color_scheme", Variant.From(value));
    }

    public bool MaterialDepthTest
    {
        get => (bool)_backing.Get("material_depth_test");
        set => _backing.Set("material_depth_test", Variant.From(value));
    }

}