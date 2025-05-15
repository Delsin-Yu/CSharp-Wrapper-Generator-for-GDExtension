using GDExtension.Wrappers;
using Godot;

public partial class Test : Node
{
    /// <summary>
    /// You may export the wrapper type and assign the reference
    /// from the editor. Note that you need to attach the wrapper
    /// script to the GDExtension Node instance.
    /// </summary>
    [Export] private GDCubismUserModel _gdCubismUserModel;
    
    /// <summary>
    /// Developers may wrap around an exported resource using
    /// the wrapper. Ensure the type is a match.
    /// </summary>
    /// <remarks>
    /// This hint should filter the inspector options.
    /// </remarks>
    [Export(PropertyHint.ResourceType, nameof(GDCubismParameter))] private Resource _gdCubismParameter;
    
    public override void _Ready()
    {
        // All wrapped GDExtension types should be created
        // via the Instantiate() method
        using var summator = Summator.Instantiate();
        
        // Demonstrating method calls
        GD.Print(summator.GetTotal());
        summator.Add(20);
        GD.Print(summator.GetTotal());
        summator.Reset();
        GD.Print(summator.GetTotal());

        var debugDraw2D = GDExtension.Wrappers.DebugDraw2D.Instantiate();
        GD.Print(debugDraw2D.DebugEnabled);
        debugDraw2D.DebugEnabled = true;
        GD.Print(debugDraw2D.DebugEnabled);
        debugDraw2D.DebugEnabled = false;
        GD.Print(debugDraw2D.DebugEnabled);
        
        // This wraps around an existing resource type.
        // The developer should ensure the supplied Godot
        // matches the underlying GDExtension type.
        var gdCubismParameterWrapper = GDCubismParameter.Bind(_gdCubismParameter);

        // Accessing the properties and methods from
        // an exported wrapper type. The developer 
        // has to attach the wrapper script to that
        // GDExtension node instance in order for it
        // to work.
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.AutoScale = true;
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.AutoScale = false;
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.Free();
        _gdCubismUserModel = null;
    }
}