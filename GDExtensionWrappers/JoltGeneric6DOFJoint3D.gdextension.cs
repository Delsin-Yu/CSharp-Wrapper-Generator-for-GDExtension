using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltGeneric6DOFJoint3D : JoltJoint3D
{
    public bool LinearLimitXEnabled
    {
        get => (bool)Get("linear_limit_x/enabled");
        set => Set("linear_limit_x/enabled", Variant.From(value));
    }

    public float LinearLimitXUpper
    {
        get => (float)Get("linear_limit_x/upper");
        set => Set("linear_limit_x/upper", Variant.From(value));
    }

    public float LinearLimitXLower
    {
        get => (float)Get("linear_limit_x/lower");
        set => Set("linear_limit_x/lower", Variant.From(value));
    }

    public bool LinearLimitYEnabled
    {
        get => (bool)Get("linear_limit_y/enabled");
        set => Set("linear_limit_y/enabled", Variant.From(value));
    }

    public float LinearLimitYUpper
    {
        get => (float)Get("linear_limit_y/upper");
        set => Set("linear_limit_y/upper", Variant.From(value));
    }

    public float LinearLimitYLower
    {
        get => (float)Get("linear_limit_y/lower");
        set => Set("linear_limit_y/lower", Variant.From(value));
    }

    public bool LinearLimitZEnabled
    {
        get => (bool)Get("linear_limit_z/enabled");
        set => Set("linear_limit_z/enabled", Variant.From(value));
    }

    public float LinearLimitZUpper
    {
        get => (float)Get("linear_limit_z/upper");
        set => Set("linear_limit_z/upper", Variant.From(value));
    }

    public float LinearLimitZLower
    {
        get => (float)Get("linear_limit_z/lower");
        set => Set("linear_limit_z/lower", Variant.From(value));
    }

    public bool LinearLimitSpringXEnabled
    {
        get => (bool)Get("linear_limit_spring_x/enabled");
        set => Set("linear_limit_spring_x/enabled", Variant.From(value));
    }

    public float LinearLimitSpringXFrequency
    {
        get => (float)Get("linear_limit_spring_x/frequency");
        set => Set("linear_limit_spring_x/frequency", Variant.From(value));
    }

    public float LinearLimitSpringXDamping
    {
        get => (float)Get("linear_limit_spring_x/damping");
        set => Set("linear_limit_spring_x/damping", Variant.From(value));
    }

    public bool LinearLimitSpringYEnabled
    {
        get => (bool)Get("linear_limit_spring_y/enabled");
        set => Set("linear_limit_spring_y/enabled", Variant.From(value));
    }

    public float LinearLimitSpringYFrequency
    {
        get => (float)Get("linear_limit_spring_y/frequency");
        set => Set("linear_limit_spring_y/frequency", Variant.From(value));
    }

    public float LinearLimitSpringYDamping
    {
        get => (float)Get("linear_limit_spring_y/damping");
        set => Set("linear_limit_spring_y/damping", Variant.From(value));
    }

    public bool LinearLimitSpringZEnabled
    {
        get => (bool)Get("linear_limit_spring_z/enabled");
        set => Set("linear_limit_spring_z/enabled", Variant.From(value));
    }

    public float LinearLimitSpringZFrequency
    {
        get => (float)Get("linear_limit_spring_z/frequency");
        set => Set("linear_limit_spring_z/frequency", Variant.From(value));
    }

    public float LinearLimitSpringZDamping
    {
        get => (float)Get("linear_limit_spring_z/damping");
        set => Set("linear_limit_spring_z/damping", Variant.From(value));
    }

    public bool LinearMotorXEnabled
    {
        get => (bool)Get("linear_motor_x/enabled");
        set => Set("linear_motor_x/enabled", Variant.From(value));
    }

    public float LinearMotorXTargetVelocity
    {
        get => (float)Get("linear_motor_x/target_velocity");
        set => Set("linear_motor_x/target_velocity", Variant.From(value));
    }

    public float LinearMotorXMaxForce
    {
        get => (float)Get("linear_motor_x/max_force");
        set => Set("linear_motor_x/max_force", Variant.From(value));
    }

    public bool LinearMotorYEnabled
    {
        get => (bool)Get("linear_motor_y/enabled");
        set => Set("linear_motor_y/enabled", Variant.From(value));
    }

    public float LinearMotorYTargetVelocity
    {
        get => (float)Get("linear_motor_y/target_velocity");
        set => Set("linear_motor_y/target_velocity", Variant.From(value));
    }

    public float LinearMotorYMaxForce
    {
        get => (float)Get("linear_motor_y/max_force");
        set => Set("linear_motor_y/max_force", Variant.From(value));
    }

    public bool LinearMotorZEnabled
    {
        get => (bool)Get("linear_motor_z/enabled");
        set => Set("linear_motor_z/enabled", Variant.From(value));
    }

    public float LinearMotorZTargetVelocity
    {
        get => (float)Get("linear_motor_z/target_velocity");
        set => Set("linear_motor_z/target_velocity", Variant.From(value));
    }

    public float LinearMotorZMaxForce
    {
        get => (float)Get("linear_motor_z/max_force");
        set => Set("linear_motor_z/max_force", Variant.From(value));
    }

    public bool LinearSpringXEnabled
    {
        get => (bool)Get("linear_spring_x/enabled");
        set => Set("linear_spring_x/enabled", Variant.From(value));
    }

    public float LinearSpringXFrequency
    {
        get => (float)Get("linear_spring_x/frequency");
        set => Set("linear_spring_x/frequency", Variant.From(value));
    }

    public float LinearSpringXDamping
    {
        get => (float)Get("linear_spring_x/damping");
        set => Set("linear_spring_x/damping", Variant.From(value));
    }

    public float LinearSpringXEquilibriumPoint
    {
        get => (float)Get("linear_spring_x/equilibrium_point");
        set => Set("linear_spring_x/equilibrium_point", Variant.From(value));
    }

    public bool LinearSpringYEnabled
    {
        get => (bool)Get("linear_spring_y/enabled");
        set => Set("linear_spring_y/enabled", Variant.From(value));
    }

    public float LinearSpringYFrequency
    {
        get => (float)Get("linear_spring_y/frequency");
        set => Set("linear_spring_y/frequency", Variant.From(value));
    }

    public float LinearSpringYDamping
    {
        get => (float)Get("linear_spring_y/damping");
        set => Set("linear_spring_y/damping", Variant.From(value));
    }

    public float LinearSpringYEquilibriumPoint
    {
        get => (float)Get("linear_spring_y/equilibrium_point");
        set => Set("linear_spring_y/equilibrium_point", Variant.From(value));
    }

    public bool LinearSpringZEnabled
    {
        get => (bool)Get("linear_spring_z/enabled");
        set => Set("linear_spring_z/enabled", Variant.From(value));
    }

    public float LinearSpringZFrequency
    {
        get => (float)Get("linear_spring_z/frequency");
        set => Set("linear_spring_z/frequency", Variant.From(value));
    }

    public float LinearSpringZDamping
    {
        get => (float)Get("linear_spring_z/damping");
        set => Set("linear_spring_z/damping", Variant.From(value));
    }

    public float LinearSpringZEquilibriumPoint
    {
        get => (float)Get("linear_spring_z/equilibrium_point");
        set => Set("linear_spring_z/equilibrium_point", Variant.From(value));
    }

    public bool AngularLimitXEnabled
    {
        get => (bool)Get("angular_limit_x/enabled");
        set => Set("angular_limit_x/enabled", Variant.From(value));
    }

    public float AngularLimitXUpper
    {
        get => (float)Get("angular_limit_x/upper");
        set => Set("angular_limit_x/upper", Variant.From(value));
    }

    public float AngularLimitXLower
    {
        get => (float)Get("angular_limit_x/lower");
        set => Set("angular_limit_x/lower", Variant.From(value));
    }

    public bool AngularLimitYEnabled
    {
        get => (bool)Get("angular_limit_y/enabled");
        set => Set("angular_limit_y/enabled", Variant.From(value));
    }

    public float AngularLimitYUpper
    {
        get => (float)Get("angular_limit_y/upper");
        set => Set("angular_limit_y/upper", Variant.From(value));
    }

    public float AngularLimitYLower
    {
        get => (float)Get("angular_limit_y/lower");
        set => Set("angular_limit_y/lower", Variant.From(value));
    }

    public bool AngularLimitZEnabled
    {
        get => (bool)Get("angular_limit_z/enabled");
        set => Set("angular_limit_z/enabled", Variant.From(value));
    }

    public float AngularLimitZUpper
    {
        get => (float)Get("angular_limit_z/upper");
        set => Set("angular_limit_z/upper", Variant.From(value));
    }

    public float AngularLimitZLower
    {
        get => (float)Get("angular_limit_z/lower");
        set => Set("angular_limit_z/lower", Variant.From(value));
    }

    public bool AngularMotorXEnabled
    {
        get => (bool)Get("angular_motor_x/enabled");
        set => Set("angular_motor_x/enabled", Variant.From(value));
    }

    public float AngularMotorXTargetVelocity
    {
        get => (float)Get("angular_motor_x/target_velocity");
        set => Set("angular_motor_x/target_velocity", Variant.From(value));
    }

    public float AngularMotorXMaxTorque
    {
        get => (float)Get("angular_motor_x/max_torque");
        set => Set("angular_motor_x/max_torque", Variant.From(value));
    }

    public bool AngularMotorYEnabled
    {
        get => (bool)Get("angular_motor_y/enabled");
        set => Set("angular_motor_y/enabled", Variant.From(value));
    }

    public float AngularMotorYTargetVelocity
    {
        get => (float)Get("angular_motor_y/target_velocity");
        set => Set("angular_motor_y/target_velocity", Variant.From(value));
    }

    public float AngularMotorYMaxTorque
    {
        get => (float)Get("angular_motor_y/max_torque");
        set => Set("angular_motor_y/max_torque", Variant.From(value));
    }

    public bool AngularMotorZEnabled
    {
        get => (bool)Get("angular_motor_z/enabled");
        set => Set("angular_motor_z/enabled", Variant.From(value));
    }

    public float AngularMotorZTargetVelocity
    {
        get => (float)Get("angular_motor_z/target_velocity");
        set => Set("angular_motor_z/target_velocity", Variant.From(value));
    }

    public float AngularMotorZMaxTorque
    {
        get => (float)Get("angular_motor_z/max_torque");
        set => Set("angular_motor_z/max_torque", Variant.From(value));
    }

    public bool AngularSpringXEnabled
    {
        get => (bool)Get("angular_spring_x/enabled");
        set => Set("angular_spring_x/enabled", Variant.From(value));
    }

    public float AngularSpringXFrequency
    {
        get => (float)Get("angular_spring_x/frequency");
        set => Set("angular_spring_x/frequency", Variant.From(value));
    }

    public float AngularSpringXDamping
    {
        get => (float)Get("angular_spring_x/damping");
        set => Set("angular_spring_x/damping", Variant.From(value));
    }

    public float AngularSpringXEquilibriumPoint
    {
        get => (float)Get("angular_spring_x/equilibrium_point");
        set => Set("angular_spring_x/equilibrium_point", Variant.From(value));
    }

    public bool AngularSpringYEnabled
    {
        get => (bool)Get("angular_spring_y/enabled");
        set => Set("angular_spring_y/enabled", Variant.From(value));
    }

    public float AngularSpringYFrequency
    {
        get => (float)Get("angular_spring_y/frequency");
        set => Set("angular_spring_y/frequency", Variant.From(value));
    }

    public float AngularSpringYDamping
    {
        get => (float)Get("angular_spring_y/damping");
        set => Set("angular_spring_y/damping", Variant.From(value));
    }

    public float AngularSpringYEquilibriumPoint
    {
        get => (float)Get("angular_spring_y/equilibrium_point");
        set => Set("angular_spring_y/equilibrium_point", Variant.From(value));
    }

    public bool AngularSpringZEnabled
    {
        get => (bool)Get("angular_spring_z/enabled");
        set => Set("angular_spring_z/enabled", Variant.From(value));
    }

    public float AngularSpringZFrequency
    {
        get => (float)Get("angular_spring_z/frequency");
        set => Set("angular_spring_z/frequency", Variant.From(value));
    }

    public float AngularSpringZDamping
    {
        get => (float)Get("angular_spring_z/damping");
        set => Set("angular_spring_z/damping", Variant.From(value));
    }

    public float AngularSpringZEquilibriumPoint
    {
        get => (float)Get("angular_spring_z/equilibrium_point");
        set => Set("angular_spring_z/equilibrium_point", Variant.From(value));
    }

}