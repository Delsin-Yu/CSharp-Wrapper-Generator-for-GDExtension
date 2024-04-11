using GDExtension.Wrappers;
using Godot;

namespace GDExtensionAPIGenerator;

public partial class TestScript : GDCubismUserModel
{
    [Export] private GDCubismUserModel _userModel;
    
    public override void _Ready()
    {
        LoadExpressions = false;
    }
}