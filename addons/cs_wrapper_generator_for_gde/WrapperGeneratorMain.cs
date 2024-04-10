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

		Generator.Button = _button;
		_button.Pressed += SetButton;
		_button.Pressed += Generator.Generate;
		
		AddControlToDock(DockSlot.LeftBr, _button);
	}

	private void SetButton() => Generator.Button = _button;

	public override void _ExitTree()
	{
		RemoveControlFromDocks(_button);
		_button.Pressed -= Generator.Generate;
		_button.Pressed -= SetButton;
		_button.Free();
	}
}


#endif
