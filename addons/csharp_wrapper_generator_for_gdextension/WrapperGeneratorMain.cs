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
    private EditorSettings _editorSettings;

    private const string DefaultNamespace = "GDExtension.Wrappers";
    private const string DefaultPath = "GDExtensionWrappers";
    private const string NamespaceSavePath = "gdextension_wrapper_generator/namespace";
    private const string PathSavePath = "gdextension_wrapper_generator/target_path";

    public override void _EnterTree()
    {
        _button = new() { Text = "Generate" };
        _vbox = new() { Name = "Wrapper Generator" };
        var grid = new GridContainer { Columns = 2 };

        grid.AddChild(new Label{Text = "Namespace:"});
        
        _nameSpace = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        _editorSettings = EditorInterface.Singleton.GetEditorSettings();
        
        if (!_editorSettings.HasSetting(NamespaceSavePath) || _editorSettings.GetSetting(NamespaceSavePath).VariantType != Variant.Type.String) 
            _editorSettings.SetSetting(NamespaceSavePath, DefaultNamespace);
        var propertyInfo = new Godot.Collections.Dictionary
        {
            { "name", NamespaceSavePath },
            { "type", Variant.From(Variant.Type.String) },
            { "hint", "" },
            { "hint_string", "" },
        };
        _editorSettings.AddPropertyInfo(propertyInfo);
        
        var savedNamespaceVariant = _editorSettings.GetSetting(NamespaceSavePath);
        var savedNamespace = EscapeNamespaceKeyWords(savedNamespaceVariant.AsString());
        _nameSpace.Text = savedNamespace;
        _nameSpace.TextChanged += text => _editorSettings.SetSetting(NamespaceSavePath, text);
        grid.AddChild(_nameSpace);
        
        grid.AddChild(new Label{Text = "Save Path:"});

        _targetPath = new() { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

        if(!_editorSettings.HasSetting(PathSavePath) || _editorSettings.GetSetting(PathSavePath).VariantType != Variant.Type.String) 
            _editorSettings.SetSetting(PathSavePath, DefaultPath);
        var pathPropertyInfo = new Godot.Collections.Dictionary
        {
            { "name", PathSavePath },
            { "type", Variant.From(Variant.Type.String) },
            { "hint", "" },
            { "hint_string", "" },
        };
        _editorSettings.AddPropertyInfo(pathPropertyInfo);
        
        var savedPathVariant = _editorSettings.GetSetting(PathSavePath);
        var savedPath = EscapePath(savedPathVariant.AsString());
        _targetPath.Text = savedPath;
        _targetPath.TextChanged += text => _editorSettings.SetSetting(PathSavePath, text);
        
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
        
        var escapedNamespace = EscapeNamespaceKeyWords(_nameSpace.Text);
        if (escapedNamespace != _nameSpace.Text)
        {
            GD.PushWarning($"The namespace contained invalid characters and was escaped to '{escapedNamespace}'");
            _nameSpace.Text = escapedNamespace;
            _editorSettings.Set(NamespaceSavePath, escapedNamespace);
        }
        
        gdExtensionTypes.AsParallel().ForAll(type => TypeWriter.WriteType(type, escapedNamespace, files, warnings));

        var escapedPath = EscapePath(_targetPath.Text);
        if (escapedPath != _targetPath.Text)
        {
            GD.PushWarning($"The target path contained invalid characters and was escaped to '{escapedPath}'");
            _targetPath.Text = escapedPath;
            _editorSettings.Set(PathSavePath, escapedPath);
        }

        var godotTargetPath = $"res://{escapedPath}";
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