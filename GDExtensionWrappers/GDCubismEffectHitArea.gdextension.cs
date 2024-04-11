using Godot;

namespace GDExtension.RefCountedWrappers;

public class GDCubismEffectHitArea : GDCubismEffect
{

    protected override RefCounted Construct() =>
        (RefCounted)ClassDB.Instantiate("GDCubismEffectHitArea");

    public bool Monitoring
    {
        get => (bool)_backing.Get("monitoring");
        set => _backing.Set("monitoring", Variant.From(value));
    }

    public void SetTarget(Vector2 target) => _backing.Call("set_target", target);

    public Vector2 GetTarget() => _backing.Call("get_target").As<Vector2>();

    public Godot.Collections.Dictionary GetDetail(GDCubismUserModel model, string id) => _backing.Call("get_detail", model, id).As<Godot.Collections.Dictionary>();

}