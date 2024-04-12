#if TOOLS
using Godot;

namespace GDExtensionAPIGenerator;

[Tool]
public partial class WrapperGeneratorMain : EditorPlugin
{
	private Button _button;
	
	public override void _EnterTree()
	{

		_button = new() { Text = "Generate", Name = "Wrapper Generator" };
		_button.Pressed += GeneratorMain.Generate;
		AddControlToDock(DockSlot.LeftBr, _button);
	}

	public override void _ExitTree()
	{
		RemoveControlFromDocks(_button);
		_button.Pressed -= GeneratorMain.Generate;
		_button.Free();
	}
}


#endif
