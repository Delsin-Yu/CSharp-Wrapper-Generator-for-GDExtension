# C# Wrapper Generator for GDExtension (Work In Progress)

[English](#english) | [中文](#中文)

## English

The [GDExtension System](https://docs.godotengine.org/en/stable/tutorials/scripting/gdextension/what_is_gdextension.html) is a powerful Godot plugin framework for developers to integrate high-performance, native code that expands Godot's abilities. However, these additional types can only be easily accessed through the `GDScript` language (such as `direct type reference`, and `properties/methods highlighting`, etc.). Unfortunately, the other officially supported language, C#, does not have these features.  
Currently, C# developers are required to access these types via `Engine Level Reflections`, or the APIs from the [ClassDB](https://docs.godotengine.org/en/stable/classes/class_classdb.html) to [instantiate these types by StringName](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-instantiate), and [access their](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-get-property) [members](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-set-property) [by StringName](https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-call).  
This plugin is designed to automate this wrapper generation process by leveraging the information gathered from ClassDB.

### Goal

We aim to support all functionalities a developer may achieve when using GDScript to interact with the GDExtension types and try to stay as consistent with the current Godot .Net Module Styled API as possible.

### Wrapper Type

Wrappers are, basically, C# scripts attached to the GDExtension provided node instance, with generated APIs matching the GDExtension implementation.

#### Wrapper Instantiation & Binding

To create a GDExtension type instance, call `TypeName.Instantiate()`.

To bind (wrap) a GDExtension type instance, call `TypeName.Bind(instance)`.

#### API Support

The following type members are supported by the wrappers:

- **Method**: Supports invoking the method with typed signatures. If a signature is a GDExtension type, the wrapper type is used and the conversion is handled by the implementation.
- **Property**: Property access is supported. We have not yet encountered properties that are GDExtension types. We will add support for that if this is the case.
- **Signal**: Exposed as C# events with typed signatures. If a signature is a GDExtension type, the wrapper type is used and the conversion is handled automatically. You can subscribe and unsubscribe to signals through the implementation.

#### Polymorphism Usage

By inheriting a wrapper type, you can code as if you were inheriting built-in Godot node types, preserving a consistent workflow.

### Try this Work In Progress Project Out

1. Clone the repository.  
2. Open the repository in Godot 4.2.1+.  
3. Open `Test.cs`, then comment out its contents.  
4. Build the C# solution.  
5. Enable the addon; find the new addon window in the bottom-left corner of the Godot editor.  
6. In that window, click “Generate.”  
7. Inspect the generated code in your IDE.  
8. Uncomment everything in `Test.cs`.  
9. Run the project.

### Limitations

- The generator is written in C#, requiring a Mono version of Godot. Future plans include migrating the generator to GDScript or a standalone application.
- Virtual method support is missing (contributions are welcome).  
- This is a new project, and the wrappers are not yet thoroughly tested.  
- Calls use Godot reflections (`ClassDB.Call`), so performance may not match plain C# implementations.

### Current Progress

Major work is complete. Below is a sample of how to interact with the generated wrappers:

```csharp
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
        
        // Method calls are supported
        GD.Print(summator.GetTotal());
        summator.Add(20);
        GD.Print(summator.GetTotal());
        summator.Reset();
        GD.Print(summator.GetTotal());

        var gdCubismParameterInstance = GDCubismParameter.Instantiate();
        GD.Print(gdCubismParameterInstance.DefaultValue);
        gdCubismParameterInstance.DefaultValue = 20;
        GD.Print(gdCubismParameterInstance.DefaultValue);
        gdCubismParameterInstance.DefaultValue = -20;
        GD.Print(gdCubismParameterInstance.DefaultValue);
        
        // This wraps around an existing resource type.
        // The developer should ensure the supplied Godot
        // matches the underlying GDExtension type.
        var gdCubismParameterWrapper = GDCubismParameter.Bind(_gdCubismParameter);
        GD.Print(gdCubismParameterWrapper.DefaultValue);
        gdCubismParameterWrapper.DefaultValue = 20;
        GD.Print(gdCubismParameterWrapper.DefaultValue);
        gdCubismParameterWrapper.DefaultValue = -20;
        GD.Print(gdCubismParameterWrapper.DefaultValue);

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

        var joltGeneric6DofJoint3D = JoltGeneric6DOFJoint3D.Instantiate();
        AddChild(joltGeneric6DofJoint3D);
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        joltGeneric6DofJoint3D.AngularLimitYEnabled = false;
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        joltGeneric6DofJoint3D.AngularLimitYEnabled = true;
    }
}
```

## 中文

[GDExtension 系统](https://docs.godotengine.org/en/stable/tutorials/scripting/gdextension/what_is_gdextension.html) 是一个强大的 Godot 插件框架，开发者可以通过它集成高性能的原生代码来扩展 Godot 的功能。然而，这些附加类型只能通过 `GDScript` 语言轻松访问（例如 `直接类型引用` 和 `属性/方法高亮` 等）。不幸的是，另一个官方支持的语言 C# 并没有这些功能。  
目前，C# 开发者需要通过 `引擎级反射` 或 [ClassDB](https://docs.godotengine.org/en/stable/classes/class_classdb.html) 的 API 来 [通过 StringName 实例化这些类型](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-instantiate)，并 [通过 StringName 访问它们的成员](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-get-property)。  
这个插件旨在通过利用从 ClassDB 收集的信息来自动化这个包装器生成过程。

### 目标

我们旨在支持开发者使用 GDScript 与 GDExtension 类型交互时可以实现的所有功能，并尽量保持与当前 Godot .Net 模块风格 API 的一致性。

### 包装器类型

包装器基本上是附加到 GDExtension 提供的节点实例上的 C# 脚本，生成的 API 与 GDExtension 实现相匹配。

#### 包装器实例化与绑定

要创建一个 GDExtension 类型实例，请调用 `TypeName.Instantiate()`。

要绑定（包装）一个 GDExtension 类型实例，请调用 `TypeName.Bind(instance)`。

#### API 支持

包装器支持以下类型成员：

- **方法**：支持使用类型化签名调用方法。如果签名是 GDExtension 类型，则使用包装器类型，并由实现处理转换。
- **属性**：支持属性访问。我们尚未遇到 GDExtension 类型的属性。如果遇到这种情况，我们将添加支持。
- **信号**：作为具有类型化签名的 C# 事件公开。如果签名是 GDExtension 类型，则使用包装器类型，并自动处理转换。您可以通过实现订阅和取消订阅信号。

#### 多态使用

通过继承包装器类型，您可以像继承内置的 Godot 节点类型一样进行编码，保持一致的工作流程。

### 尝试这个进行中的项目

1. 克隆仓库。  
2. 在 Godot 4.2.1+ 中打开仓库。  
3. 打开 `Test.cs`，然后注释掉其内容。  
4. 构建 C# 解决方案。  
5. 打开插件；在 Godot 编辑器的左下角找到新的插件窗口。  
6. 在该窗口中，点击“生成”。  
7. 在您的 IDE 中检查生成的代码。  
8. 取消注释 `Test.cs` 中的所有内容。  
9. 运行项目。

### 限制

- 生成器是用 C# 编写的，需要 Mono 版本的 Godot。未来计划将生成器迁移到 GDScript 或独立应用程序。
- 缺少虚方法支持（欢迎贡献）。  
- 这是一个新项目，包装器尚未经过彻底测试。  
- 调用使用 Godot 反射（`ClassDB.Call`），因此性能可能不如纯 C# 实现。

### 当前进展

主要工作已完成。以下是如何与生成的包装器交互的示例：

```csharp
public partial class Test : Node
{
    /// <summary>
    /// 您可以导出包装器类型并从编辑器中分配引用。
    /// 请注意，您需要将包装器脚本附加到 GDExtension 节点实例。
    /// </summary>
    [Export] private GDCubismUserModel _gdCubismUserModel;
    
    /// <summary>
    /// 开发者可以使用包装器包装导出的资源。
    /// 确保类型匹配。
    /// </summary>
    /// <remarks>
    /// 这个提示应该过滤检查器选项。
    /// </remarks>
    [Export(PropertyHint.ResourceType, nameof(GDCubismParameter))] private Resource _gdCubismParameter;
    
    public override void _Ready()
    {
        // 所有包装的 GDExtension 类型应通过 Instantiate() 方法创建
        using var summator = Summator.Instantiate();
        
        // 演示方法调用
        GD.Print(summator.GetTotal());
        summator.Add(20);
        GD.Print(summator.GetTotal());
        summator.Reset();
        GD.Print(summator.GetTotal());

        var gdCubismParameterInstance = GDCubismParameter.Instantiate();
        GD.Print(gdCubismParameterInstance.DefaultValue);
        gdCubismParameterInstance.DefaultValue = 20;
        GD.Print(gdCubismParameterInstance.DefaultValue);
        gdCubismParameterInstance.DefaultValue = -20;
        GD.Print(gdCubismParameterInstance.DefaultValue);
        
        // 这将包装一个现有的资源类型。
        // 开发者应确保提供的 Godot 匹配底层的 GDExtension 类型。
        var gdCubismParameterWrapper = GDCubismParameter.Bind(_gdCubismParameter);
        GD.Print(gdCubismParameterWrapper.DefaultValue);
        gdCubismParameterWrapper.DefaultValue = 20;
        GD.Print(gdCubismParameterWrapper.DefaultValue);
        gdCubismParameterWrapper.DefaultValue = -20;
        GD.Print(gdCubismParameterWrapper.DefaultValue);

        // 访问导出的包装器类型的属性和方法。
        // 开发者必须将包装器脚本附加到该 GDExtension 节点实例才能工作。
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.AutoScale = true;
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.AutoScale = false;
        GD.Print(_gdCubismUserModel.AutoScale);
        _gdCubismUserModel.Free();
        _gdCubismUserModel = null;

        var joltGeneric6DofJoint3D = JoltGeneric6DOFJoint3D.Instantiate();
        AddChild(joltGeneric6DofJoint3D);
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        joltGeneric6DofJoint3D.AngularLimitYEnabled = false;
        GD.Print(joltGeneric6DofJoint3D.AngularLimitYEnabled);
        joltGeneric6DofJoint3D.AngularLimitYEnabled = true;
    }
}
```
