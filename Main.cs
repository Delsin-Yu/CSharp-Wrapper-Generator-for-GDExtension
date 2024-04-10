using System;
using Godot;

namespace GDExtensionAPIGenerator;

public partial class Main : Node
{
    [Export] private LineEdit _godotPathLabel;
    [Export] private Button _selectGodotPathBtn;
    [Export] private Label _projectPathLabel;
    [Export] private Button _selectProjectPathBtn;
    [Export] private Button _generateBtn;

    public override void _Ready()
    {
        Bindings.Bind(
            _selectGodotPathBtn,
            instance =>
            {
                //new FileDialog()
            },
            this
        );
    }
}

internal static class Bindings
{
    public static void Bind<T>(Button button, Action<T> onClick, T instance) => 
        button.Pressed += () => onClick(instance);
}