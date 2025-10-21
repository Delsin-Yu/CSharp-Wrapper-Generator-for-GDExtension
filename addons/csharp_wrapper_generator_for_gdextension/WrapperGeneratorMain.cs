#if TOOLS
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Godot;

namespace GDExtensionAPIGenerator;

[Tool]
public partial class WrapperGeneratorMain : EditorPlugin
{
    private Button _button;
    private VBoxContainer _vbox;

    public override void _EnterTree()
    {
        _button = new() { Text = "Generate" };
        _vbox = new() { Name = "Wrapper Generator" };
        _vbox.AddChild(_button);
        _button.Pressed += DoGenerate;
        AddControlToDock(DockSlot.LeftBr, _vbox);
    }

    public override void _ExitTree()
    {
        RemoveControlFromDocks(_vbox);
        _button.Pressed -= DoGenerate;
        _button.Free();
        _vbox.Free();
    }

    private void DoGenerate()
    {
        var warnings = new ConcurrentBag<string>();
        TypeCollector.CreateClassDiagram(out var gdExtensionTypes, warnings);
        
        
        var files = new ConcurrentBag<FileConstruction>();
        gdExtensionTypes.AsParallel().ForAll(type => TypeWriter.WriteType(type, files, warnings));
        
        var outputDir = ProjectSettings.GlobalizePath("res://GDExtensionWrappers");
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir!);
        files.AsParallel().ForAll(file => File.WriteAllText(Path.Combine(outputDir, file.FileName), file.SourceCode));

        var warningArray = warnings.ToHashSet().Order().ToArray();
        if(warningArray.Length == 0) return;
        GD.PushWarning($"({warningArray.Length}) warning(s) during rendering the GDExtension wrapper classes:");
        foreach (var warningMessage in warningArray) 
            GD.PushWarning(warningMessage);
    }
}


#endif