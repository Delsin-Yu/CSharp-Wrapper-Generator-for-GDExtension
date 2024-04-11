using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltPhysicsServer3D : PhysicsServer3DExtension
{
    public void DumpDebugSnapshots(string dir) => Call("dump_debug_snapshots", dir);

    public void SpaceDumpDebugSnapshot(Rid space, string dir) => Call("space_dump_debug_snapshot", space, dir);

    public bool JointGetEnabled(Rid joint) => Call("joint_get_enabled", joint).As<bool>();

    public void JointSetEnabled(Rid joint, bool enabled) => Call("joint_set_enabled", joint, enabled);

    public int JointGetSolverVelocityIterations(Rid joint) => Call("joint_get_solver_velocity_iterations", joint).As<int>();

    public void JointSetSolverVelocityIterations(Rid joint, int value) => Call("joint_set_solver_velocity_iterations", joint, value);

    public int JointGetSolverPositionIterations(Rid joint) => Call("joint_get_solver_position_iterations", joint).As<int>();

    public void JointSetSolverPositionIterations(Rid joint, int value) => Call("joint_set_solver_position_iterations", joint, value);

    public float PinJointGetAppliedForce(Rid joint) => Call("pin_joint_get_applied_force", joint).As<float>();

    public float HingeJointGetJoltParam(Rid joint, int param) => Call("hinge_joint_get_jolt_param", joint, param).As<float>();

    public void HingeJointSetJoltParam(Rid joint, int param, float value) => Call("hinge_joint_set_jolt_param", joint, param, value);

    public bool HingeJointGetJoltFlag(Rid joint, int flag) => Call("hinge_joint_get_jolt_flag", joint, flag).As<bool>();

    public void HingeJointSetJoltFlag(Rid joint, int flag, bool value) => Call("hinge_joint_set_jolt_flag", joint, flag, value);

    public float HingeJointGetAppliedForce(Rid joint) => Call("hinge_joint_get_applied_force", joint).As<float>();

    public float HingeJointGetAppliedTorque(Rid joint) => Call("hinge_joint_get_applied_torque", joint).As<float>();

    public float SliderJointGetJoltParam(Rid joint, int param) => Call("slider_joint_get_jolt_param", joint, param).As<float>();

    public void SliderJointSetJoltParam(Rid joint, int param, float value) => Call("slider_joint_set_jolt_param", joint, param, value);

    public bool SliderJointGetJoltFlag(Rid joint, int flag) => Call("slider_joint_get_jolt_flag", joint, flag).As<bool>();

    public void SliderJointSetJoltFlag(Rid joint, int flag, bool value) => Call("slider_joint_set_jolt_flag", joint, flag, value);

    public float SliderJointGetAppliedForce(Rid joint) => Call("slider_joint_get_applied_force", joint).As<float>();

    public float SliderJointGetAppliedTorque(Rid joint) => Call("slider_joint_get_applied_torque", joint).As<float>();

    public float ConeTwistJointGetJoltParam(Rid joint, int param) => Call("cone_twist_joint_get_jolt_param", joint, param).As<float>();

    public void ConeTwistJointSetJoltParam(Rid joint, int param, float value) => Call("cone_twist_joint_set_jolt_param", joint, param, value);

    public bool ConeTwistJointGetJoltFlag(Rid joint, int flag) => Call("cone_twist_joint_get_jolt_flag", joint, flag).As<bool>();

    public void ConeTwistJointSetJoltFlag(Rid joint, int flag, bool value) => Call("cone_twist_joint_set_jolt_flag", joint, flag, value);

    public float ConeTwistJointGetAppliedForce(Rid joint) => Call("cone_twist_joint_get_applied_force", joint).As<float>();

    public float ConeTwistJointGetAppliedTorque(Rid joint) => Call("cone_twist_joint_get_applied_torque", joint).As<float>();

    public float Generic6DofJointGetJoltParam(Rid joint, int param, int unnamedArg2) => Call("generic_6dof_joint_get_jolt_param", joint, param, unnamedArg2).As<float>();

    public void Generic6DofJointSetJoltParam(Rid joint, int param, int value, float unnamedArg3) => Call("generic_6dof_joint_set_jolt_param", joint, param, value, unnamedArg3);

    public bool Generic6DofJointGetJoltFlag(Rid joint, int flag, int unnamedArg2) => Call("generic_6dof_joint_get_jolt_flag", joint, flag, unnamedArg2).As<bool>();

    public void Generic6DofJointSetJoltFlag(Rid joint, int flag, int value, bool unnamedArg3) => Call("generic_6dof_joint_set_jolt_flag", joint, flag, value, unnamedArg3);

    public float Generic6DofJointGetAppliedForce(Rid joint) => Call("generic_6dof_joint_get_applied_force", joint).As<float>();

    public float Generic6DofJointGetAppliedTorque(Rid joint) => Call("generic_6dof_joint_get_applied_torque", joint).As<float>();

}