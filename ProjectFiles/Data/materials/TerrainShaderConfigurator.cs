using Godot;
using System.Collections.Generic;
[Tool]
public partial class TerrainShaderConfigurator : Node {
    [Export] TerrainGenController terrainGenController;
    [Export] ObjectsSpawningController objectsSpawningController;

    [Export] bool update;
    [Export] ShaderMaterial material;
    [Export] TerrainShaderLayer[] layers;


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

        int usedLayersCount;
        if (objectsSpawningController.displayObjectsSpawningRegions) {
            usedLayersCount = objectsSpawningController.objectPool.Length;
            foreach (ObjectSettings objectSetting in objectsSpawningController.objectPool) {
                brightness.Add(1);
                tint.Add(new(objectSetting.spawningRegionColor.R, objectSetting.spawningRegionColor.G, objectSetting.spawningRegionColor.B));
                tintStrength.Add(1);
                textureScale.Add(1);
                textures.Add(new());
                textureSaturation.Add(1);


                var texture = new GradientTexture1D();
                texture.Gradient = new();

                texture.Gradient.Offsets = [
                    objectSetting.heightPercentageRange.X - .01f,
                    objectSetting.heightPercentageRange.X,
                    objectSetting.heightPercentageRange.Y,
                    objectSetting.heightPercentageRange.Y + .01f];
                texture.Gradient.Colors = [
                    Color.Color8(0,0,0),
                    Color.Color8(255,255,255),
                    Color.Color8(255,255,255),
                    Color.Color8(0,0,0)];


                visibilityHeight.Add(texture);
            }

        } else {
            usedLayersCount = layers.Length;
            foreach (TerrainShaderLayer layer in layers) {
                brightness.Add(layer.brightness);
                tint.Add(layer.tint);
                tintStrength.Add(layer.tintStrength);
                textureScale.Add(layer.textureScale);
                visibilityHeight.Add(layer.visibilityHeight);
                textures.Add(layer.textures);
                textureSaturation.Add(layer.textureSaturation);
            }
        }


        material.SetShaderParameter("brightness", brightness.ToArray());
        material.SetShaderParameter("tint", tint.ToArray());
        material.SetShaderParameter("tintStrength", tintStrength.ToArray());
        material.SetShaderParameter("visibilityHeight", visibilityHeight.ToArray());
        material.SetShaderParameter("textureSaturation", textureSaturation.ToArray());

        material.SetShaderParameter("textureScale", textureScale.ToArray());
        material.SetShaderParameter("textures", textures.ToArray());


        material.SetShaderParameter("minMaxHeight", terrainGenController.minMaxHeightForMaterialsAndObjects);
        material.SetShaderParameter("globalScale", globalScale);
        material.SetShaderParameter("saturation", saturation);
        material.SetShaderParameter("globalBrightness", globalBrightness);
        material.SetShaderParameter("usedLayersCount", usedLayersCount);



    }
}
