#if TOOLS
using Godot;
using System;

namespace GDExtensionAPIGenerator;

[Tool]
public partial class WrapperGeneratorMain : EditorPlugin
{
	private Button _button;
	
	public override void _EnterTree()
	{
		_button = new() { Text = "Generate", Name = "Wrapper Generator" };

		_button.Pressed += Generator.Generate;
		
		AddControlToDock(DockSlot.LeftBr, _button);
	}

	public override void _ExitTree()
	{
		RemoveControlFromDocks(_button);
		Generator.UnloadSystemTextJson();
		_button.Pressed -= Generator.Generate;
		_button.Free();
	}
}


#endif
