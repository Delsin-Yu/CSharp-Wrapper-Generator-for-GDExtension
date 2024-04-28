// using GDExtension.Wrappers;
// using Godot;
//
// public partial class Test : Node
// {
//     
//     /// <summary>
//     /// You may export the wrapper type and assign the reference
//     /// from the editor, note that you need to attach the wrapper
//     /// script to the GDExtension Node instance.
//     /// </summary>
//     [Export] private GDCubismUserModel _gdCubismUserModel;
//     
//     /// <summary>
//     /// Developer may wrap around an exported resource using
//     /// the wrapper, however it is crucial to make sure the
//     /// type you are wrapping around is the matching type.
//     /// </summary>
//     /// <remarks>
//     /// This hint should filter the selections in the inspector,
//     /// on idea why it isn't working...
//     /// </remarks>
//     [Export(PropertyHint.ResourceType, nameof(GDCubismParameter))] private Resource _gdCubismParameter;
//     
//     public override void _Ready()
//     {
//         // All wrapped GDExtension types should be
//         // created through the Instantiate() method 
//         using var summator = Summator.Instantiate();
//         
//         // Method calls are supported
//         GD.Print(summator.GetTotal());
//         summator.Add(20);
//         GD.Print(summator.GetTotal());
//         summator.Reset();
//         GD.Print(summator.GetTotal());
//
//         var gdCubismParameterInstance = GDCubismParameter.Instantiate();
//         GD.Print(gdCubismParameterInstance.DefaultValue);
//         gdCubismParameterInstance.DefaultValue = 20;
//         GD.Print(gdCubismParameterInstance.DefaultValue);
//         gdCubismParameterInstance.DefaultValue = -20;
//         GD.Print(gdCubismParameterInstance.DefaultValue);
//         
//         // This wraps around an existing resource type,
//         // the developer should ensure the supplied godot
//         // matches the underlying GDExtension type.
//         var gdCubismParameterWrapper = GDCubismParameter.Bind(_gdCubismParameter);
//         GD.Print(gdCubismParameterWrapper.DefaultValue);
//         gdCubismParameterWrapper.DefaultValue = 20;
//         GD.Print(gdCubismParameterWrapper.DefaultValue);
//         gdCubismParameterWrapper.DefaultValue = -20;
//         GD.Print(gdCubismParameterWrapper.DefaultValue);
//
//         // Accessing the properties and methods from
//         // an exported wrapper type. The developer 
//         // have to attach the wrapper script to that
//         // GDExtension node instance in order for it
//         // to work.
//         GD.Print(_gdCubismUserModel.AutoScale);
//         _gdCubismUserModel.AutoScale = true;
//         GD.Print(_gdCubismUserModel.AutoScale);
//         _gdCubismUserModel.AutoScale = false;
//         GD.Print(_gdCubismUserModel.AutoScale);
//         _gdCubismUserModel.Free();
//         _gdCubismUserModel = null;
//
//         var joltGeneric6DofJoint3D = JoltGeneric6DOFJoint3D.Instantiate();
//         AddChild(joltGeneric6DofJoint3D);
//         GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
//         joltGeneric6DofJoint3D.AngularLimitYEnabled = false;
//         GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
//         joltGeneric6DofJoint3D.AngularLimitYEnabled = true;
//         GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
//
//         var cameraHelper = MediaPipeCameraHelper.Instantiate();
//     }
// }
