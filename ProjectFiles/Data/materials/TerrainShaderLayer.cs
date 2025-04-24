using Godot;
[GlobalClass, Tool]
public partial class TerrainShaderLayer : Resource {
    [Export] public float brightness;
    [Export(PropertyHint.ColorNoAlpha)] public Vector3 tint;
    [Export] public GradientTexture1D visibilityHeight;
    [Export] public float tintStrength;
    [Export] public float textureScale;
    [Export] public Texture textures;
    [Export] public float textureSaturation;
}
