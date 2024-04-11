using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltHingeJoint3D : JoltJoint3D
{
    public bool LimitEnabled
    {
        get => (bool)Get("limit_enabled");
        set => Set("limit_enabled", Variant.From(value));
    }

    public float LimitUpper
    {
        get => (float)Get("limit_upper");
        set => Set("limit_upper", Variant.From(value));
    }

    public float LimitLower
    {
        get => (float)Get("limit_lower");
        set => Set("limit_lower", Variant.From(value));
    }

    public bool LimitSpringEnabled
    {
        get => (bool)Get("limit_spring_enabled");
        set => Set("limit_spring_enabled", Variant.From(value));
    }

    public float LimitSpringFrequency
    {
        get => (float)Get("limit_spring_frequency");
        set => Set("limit_spring_frequency", Variant.From(value));
    }

    public float LimitSpringDamping
    {
        get => (float)Get("limit_spring_damping");
        set => Set("limit_spring_damping", Variant.From(value));
    }

    public bool MotorEnabled
    {
        get => (bool)Get("motor_enabled");
        set => Set("motor_enabled", Variant.From(value));
    }

    public float MotorTargetVelocity
    {
        get => (float)Get("motor_target_velocity");
        set => Set("motor_target_velocity", Variant.From(value));
    }

    public float MotorMaxTorque
    {
        get => (float)Get("motor_max_torque");
        set => Set("motor_max_torque", Variant.From(value));
    }

}