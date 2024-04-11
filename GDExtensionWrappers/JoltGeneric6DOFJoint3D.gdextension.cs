using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltGeneric6DOFJoint3D : JoltJoint3D
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltGeneric6DOFJoint3D");

    public bool LinearLimitXEnabled
    {
        get => (bool)_backing.Get("linear_limit_x/enabled");
        set => _backing.Set("linear_limit_x/enabled", Variant.From(value));
    }

    public float LinearLimitXUpper
    {
        get => (float)_backing.Get("linear_limit_x/upper");
        set => _backing.Set("linear_limit_x/upper", Variant.From(value));
    }

    public float LinearLimitXLower
    {
        get => (float)_backing.Get("linear_limit_x/lower");
        set => _backing.Set("linear_limit_x/lower", Variant.From(value));
    }

    public bool LinearLimitYEnabled
    {
        get => (bool)_backing.Get("linear_limit_y/enabled");
        set => _backing.Set("linear_limit_y/enabled", Variant.From(value));
    }

    public float LinearLimitYUpper
    {
        get => (float)_backing.Get("linear_limit_y/upper");
        set => _backing.Set("linear_limit_y/upper", Variant.From(value));
    }

    public float LinearLimitYLower
    {
        get => (float)_backing.Get("linear_limit_y/lower");
        set => _backing.Set("linear_limit_y/lower", Variant.From(value));
    }

    public bool LinearLimitZEnabled
    {
        get => (bool)_backing.Get("linear_limit_z/enabled");
        set => _backing.Set("linear_limit_z/enabled", Variant.From(value));
    }

    public float LinearLimitZUpper
    {
        get => (float)_backing.Get("linear_limit_z/upper");
        set => _backing.Set("linear_limit_z/upper", Variant.From(value));
    }

    public float LinearLimitZLower
    {
        get => (float)_backing.Get("linear_limit_z/lower");
        set => _backing.Set("linear_limit_z/lower", Variant.From(value));
    }

    public bool LinearLimitSpringXEnabled
    {
        get => (bool)_backing.Get("linear_limit_spring_x/enabled");
        set => _backing.Set("linear_limit_spring_x/enabled", Variant.From(value));
    }

    public float LinearLimitSpringXFrequency
    {
        get => (float)_backing.Get("linear_limit_spring_x/frequency");
        set => _backing.Set("linear_limit_spring_x/frequency", Variant.From(value));
    }

    public float LinearLimitSpringXDamping
    {
        get => (float)_backing.Get("linear_limit_spring_x/damping");
        set => _backing.Set("linear_limit_spring_x/damping", Variant.From(value));
    }

    public bool LinearLimitSpringYEnabled
    {
        get => (bool)_backing.Get("linear_limit_spring_y/enabled");
        set => _backing.Set("linear_limit_spring_y/enabled", Variant.From(value));
    }

    public float LinearLimitSpringYFrequency
    {
        get => (float)_backing.Get("linear_limit_spring_y/frequency");
        set => _backing.Set("linear_limit_spring_y/frequency", Variant.From(value));
    }

    public float LinearLimitSpringYDamping
    {
        get => (float)_backing.Get("linear_limit_spring_y/damping");
        set => _backing.Set("linear_limit_spring_y/damping", Variant.From(value));
    }

    public bool LinearLimitSpringZEnabled
    {
        get => (bool)_backing.Get("linear_limit_spring_z/enabled");
        set => _backing.Set("linear_limit_spring_z/enabled", Variant.From(value));
    }

    public float LinearLimitSpringZFrequency
    {
        get => (float)_backing.Get("linear_limit_spring_z/frequency");
        set => _backing.Set("linear_limit_spring_z/frequency", Variant.From(value));
    }

    public float LinearLimitSpringZDamping
    {
        get => (float)_backing.Get("linear_limit_spring_z/damping");
        set => _backing.Set("linear_limit_spring_z/damping", Variant.From(value));
    }

    public bool LinearMotorXEnabled
    {
        get => (bool)_backing.Get("linear_motor_x/enabled");
        set => _backing.Set("linear_motor_x/enabled", Variant.From(value));
    }

    public float LinearMotorXTargetVelocity
    {
        get => (float)_backing.Get("linear_motor_x/target_velocity");
        set => _backing.Set("linear_motor_x/target_velocity", Variant.From(value));
    }

    public float LinearMotorXMaxForce
    {
        get => (float)_backing.Get("linear_motor_x/max_force");
        set => _backing.Set("linear_motor_x/max_force", Variant.From(value));
    }

    public bool LinearMotorYEnabled
    {
        get => (bool)_backing.Get("linear_motor_y/enabled");
        set => _backing.Set("linear_motor_y/enabled", Variant.From(value));
    }

    public float LinearMotorYTargetVelocity
    {
        get => (float)_backing.Get("linear_motor_y/target_velocity");
        set => _backing.Set("linear_motor_y/target_velocity", Variant.From(value));
    }

    public float LinearMotorYMaxForce
    {
        get => (float)_backing.Get("linear_motor_y/max_force");
        set => _backing.Set("linear_motor_y/max_force", Variant.From(value));
    }

    public bool LinearMotorZEnabled
    {
        get => (bool)_backing.Get("linear_motor_z/enabled");
        set => _backing.Set("linear_motor_z/enabled", Variant.From(value));
    }

    public float LinearMotorZTargetVelocity
    {
        get => (float)_backing.Get("linear_motor_z/target_velocity");
        set => _backing.Set("linear_motor_z/target_velocity", Variant.From(value));
    }

    public float LinearMotorZMaxForce
    {
        get => (float)_backing.Get("linear_motor_z/max_force");
        set => _backing.Set("linear_motor_z/max_force", Variant.From(value));
    }

    public bool LinearSpringXEnabled
    {
        get => (bool)_backing.Get("linear_spring_x/enabled");
        set => _backing.Set("linear_spring_x/enabled", Variant.From(value));
    }

    public float LinearSpringXFrequency
    {
        get => (float)_backing.Get("linear_spring_x/frequency");
        set => _backing.Set("linear_spring_x/frequency", Variant.From(value));
    }

    public float LinearSpringXDamping
    {
        get => (float)_backing.Get("linear_spring_x/damping");
        set => _backing.Set("linear_spring_x/damping", Variant.From(value));
    }

    public float LinearSpringXEquilibriumPoint
    {
        get => (float)_backing.Get("linear_spring_x/equilibrium_point");
        set => _backing.Set("linear_spring_x/equilibrium_point", Variant.From(value));
    }

    public bool LinearSpringYEnabled
    {
        get => (bool)_backing.Get("linear_spring_y/enabled");
        set => _backing.Set("linear_spring_y/enabled", Variant.From(value));
    }

    public float LinearSpringYFrequency
    {
        get => (float)_backing.Get("linear_spring_y/frequency");
        set => _backing.Set("linear_spring_y/frequency", Variant.From(value));
    }

    public float LinearSpringYDamping
    {
        get => (float)_backing.Get("linear_spring_y/damping");
        set => _backing.Set("linear_spring_y/damping", Variant.From(value));
    }

    public float LinearSpringYEquilibriumPoint
    {
        get => (float)_backing.Get("linear_spring_y/equilibrium_point");
        set => _backing.Set("linear_spring_y/equilibrium_point", Variant.From(value));
    }

    public bool LinearSpringZEnabled
    {
        get => (bool)_backing.Get("linear_spring_z/enabled");
        set => _backing.Set("linear_spring_z/enabled", Variant.From(value));
    }

    public float LinearSpringZFrequency
    {
        get => (float)_backing.Get("linear_spring_z/frequency");
        set => _backing.Set("linear_spring_z/frequency", Variant.From(value));
    }

    public float LinearSpringZDamping
    {
        get => (float)_backing.Get("linear_spring_z/damping");
        set => _backing.Set("linear_spring_z/damping", Variant.From(value));
    }

    public float LinearSpringZEquilibriumPoint
    {
        get => (float)_backing.Get("linear_spring_z/equilibrium_point");
        set => _backing.Set("linear_spring_z/equilibrium_point", Variant.From(value));
    }

    public bool AngularLimitXEnabled
    {
        get => (bool)_backing.Get("angular_limit_x/enabled");
        set => _backing.Set("angular_limit_x/enabled", Variant.From(value));
    }

    public float AngularLimitXUpper
    {
        get => (float)_backing.Get("angular_limit_x/upper");
        set => _backing.Set("angular_limit_x/upper", Variant.From(value));
    }

    public float AngularLimitXLower
    {
        get => (float)_backing.Get("angular_limit_x/lower");
        set => _backing.Set("angular_limit_x/lower", Variant.From(value));
    }

    public bool AngularLimitYEnabled
    {
        get => (bool)_backing.Get("angular_limit_y/enabled");
        set => _backing.Set("angular_limit_y/enabled", Variant.From(value));
    }

    public float AngularLimitYUpper
    {
        get => (float)_backing.Get("angular_limit_y/upper");
        set => _backing.Set("angular_limit_y/upper", Variant.From(value));
    }

    public float AngularLimitYLower
    {
        get => (float)_backing.Get("angular_limit_y/lower");
        set => _backing.Set("angular_limit_y/lower", Variant.From(value));
    }

    public bool AngularLimitZEnabled
    {
        get => (bool)_backing.Get("angular_limit_z/enabled");
        set => _backing.Set("angular_limit_z/enabled", Variant.From(value));
    }

    public float AngularLimitZUpper
    {
        get => (float)_backing.Get("angular_limit_z/upper");
        set => _backing.Set("angular_limit_z/upper", Variant.From(value));
    }

    public float AngularLimitZLower
    {
        get => (float)_backing.Get("angular_limit_z/lower");
        set => _backing.Set("angular_limit_z/lower", Variant.From(value));
    }

    public bool AngularMotorXEnabled
    {
        get => (bool)_backing.Get("angular_motor_x/enabled");
        set => _backing.Set("angular_motor_x/enabled", Variant.From(value));
    }

    public float AngularMotorXTargetVelocity
    {
        get => (float)_backing.Get("angular_motor_x/target_velocity");
        set => _backing.Set("angular_motor_x/target_velocity", Variant.From(value));
    }

    public float AngularMotorXMaxTorque
    {
        get => (float)_backing.Get("angular_motor_x/max_torque");
        set => _backing.Set("angular_motor_x/max_torque", Variant.From(value));
    }

    public bool AngularMotorYEnabled
    {
        get => (bool)_backing.Get("angular_motor_y/enabled");
        set => _backing.Set("angular_motor_y/enabled", Variant.From(value));
    }

    public float AngularMotorYTargetVelocity
    {
        get => (float)_backing.Get("angular_motor_y/target_velocity");
        set => _backing.Set("angular_motor_y/target_velocity", Variant.From(value));
    }

    public float AngularMotorYMaxTorque
    {
        get => (float)_backing.Get("angular_motor_y/max_torque");
        set => _backing.Set("angular_motor_y/max_torque", Variant.From(value));
    }

    public bool AngularMotorZEnabled
    {
        get => (bool)_backing.Get("angular_motor_z/enabled");
        set => _backing.Set("angular_motor_z/enabled", Variant.From(value));
    }

    public float AngularMotorZTargetVelocity
    {
        get => (float)_backing.Get("angular_motor_z/target_velocity");
        set => _backing.Set("angular_motor_z/target_velocity", Variant.From(value));
    }

    public float AngularMotorZMaxTorque
    {
        get => (float)_backing.Get("angular_motor_z/max_torque");
        set => _backing.Set("angular_motor_z/max_torque", Variant.From(value));
    }

    public bool AngularSpringXEnabled
    {
        get => (bool)_backing.Get("angular_spring_x/enabled");
        set => _backing.Set("angular_spring_x/enabled", Variant.From(value));
    }

    public float AngularSpringXFrequency
    {
        get => (float)_backing.Get("angular_spring_x/frequency");
        set => _backing.Set("angular_spring_x/frequency", Variant.From(value));
    }

    public float AngularSpringXDamping
    {
        get => (float)_backing.Get("angular_spring_x/damping");
        set => _backing.Set("angular_spring_x/damping", Variant.From(value));
    }

    public float AngularSpringXEquilibriumPoint
    {
        get => (float)_backing.Get("angular_spring_x/equilibrium_point");
        set => _backing.Set("angular_spring_x/equilibrium_point", Variant.From(value));
    }

    public bool AngularSpringYEnabled
    {
        get => (bool)_backing.Get("angular_spring_y/enabled");
        set => _backing.Set("angular_spring_y/enabled", Variant.From(value));
    }

    public float AngularSpringYFrequency
    {
        get => (float)_backing.Get("angular_spring_y/frequency");
        set => _backing.Set("angular_spring_y/frequency", Variant.From(value));
    }

    public float AngularSpringYDamping
    {
        get => (float)_backing.Get("angular_spring_y/damping");
        set => _backing.Set("angular_spring_y/damping", Variant.From(value));
    }

    public float AngularSpringYEquilibriumPoint
    {
        get => (float)_backing.Get("angular_spring_y/equilibrium_point");
        set => _backing.Set("angular_spring_y/equilibrium_point", Variant.From(value));
    }

    public bool AngularSpringZEnabled
    {
        get => (bool)_backing.Get("angular_spring_z/enabled");
        set => _backing.Set("angular_spring_z/enabled", Variant.From(value));
    }

    public float AngularSpringZFrequency
    {
        get => (float)_backing.Get("angular_spring_z/frequency");
        set => _backing.Set("angular_spring_z/frequency", Variant.From(value));
    }

    public float AngularSpringZDamping
    {
        get => (float)_backing.Get("angular_spring_z/damping");
        set => _backing.Set("angular_spring_z/damping", Variant.From(value));
    }

    public float AngularSpringZEquilibriumPoint
    {
        get => (float)_backing.Get("angular_spring_z/equilibrium_point");
        set => _backing.Set("angular_spring_z/equilibrium_point", Variant.From(value));
    }

    public float GetParamX(int param) => _backing.Call("get_param_x", param).As<float>();

    public void SetParamX(int param, float value) => _backing.Call("set_param_x", param, value);

    public float GetParamY(int param) => _backing.Call("get_param_y", param).As<float>();

    public void SetParamY(int param, float value) => _backing.Call("set_param_y", param, value);

    public float GetParamZ(int param) => _backing.Call("get_param_z", param).As<float>();

    public void SetParamZ(int param, float value) => _backing.Call("set_param_z", param, value);

    public bool GetFlagX(int flag) => _backing.Call("get_flag_x", flag).As<bool>();

    public void SetFlagX(int flag, bool enabled) => _backing.Call("set_flag_x", flag, enabled);

    public bool GetFlagY(int flag) => _backing.Call("get_flag_y", flag).As<bool>();

    public void SetFlagY(int flag, bool enabled) => _backing.Call("set_flag_y", flag, enabled);

    public bool GetFlagZ(int flag) => _backing.Call("get_flag_z", flag).As<bool>();

    public void SetFlagZ(int flag, bool enabled) => _backing.Call("set_flag_z", flag, enabled);

    public float GetAppliedForce() => _backing.Call("get_applied_force").As<float>();

    public float GetAppliedTorque() => _backing.Call("get_applied_torque").As<float>();

}