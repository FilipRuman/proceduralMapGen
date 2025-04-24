using Godot;
using System.Collections.Generic;
[Tool]
public partial class TerrainShaderConfigurator : Node {
    [Export] TerrainGenController terrainGenController;
    [Export] bool update;
    [Export] ShaderMaterial material;
    [Export] TerrainShaderLayer[] layers;


    [Export] Vector2 minMaxHeight;
    [Export] float globalScale;
    [Export] float globalBrightness;
    [Export] float saturation;


    public override void _Process(double delta) {
        if (layers.Length >= 10)
            GD.PrintErr("Amount of layers is higher that the max amount of layers inside shader, increase the amount of layers inside shader code!");

        if (!Engine.IsEditorHint() || !update)
            return;

        List<float> brightness = new(layers.Length);
        List<Vector3> tint = new(layers.Length);
        List<float> tintStrength = new(layers.Length);
        List<float> textureScale = new(layers.Length);
        List<Texture> textures = new(layers.Length);
        List<Texture> visibilityHeight = new(layers.Length);
        List<float> textureSaturation = new(layers.Length);


        foreach (TerrainShaderLayer layer in layers) {
            brightness.Add(layer.brightness);
            tint.Add(layer.tint);
            tintStrength.Add(layer.tintStrength);
            textureScale.Add(layer.textureScale);
            visibilityHeight.Add(layer.visibilityHeight);
            textures.Add(layer.textures);
            textureSaturation.Add(layer.textureSaturation);
        }

        material.SetShaderParameter("brightness", brightness.ToArray());
        material.SetShaderParameter("tint", tint.ToArray());
        material.SetShaderParameter("tintStrength", tintStrength.ToArray());
        material.SetShaderParameter("visibilityHeight", visibilityHeight.ToArray());
        material.SetShaderParameter("textureSaturation", textureSaturation.ToArray());

        material.SetShaderParameter("textureScale", textureScale.ToArray());
        material.SetShaderParameter("textures", textures.ToArray());


        material.SetShaderParameter("minMaxHeight", minMaxHeight);
        material.SetShaderParameter("globalScale", globalScale);
        material.SetShaderParameter("saturation", saturation);
        material.SetShaderParameter("globalBrightness", globalBrightness);
        material.SetShaderParameter("usedLayersCount", layers.Length);



    }
}
