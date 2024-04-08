using System;
using Godot;

namespace GDExtensionAPIGenerator;

public partial class Main : Node
{
    private static readonly string[] PLATFORM_EXECUTABLE_EXTENSION =
    [
#if GODOT_WINDOWS
        "*.exe ; Windows Executable Program",
#elif GODOT_MACOS
        "*.app ; Mac OS Executable Program"
#endif
    ];

    private const string CACHE_PATH = "user://application_cache.json";

    
    [Export] private LineEdit _godotPath;
    [Export] private Button _selectGodotPathBtn;
    [Export] private LineEdit _projectPath;
    [Export] private Button _selectProjectPathBtn;
    [Export] private Button _generateBtn;

    private FileDialog _godotPathFileDialog;
    private FileDialog _godotProjectFileDialog;

    private UserHistoryInput _userHistoryInput;

    public override void _Notification(int what)
    {
        if (what != NotificationWMCloseRequest) return;
        
        var userHistoryJson = JsonSerializer.Serialize(_userHistoryInput, ProjectJsonSerializerContext.Default.UserHistoryInput);
        using var fileWriteHandle = FileAccess.Open(CACHE_PATH, FileAccess.ModeFlags.WriteRead);
        fileWriteHandle.StoreString(userHistoryJson);
        GetTree().Quit();
    }

    public override void _Ready()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => Console.WriteLine(((Exception)args.ExceptionObject).ToString());
        if (FileAccess.FileExists(CACHE_PATH))
        {
            string fileText;

            using (var fileAccess = FileAccess.Open(CACHE_PATH, FileAccess.ModeFlags.Read))
            {
                fileText = fileAccess.GetAsText();
            }

            try
            {
                _userHistoryInput = JsonSerializer.Deserialize(fileText, ProjectJsonSerializerContext.Default.UserHistoryInput);
                _godotPath.Text = _userHistoryInput.GodotEditorPath;
                _projectPath.Text = _userHistoryInput.GodotProjectPath;
            }
            catch { }
        }

        _godotPathFileDialog = new()
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            AlwaysOnTop = true,
            UseNativeDialog = true,
            Title = "Select Godot Executable Path",
            Filters = PLATFORM_EXECUTABLE_EXTENSION,
        };

        _godotProjectFileDialog = new()
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenDir,
            AlwaysOnTop = true,
            UseNativeDialog = true,
            Title = "Select Godot Project Path",
        };

        _godotPath.DragAndDropSelectionEnabled = true;
        _projectPath.DragAndDropSelectionEnabled = true;
        _godotPath.TextChanged += newPath => _userHistoryInput.GodotEditorPath = newPath; 
        _projectPath.TextChanged += newPath => _userHistoryInput.GodotProjectPath = newPath; 
        
        _godotPathFileDialog.FileSelected += path =>
        {
            _godotPath.Text = path;
            _godotPath.EmitSignal(LineEdit.SignalName.TextChanged, path);
        };
        _godotProjectFileDialog.DirSelected += path =>
        {
            _projectPath.Text = path;
            _projectPath.EmitSignal(LineEdit.SignalName.TextChanged, path);
        };

        _selectGodotPathBtn.Pressed += () =>
        {
            _godotPathFileDialog.CurrentPath = _godotPath.Text;
            _godotPathFileDialog.Show();
        };

        _selectProjectPathBtn.Pressed += () =>
        {
            _godotProjectFileDialog.CurrentPath = _projectPath.Text;
            _godotProjectFileDialog.Show();
        };

        _generateBtn.Pressed += () =>
        {
            var godotPathText = _godotPath.Text;
            
            if (!Generator.CheckGodotPath(godotPathText, out var errorMessage))
            {
                GD.PrintErr(errorMessage);
                return;
            }

            var projectPathText = _projectPath.Text;

            if (!Generator.CheckProjectPath(projectPathText, out errorMessage))
            {
                GD.PrintErr(errorMessage);
                return;
            }
            
            Generator.Generate(godotPathText, projectPathText);
        };
    }
}