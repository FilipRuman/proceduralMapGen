
namespace FilipRuman.ProceduralMapGen {
    using Godot;
    [Tool, GlobalClass]
    public partial class NoiseController : Node {

        [Export] TerrainGenController terrainGenController;
        [Export] public uint seed;
        [Export] public NoiseComponent[] components;

        [Export] uint terrainSmoothnessNoiseIndex;

        public float GetValue(Vector2 posOnTerrain, Vector3 globalPosition) {
            float totalValue = 0;
            posOnTerrain *= terrainGenController.terrainScale;
            Vector2 noisePosition = new(posOnTerrain.X + globalPosition.X, posOnTerrain.Y + globalPosition.Z);

            float smoothness = 1 - (GetNoise(components[terrainSmoothnessNoiseIndex], noisePosition) + 1) / 2;

            for (int i = 0; i < components.Length; i++) {
                var component = components[i];
                float value = component.valueBasedOnNoise.SampleBaked(GetNoise(component, noisePosition));
                totalValue += component.strength * SmoothnessModifier(smoothness, component) * value;
            }
            return totalValue * terrainGenController.heightModifier;
        }
        float GetNoise(NoiseComponent component, Vector2 pos) {
            pos *= component.frequencyModifier;
            float flipModifier = (component.flipped ? -1 : 1);
            return flipModifier * component.noise.GetNoise2D(pos.X, pos.Y);
        }

        float SmoothnessModifier(float smoothness, NoiseComponent component) => component.detailValueModifier.SampleBaked(smoothness);

    }
}
