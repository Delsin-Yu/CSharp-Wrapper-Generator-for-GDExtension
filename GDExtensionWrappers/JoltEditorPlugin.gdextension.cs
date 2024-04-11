using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class JoltEditorPlugin : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("JoltEditorPlugin");

    public JoltEditorPlugin Construct(RefCounted backing) =>
        new JoltEditorPlugin(backing);

    protected readonly RefCounted _backing;

    public JoltEditorPlugin() => _backing = Construct();

    private JoltEditorPlugin(RefCounted backing) => _backing = backing;

    public void Dispose() => _backing.Dispose();

    public void ToolMenuPressed(int unnamedArg0) => _backing.Call("_tool_menu_pressed", unnamedArg0);

    public void SnapshotsDirSelected(string unnamedArg0) => _backing.Call("_snapshots_dir_selected", unnamedArg0);

}