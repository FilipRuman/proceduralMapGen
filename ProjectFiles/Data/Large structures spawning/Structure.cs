using Godot;
using System;
[Tool, GlobalClass]
public partial class Structure : Resource {
    [Export] public PackedScene scene;
    [Export] public Vector2 scaleRange;
    [Export(PropertyHint.Range, "0,1,")] public Vector2 heightPercentageRange;
    [Export] public Vector2 dimensions;
    [Export] public float heightCheckDensity;
    [Export] public float maxHeightVariation;

    [Export] public float structuresSpawningBlockDistance;
    [Export] public float objectsSpawningBlockDistance;
}
