
namespace FilipRuman.ProceduralMapGen {
    using Godot;
    [Tool, GlobalClass]

    public partial class NoiseComponent : Resource {
        [Export] private string debugName;
        [Export] public bool flipped;
        [Export] public FastNoiseLite noise;
        [Export] public float strength;
        [Export] public float frequencyModifier = 1f;
        [Export] public Curve detailValueModifier;
        [Export] public Curve valueBasedOnNoise;
    }
}
