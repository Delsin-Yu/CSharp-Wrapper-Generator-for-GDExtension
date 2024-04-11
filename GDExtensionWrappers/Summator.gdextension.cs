using Godot;

namespace GDExtension.NodeWrappers;

public partial class Summator : RefCounted
{
    public void Add(int value) => Call("add", value);

    public void Reset() => Call("reset");

    public int GetTotal() => Call("get_total").As<int>();

}