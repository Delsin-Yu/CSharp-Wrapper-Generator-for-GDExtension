# The C# Wrapper Generator for GDExtension (Work In Progress)

The [GDExtension System](https://docs.godotengine.org/en/stable/tutorials/scripting/gdextension/what_is_gdextension.html) is a powerful Godot plugin framework for developers to integrate high-performance, native code that expands Godot's abilities. However, these additional types can only be accessed easily through the `GDScript` language (such as `direct type reference`, and `properties/methods highlighting`, etc.), the other officially supported language, C#, unfortunately, does not have these features.  
Currently, C# developers are required to access these types via `Engine Level Reflections`, or the APIs from the [ClassDB](https://docs.godotengine.org/en/stable/classes/class_classdb.html) to [instantiate these types by StringName](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-instantiate), and [access their](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-get-property) [members](https://docs.godotengine.org/en/stable/classes/class_classdb.html#class-classdb-method-class-set-property) [by StringName](https://docs.godotengine.org/en/stable/classes/class_object.html#class-object-method-call).  
This plugin is designed to automate this wrapper generation process by leveraging the information gathered from ClassDB.  
The goal for this plugin is to generate wrappers for all GDExtension classes, primarily the following:
1. Nodes: The developer may inherit or export the generated wrappers to access the node's members, and use the `Construct` static method (instead of the `new()`) to create new instances.
2. Resources: The current design is a thin wrapper that wraps around an instance of `Resource`, the developer may create this wrapper in runtime(`new MyWrapper(resource)`) as a proxy for accessing the members of an instance of Resource.
3. RefCounted: The developer may use the `new()` to create an instance of the wrapper as well as construct/bind the underlying ref counted object, this should act like classes provided by the Godot.
4. Singletons: We are still trying to work this out.
5. Passing the wrappers as arguments to the method calls, or as the return value from the methods, which is a big challenge we are facing right now.

## Try this Work In Progress Project out

1. Clone the repo.
2. Open the repo in Godot 4.2.1.
3. Build the C# Solution.
4. Turn on the addon, and you should find the addon window in the bottom left corner of the editor.
5. Go to that window, and click the button.
6. Inspect the generated codes in the project or your IDE.
