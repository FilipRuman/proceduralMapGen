using Godot;
using System.Collections.Generic;
[Tool]
public partial class ObjectsSpawningController : Node3D {
    [Export] public TerrainGenController terrainGenController;
    [Export] public ObjectSettings[] objectPool;
    [Export] uint density;


    public void SpawnObjectsOnTerrain(Vector2 globalPosition, Node parent) {
        //Objects are instantiating under the terrain node that is scaled so i have to scale positions back
        // globalPosition /= terrainGenController.terrainScale;
        RandomNumberGenerator rng = new();
        rng.Seed = (uint)globalPosition.X + (uint)globalPosition.Y + terrainGenController.noiseController.seed;

        var spaceState = GetWorld3D().DirectSpaceState;
        for (uint i = 0; i < density; i++) {
            var point = GetPositionToSpawn(globalPosition, rng, spaceState);


            var viableObjectsList = /* GetListOfObjectsThatCanBeUsedForThisPoint(point) */ objectPool;
            // if (viableObjectsList.Count == 0) continue;

            var objectToSpawn = viableObjectsList[rng.RandiRange(0, viableObjectsList.Length - 1)];
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
        foreach (ObjectSettings @object in objectPool) {
            if (point.Y < @object.heightRange.X || point.Y > @object.heightRange.Y)
                continue;
            output.Add(@object);
        }
        return output;
    }

    private Vector3 GetPositionToSpawn(Vector2 globalPosition, RandomNumberGenerator rng, PhysicsDirectSpaceState3D spaceState) {
        // while (true) {


        var pointToCheck = GetRandomPointToCheck(globalPosition, rng);
        //TODO: Add scale etc.
        var height = terrainGenController.noiseController.GetValue(pointToCheck, new(globalPosition.X, 0, globalPosition.Y));


        return new(pointToCheck.X, height, pointToCheck.Y);
        // }
    }
    private Vector2 GetRandomPointToCheck(Vector2 globalPosition, RandomNumberGenerator rng) {

        float offset = terrainGenController.terrainSize / 2f;
        return new(rng.RandfRange(offset, -offset), rng.RandfRange(offset, -offset));
    }
}
