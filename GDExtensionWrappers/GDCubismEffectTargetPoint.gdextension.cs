using Godot;

namespace GDExtension.NodeWrappers;

public partial class GDCubismEffectTargetPoint : GDCubismEffect
{
    public string HeadAngleX
    {
        get => (string)Get("head_angle_x");
        set => Set("head_angle_x", Variant.From(value));
    }

    public string HeadAngleY
    {
        get => (string)Get("head_angle_y");
        set => Set("head_angle_y", Variant.From(value));
    }

    public string HeadAngleZ
    {
        get => (string)Get("head_angle_z");
        set => Set("head_angle_z", Variant.From(value));
    }

    public double HeadRange
    {
        get => (double)Get("head_range");
        set => Set("head_range", Variant.From(value));
    }

    public string BodyAngleX
    {
        get => (string)Get("body_angle_x");
        set => Set("body_angle_x", Variant.From(value));
    }

    public double BodyRange
    {
        get => (double)Get("body_range");
        set => Set("body_range", Variant.From(value));
    }

    public string EyesBallX
    {
        get => (string)Get("eyes_ball_x");
        set => Set("eyes_ball_x", Variant.From(value));
    }

    public string EyesBallY
    {
        get => (string)Get("eyes_ball_y");
        set => Set("eyes_ball_y", Variant.From(value));
    }

    public double EyesRange
    {
        get => (double)Get("eyes_range");
        set => Set("eyes_range", Variant.From(value));
    }

}