using GDExtension.Wrappers;
using Godot;

public partial class Test : Node
{
    [Export] private GDCubismUserModel _gdCubismUserModel;
    
    public override void _Ready()
    {
        using var summator = new Summator();
        GD.Print(summator.GetTotal());
        summator.Add(20);
        GD.Print(summator.GetTotal());
        summator.Reset();
        GD.Print(summator.GetTotal());

        using (var gdCubismParameter = new GDCubismParameter())
        {
            GD.Print(gdCubismParameter.DefaultValue);
            gdCubismParameter.DefaultValue = 20;
            GD.Print(gdCubismParameter.DefaultValue);
            gdCubismParameter.DefaultValue = -20;
            GD.Print(gdCubismParameter.DefaultValue);
        }

        using (_gdCubismUserModel)
        {
            GD.Print(_gdCubismUserModel.AutoScale);
            _gdCubismUserModel.AutoScale = true;
            GD.Print(_gdCubismUserModel.AutoScale);
            _gdCubismUserModel.AutoScale = false;
            GD.Print(_gdCubismUserModel.AutoScale);
            _gdCubismUserModel.Free();
            _gdCubismUserModel = null;
        }

        using (var joltGeneric6DofJoint3D = JoltGeneric6DOFJoint3D.Construct())
        {
            AddChild(joltGeneric6DofJoint3D);
            GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
            joltGeneric6DofJoint3D.AngularLimitYEnabled = false;
            GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
            joltGeneric6DofJoint3D.AngularLimitYEnabled = true;
            GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        }
    }
}
