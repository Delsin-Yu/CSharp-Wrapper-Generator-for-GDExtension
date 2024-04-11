using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismEffectTargetPoint : GDCubismEffect
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismEffectTargetPoint");

    public string HeadAngleX
    {
        get => (string)_backing.Get("head_angle_x");
        set => _backing.Set("head_angle_x", Variant.From(value));
    }

    public string HeadAngleY
    {
        get => (string)_backing.Get("head_angle_y");
        set => _backing.Set("head_angle_y", Variant.From(value));
    }

    public string HeadAngleZ
    {
        get => (string)_backing.Get("head_angle_z");
        set => _backing.Set("head_angle_z", Variant.From(value));
    }

    public float HeadRange
    {
        get => (float)_backing.Get("head_range");
        set => _backing.Set("head_range", Variant.From(value));
    }

    public string BodyAngleX
    {
        get => (string)_backing.Get("body_angle_x");
        set => _backing.Set("body_angle_x", Variant.From(value));
    }

    public float BodyRange
    {
        get => (float)_backing.Get("body_range");
        set => _backing.Set("body_range", Variant.From(value));
    }

    public string EyesBallX
    {
        get => (string)_backing.Get("eyes_ball_x");
        set => _backing.Set("eyes_ball_x", Variant.From(value));
    }

    public string EyesBallY
    {
        get => (string)_backing.Get("eyes_ball_y");
        set => _backing.Set("eyes_ball_y", Variant.From(value));
    }

    public float EyesRange
    {
        get => (float)_backing.Get("eyes_range");
        set => _backing.Set("eyes_range", Variant.From(value));
    }

    public void SetTarget(Vector2 target) => _backing.Call("set_target", target);

    public Vector2 GetTarget() => _backing.Call("get_target").As<Vector2>();

}