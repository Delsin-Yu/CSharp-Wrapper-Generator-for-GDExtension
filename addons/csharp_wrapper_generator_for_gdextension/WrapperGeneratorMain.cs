#if TOOLS
using Godot;

namespace GDExtensionAPIGenerator;

[Tool]
public partial class WrapperGeneratorMain : EditorPlugin
{
    private Button _button;
    private CheckBox _includeTestsCheckBox;
    private VBoxContainer _vbox;

    public override void _EnterTree()
    {
        _button = new() { Text = "Generate" };
        _includeTestsCheckBox = new() { Text = "Include GD Unit4 Tests" };
        _vbox = new() { Name = "Wrapper Generator" };
        _vbox.AddChild(_button);
        _vbox.AddChild(_includeTestsCheckBox);
        _button.Pressed += DoGenerate;
        AddControlToDock(DockSlot.LeftBr, _vbox);
    }

    public override void _ExitTree()
    {
        RemoveControlFromDocks(_vbox);
        _button.Pressed -= DoGenerate;
        _button.Free();
        _includeTestsCheckBox.Free();
        _vbox.Free();
    }

    private void DoGenerate() =>
        GeneratorMain.Generate(_includeTestsCheckBox.ButtonPressed);
}


#endif