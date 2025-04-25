using Godot;
[Tool, GlobalClass]
public partial class ObjectSettings : Resource {
    [Export] public PackedScene scene;
    [Export] public Vector3 rotationRange;
    [Export] public Vector2 scaleRange;
    [Export(PropertyHint.Range, "0,1,")] public Vector2 heightPercentageRange;
    [Export] public Color spawningRegionColor;
}
