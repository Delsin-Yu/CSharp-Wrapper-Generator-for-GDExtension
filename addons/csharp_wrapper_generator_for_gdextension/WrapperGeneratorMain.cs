#if TOOLS
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Godot;
using FileAccess = Godot.FileAccess;

namespace GDExtensionAPIGenerator;

[Tool]
public partial class WrapperGeneratorMain : EditorPlugin
{
    private Button _button;
    private VBoxContainer _vbox;
    private LineEdit _nameSpace;
    private LineEdit _targetPath;

    private const string DefaultNamespace = "GDExtension.Wrappers";
    private const string DefaultPath = "GDExtensionWrappers";
    
    public override void _EnterTree()
    {
        _button = new() { Text = "Generate" };
        _vbox = new() { Name = "Wrapper Generator" };
        var grid = new GridContainer { Columns = 2 };

        grid.AddChild(new Label{Text = "Namespace:"});
        
        _nameSpace = new()
        {
            Text = DefaultNamespace,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        const string namespaceSavePath = "gdextension_wrapper_generator/namespace";
        if(ProjectSettings.HasSetting(namespaceSavePath))
        {
            var savedNamespaceVariant = ProjectSettings.GetSetting(namespaceSavePath);
            if(savedNamespaceVariant.VariantType == Variant.Type.String)
            {
                var savedNamespace = EscapeNamespaceKeyWords(savedNamespaceVariant.AsString());
                _nameSpace.Text = savedNamespace;
                ProjectSettings.SetSetting(namespaceSavePath, savedNamespace);
            }
        }
        _nameSpace.TextSubmitted += text =>
        {
            text = EscapeNamespaceKeyWords(text);
            _nameSpace.Text = text;
            ProjectSettings.SetSetting(namespaceSavePath, text);
        };
        grid.AddChild(_nameSpace);
            
        grid.AddChild(new Label{Text = "Save Path:"});

        _targetPath = new()
        {
            Text = DefaultPath, 
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        const string pathSavePath = "gdextension_wrapper_generator/target_path";
        if(ProjectSettings.HasSetting(pathSavePath))
        {
            var savedPathVariant = ProjectSettings.GetSetting(pathSavePath);
            if(savedPathVariant.VariantType == Variant.Type.String)
            {
                var savedPath = EscapePath(savedPathVariant.AsString());
                _targetPath.Text = savedPath;
                ProjectSettings.SetSetting(pathSavePath, savedPath);
            }
        }
        _targetPath.TextSubmitted += text =>
        {
            text = EscapePath(text);
            _targetPath.Text = text;
            ProjectSettings.SetSetting(pathSavePath, text);
        };
        var targetPathBox = new HBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        targetPathBox.AddThemeConstantOverride("separation", 0);
        targetPathBox.AddChild(new Label { Text = "res://" });
        targetPathBox.AddChild(_targetPath);
        grid.AddChild(targetPathBox);
        
        _vbox.AddChild(grid);
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
        var nameSpace = _nameSpace.Text;
        gdExtensionTypes.AsParallel().ForAll(type => TypeWriter.WriteType(type, nameSpace, files, warnings));

        var godotTargetPath = $"res://{_targetPath.Text}";
        var outputDir = ProjectSettings.GlobalizePath(godotTargetPath);
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        Directory.CreateDirectory(outputDir!);
        foreach (var file in files)
        {
            var targetPath = godotTargetPath.PathJoin(file.FileName);
            using var fileAccess = FileAccess.Open(targetPath, FileAccess.ModeFlags.Write);
            fileAccess.StoreString(file.SourceCode.ReplaceLineEndings("\n"));
        }

        var warningArray = warnings.ToHashSet().Order().ToArray();
        if(warningArray.Length == 0) return;
        GD.PushWarning($"({warningArray.Length}) warning(s) during rendering the GDExtension wrapper classes:");
        foreach (var warningMessage in warningArray) 
            GD.PushWarning(warningMessage);
    }
}


#endif