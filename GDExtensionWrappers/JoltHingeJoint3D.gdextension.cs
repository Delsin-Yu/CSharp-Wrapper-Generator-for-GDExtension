using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltHingeJoint3D : JoltJoint3D
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltHingeJoint3D");

    public bool LimitEnabled
    {
        get => (bool)_backing.Get("limit_enabled");
        set => _backing.Set("limit_enabled", Variant.From(value));
    }

    public float LimitUpper
    {
        get => (float)_backing.Get("limit_upper");
        set => _backing.Set("limit_upper", Variant.From(value));
    }

    public float LimitLower
    {
        get => (float)_backing.Get("limit_lower");
        set => _backing.Set("limit_lower", Variant.From(value));
    }

    public bool LimitSpringEnabled
    {
        get => (bool)_backing.Get("limit_spring_enabled");
        set => _backing.Set("limit_spring_enabled", Variant.From(value));
    }

    public float LimitSpringFrequency
    {
        get => (float)_backing.Get("limit_spring_frequency");
        set => _backing.Set("limit_spring_frequency", Variant.From(value));
    }

    public float LimitSpringDamping
    {
        get => (float)_backing.Get("limit_spring_damping");
        set => _backing.Set("limit_spring_damping", Variant.From(value));
    }

    public bool MotorEnabled
    {
        get => (bool)_backing.Get("motor_enabled");
        set => _backing.Set("motor_enabled", Variant.From(value));
    }

    public float MotorTargetVelocity
    {
        get => (float)_backing.Get("motor_target_velocity");
        set => _backing.Set("motor_target_velocity", Variant.From(value));
    }

    public float MotorMaxTorque
    {
        get => (float)_backing.Get("motor_max_torque");
        set => _backing.Set("motor_max_torque", Variant.From(value));
    }

    public float GetAppliedForce() => _backing.Call("get_applied_force").As<float>();

    public float GetAppliedTorque() => _backing.Call("get_applied_torque").As<float>();

}