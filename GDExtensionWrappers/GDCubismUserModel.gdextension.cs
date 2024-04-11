using System;
using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismUserModel : IDisposable
{

    protected virtual RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismUserModel");

    public GDCubismUserModel Construct(RefCounted backing) =>
        new GDCubismUserModel(backing);

    protected readonly RefCounted _backing;

    public GDCubismUserModel() => _backing = Construct();

    private GDCubismUserModel(RefCounted backing) => _backing = backing;

    public void Dispose() => _backing.Dispose();

    public string Assets
    {
        get => (string)_backing.Get("assets");
        set => _backing.Set("assets", Variant.From(value));
    }

    public bool LoadExpressions
    {
        get => (bool)_backing.Get("load_expressions");
        set => _backing.Set("load_expressions", Variant.From(value));
    }

    public bool LoadMotions
    {
        get => (bool)_backing.Get("load_motions");
        set => _backing.Set("load_motions", Variant.From(value));
    }

    public int ParameterMode
    {
        get => (int)_backing.Get("parameter_mode");
        set => _backing.Set("parameter_mode", Variant.From(value));
    }

    public int PlaybackProcessMode
    {
        get => (int)_backing.Get("playback_process_mode");
        set => _backing.Set("playback_process_mode", Variant.From(value));
    }

    public float SpeedScale
    {
        get => (float)_backing.Get("speed_scale");
        set => _backing.Set("speed_scale", Variant.From(value));
    }

    public bool AutoScale
    {
        get => (bool)_backing.Get("auto_scale");
        set => _backing.Set("auto_scale", Variant.From(value));
    }

    public float AdjustScale
    {
        get => (float)_backing.Get("adjust_scale");
        set => _backing.Set("adjust_scale", Variant.From(value));
    }

    public Vector2 AdjustPosition
    {
        get => (Vector2)_backing.Get("adjust_position");
        set => _backing.Set("adjust_position", Variant.From(value));
    }

    public Shader ShaderAdd
    {
        get => (Shader)_backing.Get("shader_add");
        set => _backing.Set("shader_add", Variant.From(value));
    }

    public Shader ShaderMix
    {
        get => (Shader)_backing.Get("shader_mix");
        set => _backing.Set("shader_mix", Variant.From(value));
    }

    public Shader ShaderMul
    {
        get => (Shader)_backing.Get("shader_mul");
        set => _backing.Set("shader_mul", Variant.From(value));
    }

    public Shader ShaderMask
    {
        get => (Shader)_backing.Get("shader_mask");
        set => _backing.Set("shader_mask", Variant.From(value));
    }

    public Shader ShaderMaskAdd
    {
        get => (Shader)_backing.Get("shader_mask_add");
        set => _backing.Set("shader_mask_add", Variant.From(value));
    }

    public Shader ShaderMaskAddInv
    {
        get => (Shader)_backing.Get("shader_mask_add_inv");
        set => _backing.Set("shader_mask_add_inv", Variant.From(value));
    }

    public Shader ShaderMaskMix
    {
        get => (Shader)_backing.Get("shader_mask_mix");
        set => _backing.Set("shader_mask_mix", Variant.From(value));
    }

    public Shader ShaderMaskMixInv
    {
        get => (Shader)_backing.Get("shader_mask_mix_inv");
        set => _backing.Set("shader_mask_mix_inv", Variant.From(value));
    }

    public Shader ShaderMaskMul
    {
        get => (Shader)_backing.Get("shader_mask_mul");
        set => _backing.Set("shader_mask_mul", Variant.From(value));
    }

    public Shader ShaderMaskMulInv
    {
        get => (Shader)_backing.Get("shader_mask_mul_inv");
        set => _backing.Set("shader_mask_mul_inv", Variant.From(value));
    }

    public Godot.Collections.Dictionary CsmGetVersion() => _backing.Call("csm_get_version").As<Godot.Collections.Dictionary>();

    public int CsmGetLatestMocVersion() => _backing.Call("csm_get_latest_moc_version").As<int>();

    public int CsmGetMocVersion() => _backing.Call("csm_get_moc_version").As<int>();

    public Godot.Collections.Dictionary GetCanvasInfo() => _backing.Call("get_canvas_info").As<Godot.Collections.Dictionary>();

    public void Clear() => _backing.Call("clear");

    public void SetProcessCallback(int value) => _backing.Call("set_process_callback", value);

    public int GetProcessCallback() => _backing.Call("get_process_callback").As<int>();

    public Godot.Collections.Dictionary GetMotions() => _backing.Call("get_motions").As<Godot.Collections.Dictionary>();

    public GDCubismMotionQueueEntryHandle StartMotion(string group, int no, int priority) => new(_backing.Call("start_motion", group, no, priority).As<Resource>());

    public GDCubismMotionQueueEntryHandle StartMotionLoop(string group, int no, int priority, bool loop, bool loopFadeIn) => new(_backing.Call("start_motion_loop", group, no, priority, loop, loopFadeIn).As<Resource>());

    public Godot.Collections.Array GetCubismMotionQueueEntries() => _backing.Call("get_cubism_motion_queue_entries").As<Godot.Collections.Array>();

    public void StopMotion() => _backing.Call("stop_motion");

    public Godot.Collections.Array GetExpressions() => _backing.Call("get_expressions").As<Godot.Collections.Array>();

    public void StartExpression(string expressionId) => _backing.Call("start_expression", expressionId);

    public void StopExpression() => _backing.Call("stop_expression");

    public Godot.Collections.Array GetHitAreas() => _backing.Call("get_hit_areas").As<Godot.Collections.Array>();

    public Godot.Collections.Array GetParameters() => _backing.Call("get_parameters").As<Godot.Collections.Array>();

    public Godot.Collections.Array GetPartOpacities() => _backing.Call("get_part_opacities").As<Godot.Collections.Array>();

    public Godot.Collections.Dictionary GetMeshes() => _backing.Call("get_meshes").As<Godot.Collections.Dictionary>();

    public void Advance(float delta) => _backing.Call("advance", delta);

}