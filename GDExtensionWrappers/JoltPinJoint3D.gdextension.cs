using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltPinJoint3D : JoltJoint3D
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltPinJoint3D");

    public float GetAppliedForce() => _backing.Call("get_applied_force").As<float>();

}