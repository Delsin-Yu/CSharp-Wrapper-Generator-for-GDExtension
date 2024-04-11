using Godot;

namespace GDExtension.NodeWrappers;

public partial class JoltPhysicsServerFactory3D : GodotObject
{
    public JoltPhysicsServer3D CreateServer() => new(Call("create_server").As<RefCounted>());

}