using System.Text.Json.Serialization.Metadata;

namespace GDExtensionAPIGenerator;

public struct UserHistoryInput(string GodotEditorPath, string GodotProjectPath)
{
    public string GodotEditorPath { get; set; } = GodotEditorPath;
    public string GodotProjectPath { get; set; } = GodotProjectPath;
}

[JsonSerializable(typeof(UserHistoryInput))]
public partial class ProjectJsonSerializerContext : JsonSerializerContext;
