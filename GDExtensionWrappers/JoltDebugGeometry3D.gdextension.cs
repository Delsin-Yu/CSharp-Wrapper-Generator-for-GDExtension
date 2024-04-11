using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltDebugGeometry3D : GeometryInstance3D
{
    public bool DrawBodies
    {
        get => (bool)Get("draw_bodies");
        set => Set("draw_bodies", Variant.From(value));
    }

    public bool DrawShapes
    {
        get => (bool)Get("draw_shapes");
        set => Set("draw_shapes", Variant.From(value));
    }

    public bool DrawConstraints
    {
        get => (bool)Get("draw_constraints");
        set => Set("draw_constraints", Variant.From(value));
    }

    public bool DrawBoundingBoxes
    {
        get => (bool)Get("draw_bounding_boxes");
        set => Set("draw_bounding_boxes", Variant.From(value));
    }

    public bool DrawCentersOfMass
    {
        get => (bool)Get("draw_centers_of_mass");
        set => Set("draw_centers_of_mass", Variant.From(value));
    }

    public bool DrawTransforms
    {
        get => (bool)Get("draw_transforms");
        set => Set("draw_transforms", Variant.From(value));
    }

    public bool DrawVelocities
    {
        get => (bool)Get("draw_velocities");
        set => Set("draw_velocities", Variant.From(value));
    }

    public bool DrawTriangleOutlines
    {
        get => (bool)Get("draw_triangle_outlines");
        set => Set("draw_triangle_outlines", Variant.From(value));
    }

    public bool DrawConstraintReferenceFrames
    {
        get => (bool)Get("draw_constraint_reference_frames");
        set => Set("draw_constraint_reference_frames", Variant.From(value));
    }

    public bool DrawConstraintLimits
    {
        get => (bool)Get("draw_constraint_limits");
        set => Set("draw_constraint_limits", Variant.From(value));
    }

    public bool DrawAsWireframe
    {
        get => (bool)Get("draw_as_wireframe");
        set => Set("draw_as_wireframe", Variant.From(value));
    }

    public int DrawWithColorScheme
    {
        get => (int)Get("draw_with_color_scheme");
        set => Set("draw_with_color_scheme", Variant.From(value));
    }

    public bool MaterialDepthTest
    {
        get => (bool)Get("material_depth_test");
        set => Set("material_depth_test", Variant.From(value));
    }

}