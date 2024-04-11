using Godot;

namespace GDExtension.ResourcesWrappers;

public class JoltJointGizmoPlugin3D
{
    protected readonly Resource _backing;

    public JoltJointGizmoPlugin3D(Resource backing)
    {
        _backing = backing;
    }

}