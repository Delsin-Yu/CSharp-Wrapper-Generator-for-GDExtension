using Godot;

public partial class MyNode : Node
{
    int Count;

    public override void _Ready()
    {
        Count = 0;
    }

    public override void _Process(double delta)
    {
        Count = Count + 1;
        GD.Print(Count);
    }
}