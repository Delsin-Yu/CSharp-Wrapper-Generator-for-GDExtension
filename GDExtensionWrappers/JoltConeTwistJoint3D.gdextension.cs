using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltConeTwistJoint3D : JoltJoint3D
{
    public bool SwingLimitEnabled
    {
        get => (bool)Get("swing_limit_enabled");
        set => Set("swing_limit_enabled", Variant.From(value));
    }

    public float SwingLimitSpan
    {
        get => (float)Get("swing_limit_span");
        set => Set("swing_limit_span", Variant.From(value));
    }

    public bool TwistLimitEnabled
    {
        get => (bool)Get("twist_limit_enabled");
        set => Set("twist_limit_enabled", Variant.From(value));
    }

    public float TwistLimitSpan
    {
        get => (float)Get("twist_limit_span");
        set => Set("twist_limit_span", Variant.From(value));
    }

    public bool SwingMotorEnabled
    {
        get => (bool)Get("swing_motor_enabled");
        set => Set("swing_motor_enabled", Variant.From(value));
    }

    public float SwingMotorTargetVelocityY
    {
        get => (float)Get("swing_motor_target_velocity_y");
        set => Set("swing_motor_target_velocity_y", Variant.From(value));
    }

    public float SwingMotorTargetVelocityZ
    {
        get => (float)Get("swing_motor_target_velocity_z");
        set => Set("swing_motor_target_velocity_z", Variant.From(value));
    }

    public float SwingMotorMaxTorque
    {
        get => (float)Get("swing_motor_max_torque");
        set => Set("swing_motor_max_torque", Variant.From(value));
    }

    public bool TwistMotorEnabled
    {
        get => (bool)Get("twist_motor_enabled");
        set => Set("twist_motor_enabled", Variant.From(value));
    }

    public float TwistMotorTargetVelocity
    {
        get => (float)Get("twist_motor_target_velocity");
        set => Set("twist_motor_target_velocity", Variant.From(value));
    }

    public float TwistMotorMaxTorque
    {
        get => (float)Get("twist_motor_max_torque");
        set => Set("twist_motor_max_torque", Variant.From(value));
    }

}