# The C# Wrapper Generator for GDExtension (Work In Progress)

The [GDExtension System](https://docs.godotengine.org/en/stable/tutorials/scripting/gdextension/what_is_gdextension.html) is a powerful Godot plugin framework for developers to integrate high-performance, native code that expands Godot's abilities. However, these additional types can only be accessed easily through the `GDScript` language (such as `direct type reference`, and `properties/methods highlighting`, etc.), the other officially supported language, C#, unfortunately, does not have these features.  
Currently, C# developers are required to access these types via `Engine Level Reflections`, or the APIs from the [ClassDB](https://docs.godotengine.org/en/stable/classes/class_classdb.html) to [instantiate these types by StringName](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-instantiate), and [access their](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-get-property) [members](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-set-property) [by StringName](https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-call).  
This plugin is designed to automate this wrapper generation process by leveraging the information gathered from ClassDB. 

## Goal

We aim to support all functionalities a developer may achieve when using GDScript to interact with the GDExtensino types, and try to stay as consistent with the current Godot .Net Module Styled API as possible, the features we have already supported are as follows:

### Node Wrappers

Node Wrappers are script that attaches to the GDExtension nodes instnace, the developer may inherit this type and have similar experience compare to intheriting builtin Godot node types.

### Other Wrappers

Other Wrappers handles the convertion from the undenrlying type to the wrapper type. You may instantiate the wrappers directly and have the underlying GDExtension type instantiated as well, or create a wrapper instance by an existing GDExtension type *(If the `MyGDEResource` inherits from the `Resource` type, the developer may `[Export]` the `Resource` type, and calls `new MyGDEResource(_exportedResource)` to link the `_exportedResource`)*. These wrappers also supports implicit conversion between the wrapper types and the underlying types.

### Shared Members

The following members are supported by both types of wrappers:

- `Method`: Support Invoking the method with typed signatures, if the signature happends to be a GDExtension type, the wrapper type is used and the conversion is handled by the implementation.
- `Property`: Property access is supported, we have not yet encountered properties that is GDExtension type yet, we will add support for that if this is the case.
- `Signal`: Exposed as C# events with typed signatures, if the signature happends to be a GDExtension type, the wrapper type is used and the conversion is handled by the implementation. Subscriptions / Desubscriptions are made possible by the implementation.

## Try this Work In Progress Project out

1. Clone the repo.
2. Open the repo in Godot 4.2.1.
3. Open the `Test.cs`, comment out everything inside.
4. Build the C# Solution.
5. Turn on the addon, and you should find the addon window in the bottom left corner of the editor.
6. Go to that window, and click the genereate button.
7. Inspect the generated codes in the project or your IDE.
8. Uncomment everything inside the `Test.cs`.
9. Run the project.

## What's the limitation

- Virtual Method support is missing and is work in progress.
- This is a new project, the wrappers are not yet tested.
- We are doing most calls by Godot reflections, I don't expect it to perform as good as plain C# implementations.

## What we have for now

Major work have completed here is a code snippet of interacting with the genreated wrappers.

```csharp
using GDExtension.Wrappers;
using Godot;

public partial class Test : Node
{
    
    /// <summary>
    /// You may export the wrapper type and assign the reference
    /// from the editor, note that you need to attach the wrapper
    /// script to the GDExtension Node instance.
    /// </summary>
    [Export] private GDCubismUserModel _gdCubismUserModel;
    
    /// <summary>
    /// Developer may wrap around an exported resource using
    /// the wrapper, however it is crucial to make sure the
    /// type you are wrapping around is the matching type.
    /// </summary>
    /// <remarks>
    /// This hint should filter the selections in the inspector,
    /// on idea why it isn't working...
    /// </remarks>
    [Export(PropertyHint.ResourceType, nameof(GDCubismParameter))] private Resource _gdCubismParameter;
    
    public override void _Ready()
    {
        // You may directly instantiate all non-nodes
        // GDExtension classes using the constructor 
        using var summator = new Summator();
        
        // Method calls are supported
        GD.Print(summator.GetTotal());
        summator.Add(20);
        GD.Print(summator.GetTotal());
        summator.Reset();
        GD.Print(summator.GetTotal());

        // This Creates a new instance of the
        // underlying GDExtension type.
        var gdCubismParameterInstance = new GDCubismParameter();
        GD.Print(gdCubismParameterInstance.DefaultValue);
        gdCubismParameterInstance.DefaultValue = 20;
        GD.Print(gdCubismParameterInstance.DefaultValue);
        gdCubismParameterInstance.DefaultValue = -20;
        GD.Print(gdCubismParameterInstance.DefaultValue);
        
        // This takes out the underlying C# wrapper from this wrapper.
        var underlyingResource = (Resource)gdCubismParameterInstance;
        
        // This wraps around an existing resource type,
        // the developer should ensure the type for the
        // resources is the matching type.
        var gdCubismParameterWrapper = new GDCubismParameter(_gdCubismParameter);
        GD.Print(gdCubismParameterWrapper.DefaultValue);
        gdCubismParameterWrapper.DefaultValue = 20;
        GD.Print(gdCubismParameterWrapper.DefaultValue);
        gdCubismParameterWrapper.DefaultValue = -20;
        GD.Print(gdCubismParameterWrapper.DefaultValue);

        // Accessing the properties and methods from
        // an exported wrapper type. The developer 
        // have to attach the wrapper script to that
        // GDExtension node instance in order for it
        // to work.
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.AutoScale = true;
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.AutoScale = false;
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.Free();
        _gdCubismUserModel = null;

        // This construct an instance of the underlying 
        // GDExtension Node, we are using a static method
        // here is we need to do manual black magic for
        // setting up the wrapper script and the underlying 
        // GDExtension node instance.
        var joltGeneric6DofJoint3D = JoltGeneric6DOFJoint3D.Construct();
        AddChild(joltGeneric6DofJoint3D);
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        joltGeneric6DofJoint3D.AngularLimitYEnabled = false;
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        joltGeneric6DofJoint3D.AngularLimitYEnabled = true;
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
    }
}

```