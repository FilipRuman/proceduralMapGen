using Godot;
[Tool, GlobalClass]

public partial class NoiseComponent : Resource {

    [Export] public FastNoiseLite noise;
    [Export] public float strength;
    [Export] public float frequencyModifier = 1f;
    [Export(PropertyHint.Range, "0,1,")] public float detailLevel;
}
