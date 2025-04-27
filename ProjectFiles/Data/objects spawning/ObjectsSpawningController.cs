using Godot;
using System.Collections.Generic;
[Tool]
public partial class ObjectsSpawningController : Node3D {

    [Export] private TerrainGenController terrainGenController;
    [Export] private StructuresSpawningController structuresSpawningController;
    [Export] public ObjectSettings[] objectPool;
    [Export] private uint density;

    [Export] public bool displayObjectsSpawningRegions;


    public void SpawnObjectsOnTerrain(Vector2 startGlobalPosition, Node parent) {
        RandomNumberGenerator rng = new();
        rng.Seed = (uint)startGlobalPosition.X + (uint)startGlobalPosition.Y + terrainGenController.noiseController.seed;

        var spaceState = GetWorld3D().DirectSpaceState;
        for (uint i = 0; i < density; i++) {
            var point = GetPositionToSpawn(startGlobalPosition, rng);


            var currentRealGlobalPosition = new Vector3(startGlobalPosition.X + point.X * terrainGenController.terrainScale, 0/*  point.Y  *//* * terrainGenController.terrainScale */, startGlobalPosition.Y + point.Z * terrainGenController.terrainScale);
            if (!structuresSpawningController.FarEnoughFromStructures(currentRealGlobalPosition, structure: false))
                continue;

            var viableObjectsList = GetListOfObjectsThatCanBeUsedForThisPoint(point);
            if (viableObjectsList.Count == 0) continue;

            var objectToSpawn = viableObjectsList[rng.RandiRange(0, viableObjectsList.Count - 1)];
            SpawnObjectAtPoint(point, objectToSpawn, rng, parent);
        }
    }
    private void SpawnObjectAtPoint(Vector3 point, ObjectSettings objectToSpawn, RandomNumberGenerator rng, Node parent) {
        var node = (Node3D)objectToSpawn.scene.Instantiate();
        node.Position = point;
        node.Scale = Vector3.One * rng.RandfRange(objectToSpawn.scaleRange.X, objectToSpawn.scaleRange.Y) / terrainGenController.terrainScale;
        node.RotationDegrees = new(
                rng.RandfRange(-objectToSpawn.rotationRange.X, objectToSpawn.rotationRange.X),
                rng.RandfRange(-objectToSpawn.rotationRange.Y, objectToSpawn.rotationRange.Y),
                rng.RandfRange(-objectToSpawn.rotationRange.Z, objectToSpawn.rotationRange.Z)
                );

        parent.AddChild(node);
    }

    private List<ObjectSettings> GetListOfObjectsThatCanBeUsedForThisPoint(Vector3 point) {
        List<ObjectSettings> output = new();

        float heightPercentage = Mathf.Clamp(
                Mathf.InverseLerp(terrainGenController.minMaxHeightForMaterialsAndObjects.X, terrainGenController.minMaxHeightForMaterialsAndObjects.Y, point.Y * terrainGenController.terrainScale)
                , 0, 1);

        foreach (ObjectSettings @object in objectPool) {
            if (heightPercentage < @object.heightPercentageRange.X || heightPercentage > @object.heightPercentageRange.Y)
                continue;
            output.Add(@object);
        }
        return output;
    }

    private Vector3 GetPositionToSpawn(Vector2 globalPosition, RandomNumberGenerator rng) {
        var pointToCheck = GetRandomPointToCheck(globalPosition, rng);
        var height = terrainGenController.noiseController.GetValue(pointToCheck - Vector2.One * terrainGenController.terrainOffset, new(globalPosition.X, 0, globalPosition.Y));

        return new(pointToCheck.X, height, pointToCheck.Y);
    }
    private Vector2 GetRandomPointToCheck(Vector2 globalPosition, RandomNumberGenerator rng) {
        float offset = terrainGenController.terrainSize / 2f;
        return new(rng.RandfRange(offset, -offset), rng.RandfRange(offset, -offset));
    }
}
