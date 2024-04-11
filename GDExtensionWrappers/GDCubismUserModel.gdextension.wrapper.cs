using Godot;

namespace GDExtension.Wrappers;

public partial class GDCubismUserModel : SubViewport
{
    public string Assets
    {
        get => (string)Get("assets");
        set => Set("assets", Variant.From(value));
    }
    public bool LoadExpressions
    {
        get => (bool)Get("load_expressions");
        set => Set("load_expressions", Variant.From(value));
    }
    public bool LoadMotions
    {
        get => (bool)Get("load_motions");
        set => Set("load_motions", Variant.From(value));
    }
    public long ParameterMode
    {
        get => (long)Get("parameter_mode");
        set => Set("parameter_mode", Variant.From(value));
    }
    public long PlaybackProcessMode
    {
        get => (long)Get("playback_process_mode");
        set => Set("playback_process_mode", Variant.From(value));
    }
    public double SpeedScale
    {
        get => (double)Get("speed_scale");
        set => Set("speed_scale", Variant.From(value));
    }
    public bool AutoScale
    {
        get => (bool)Get("auto_scale");
        set => Set("auto_scale", Variant.From(value));
    }
    public double AdjustScale
    {
        get => (double)Get("adjust_scale");
        set => Set("adjust_scale", Variant.From(value));
    }
    public Godot.Vector2 AdjustPosition
    {
        get => (Godot.Vector2)Get("adjust_position");
        set => Set("adjust_position", Variant.From(value));
    }
    public Shader ShaderAdd
    {
        get => (Shader)Get("shader_add");
        set => Set("shader_add", Variant.From(value));
    }
    public Shader ShaderMix
    {
        get => (Shader)Get("shader_mix");
        set => Set("shader_mix", Variant.From(value));
    }
    public Shader ShaderMul
    {
        get => (Shader)Get("shader_mul");
        set => Set("shader_mul", Variant.From(value));
    }
    public Shader ShaderMask
    {
        get => (Shader)Get("shader_mask");
        set => Set("shader_mask", Variant.From(value));
    }
    public Shader ShaderMaskAdd
    {
        get => (Shader)Get("shader_mask_add");
        set => Set("shader_mask_add", Variant.From(value));
    }
    public Shader ShaderMaskAddInv
    {
        get => (Shader)Get("shader_mask_add_inv");
        set => Set("shader_mask_add_inv", Variant.From(value));
    }
    public Shader ShaderMaskMix
    {
        get => (Shader)Get("shader_mask_mix");
        set => Set("shader_mask_mix", Variant.From(value));
    }
    public Shader ShaderMaskMixInv
    {
        get => (Shader)Get("shader_mask_mix_inv");
        set => Set("shader_mask_mix_inv", Variant.From(value));
    }
    public Shader ShaderMaskMul
    {
        get => (Shader)Get("shader_mask_mul");
        set => Set("shader_mask_mul", Variant.From(value));
    }
    public Shader ShaderMaskMulInv
    {
        get => (Shader)Get("shader_mask_mul_inv");
        set => Set("shader_mask_mul_inv", Variant.From(value));
    }
}