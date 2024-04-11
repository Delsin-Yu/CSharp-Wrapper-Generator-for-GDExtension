using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltConeTwistJoint3D : JoltJoint3D
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltConeTwistJoint3D");

    public bool SwingLimitEnabled
    {
        get => (bool)_backing.Get("swing_limit_enabled");
        set => _backing.Set("swing_limit_enabled", Variant.From(value));
    }

    public float SwingLimitSpan
    {
        get => (float)_backing.Get("swing_limit_span");
        set => _backing.Set("swing_limit_span", Variant.From(value));
    }

    public bool TwistLimitEnabled
    {
        get => (bool)_backing.Get("twist_limit_enabled");
        set => _backing.Set("twist_limit_enabled", Variant.From(value));
    }

    public float TwistLimitSpan
    {
        get => (float)_backing.Get("twist_limit_span");
        set => _backing.Set("twist_limit_span", Variant.From(value));
    }

    public bool SwingMotorEnabled
    {
        get => (bool)_backing.Get("swing_motor_enabled");
        set => _backing.Set("swing_motor_enabled", Variant.From(value));
    }

    public float SwingMotorTargetVelocityY
    {
        get => (float)_backing.Get("swing_motor_target_velocity_y");
        set => _backing.Set("swing_motor_target_velocity_y", Variant.From(value));
    }

    public float SwingMotorTargetVelocityZ
    {
        get => (float)_backing.Get("swing_motor_target_velocity_z");
        set => _backing.Set("swing_motor_target_velocity_z", Variant.From(value));
    }

    public float SwingMotorMaxTorque
    {
        get => (float)_backing.Get("swing_motor_max_torque");
        set => _backing.Set("swing_motor_max_torque", Variant.From(value));
    }

    public bool TwistMotorEnabled
    {
        get => (bool)_backing.Get("twist_motor_enabled");
        set => _backing.Set("twist_motor_enabled", Variant.From(value));
    }

    public float TwistMotorTargetVelocity
    {
        get => (float)_backing.Get("twist_motor_target_velocity");
        set => _backing.Set("twist_motor_target_velocity", Variant.From(value));
    }

    public float TwistMotorMaxTorque
    {
        get => (float)_backing.Get("twist_motor_max_torque");
        set => _backing.Set("twist_motor_max_torque", Variant.From(value));
    }

    public float GetAppliedForce() => _backing.Call("get_applied_force").As<float>();

    public float GetAppliedTorque() => _backing.Call("get_applied_torque").As<float>();

}