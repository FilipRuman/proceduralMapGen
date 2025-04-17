using Godot;
[Tool]
public partial class NoiseController : Node {

    [Export] TerrainGenController terrainGenController;
    [Export] float seed;
    [Export] public NoiseComponent[] components;

    [Export] uint terrainSmoothnessNoiseIndex;
    [Export] float heightModifier;

    public float GetValue(Vector2 posOnTerrain, Vector3 globalPosition) {
        float totalValue = 0;
        posOnTerrain *= terrainGenController.terrainScale;
        Vector2 noisePosition = new(posOnTerrain.X + globalPosition.X, posOnTerrain.Y + globalPosition.Z);

        float smoothness = 1 - (GetNoise(components[terrainSmoothnessNoiseIndex], noisePosition) + 1) / 2;

        for (int i = 0; i < components.Length; i++) {
            var component = components[i];
            totalValue += component.strength * SmoothnessModifier(smoothness, component) * GetNoise(component, noisePosition);
        }
        return totalValue * heightModifier;
    }
    float GetNoise(NoiseComponent component, Vector2 pos) {
        pos *= component.frequencyModifier;
        return component.noise.GetNoise2D(pos.X, pos.Y);
    }

    float SmoothnessModifier(float smoothness, NoiseComponent component) => 1 - component.detailLevel * smoothness;

}
