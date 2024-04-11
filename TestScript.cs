using GDExtension.Wrappers;
using Godot;

namespace GDExtensionAPIGenerator;

public partial class TestScript : GDCubismUserModel
{
    public override void _Ready()
    {
        LoadExpressions = false;
        LoadMotions = false;
        SpeedScale = 1;
        AutoScale = false;
    }
}