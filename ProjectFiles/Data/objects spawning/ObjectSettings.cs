using Godot;
[Tool, GlobalClass]
public partial class ObjectSettings : Resource {
    [Export] public PackedScene scene;
    [Export] public Vector3 rotationRange;
    [Export] public Vector2 scaleRange;
    [Export] public Vector2 heightRange;
}
