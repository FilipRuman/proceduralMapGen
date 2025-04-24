using Godot;
using System.Collections.Generic;
[Tool]
public partial class TerrainGenController : Node {

    [Export] ObjectsSpawningController objectsSpawningController;
    [Export] bool forceRegenerate = new();
    [Export] PackedScene TerrainGenerationPrefab;
    [Export] Node3D terrainLayout;
    [Export] public Node3D Player;

    [Export] float ViewDistance;
    Dictionary<Vector2I, TerrainGeneration> terrainDisplays = new();

    public float terrainSize => terrainRawSize + terrainMarginSize;
    public float terrainOffset => -terrainSize / 2f;
    [Export] private float terrainMarginSize;
    [Export] private float terrainRawSize = 200;



    [Export] public float heightModifier;
    [Export] public float terrainScale;
    [Export] int framesPerUpdate = 100;
    [Export] int terrainTrianglesSize;
    [Export] float waterLevelHeight;
    [Export] public float terrainBaseHeight = 2000;

    [Export] public NoiseController noiseController;

    int frameIndex = 0;



    int lastMaxX, lastMinX, lastMaxY, lastMinY;

    bool spawningInProgress = false;

    bool LoadNewTerrain(int maxX, int minX, int maxY, int minY) {
        var returnValue = !(lastMaxX == maxX && lastMinX == minX && lastMinY == minY && lastMaxY == maxY);
        if (returnValue) {
            lastMaxX = maxX;
            lastMinX = minX;
            lastMinY = minY;
            lastMaxY = maxY;
        }
        return returnValue;
    }
    public override void _Process(double delta) {
        if (spawningInProgress)
            SpreadSpawningTerrain(terrainDisplays.Count == 0);
        frameIndex++;
        if (frameIndex < framesPerUpdate) return;
        frameIndex = 0;
        if (terrainLayout == null) return;
        if (Player == null) return;

        WhatTerrainDoYouNeedToLoad(out maxX, out minX, out maxY, out minY);

        if (!LoadNewTerrain(maxX, minX, maxY, minY) && !forceRegenerate) return;
        if (forceRegenerate)
            Regenerate();

        forceRegenerate = false;

        RemoveNotNeededTerrain(maxX, minX, maxY, minY);


        spawningInProgress = true;
        base._Process(delta);
    }

    private void Regenerate() {
        foreach (var item in terrainLayout.GetChildren()) {
            item.QueueFree();
        }
        terrainDisplays.Clear();
    }
    int minX;
    int maxX;
    int minY;
    int maxY;
    [Export] int terrainsPerFrame = 2;
    void SpreadSpawningTerrain(bool spawnAll) {
        int spawnedTerrains = 0;
        for (int x = minX; x < maxX; x++) {
            for (int y = minY; y < maxY; y++) {
                Vector2I key = new(x, y);
                if (terrainDisplays.ContainsKey(key))
                    continue;

                SpawnTerrain(key, out Vector3 spawnPoint, out Node3D node);
                objectsSpawningController.SpawnObjectsOnTerrain(new Vector2(spawnPoint.X, spawnPoint.Z), node);

                spawnedTerrains++;

                if (terrainsPerFrame == spawnedTerrains && !spawnAll)
                    return;
            }
        }
        spawningInProgress = false;
    }

    void RemoveNotNeededTerrain(int maxX, int minX, int maxY, int minY) {
        List<Vector2I> toRemove = new();
        foreach (var terrainValePair in terrainDisplays) {
            if (terrainValePair.Key.X < minX || terrainValePair.Key.X > maxX ||
                terrainValePair.Key.Y < minY || terrainValePair.Key.Y > maxY) {
                terrainValePair.Value.QueueFree();
                toRemove.Add(terrainValePair.Key);
            }
        }

        foreach (var item in toRemove) {
            terrainDisplays.Remove(item);
        }
    }

    void WhatTerrainDoYouNeedToLoad(out int maxX, out int minX, out int maxY, out int minY) {
        var playerPos = Player.GlobalPosition;

        maxX = Mathf.CeilToInt((playerPos.X + ViewDistance) / RealTerrainSize);
        minX = Mathf.CeilToInt((playerPos.X - ViewDistance) / RealTerrainSize);

        maxY = Mathf.CeilToInt((playerPos.Z + ViewDistance) / RealTerrainSize);
        minY = Mathf.CeilToInt((playerPos.Z - ViewDistance) / RealTerrainSize);
    }

    float RealTerrainSize => terrainRawSize * terrainScale;
    void SpawnTerrain(Vector2I positionOnAGrid, out Vector3 spawnPoint, out Node3D node) {
        node = (Node3D)TerrainGenerationPrefab.Instantiate(PackedScene.GenEditState.Main);
        terrainLayout.AddChild(node);
        var terrain = (TerrainGeneration)node;

        terrainDisplays.Add(positionOnAGrid, terrain);

        terrain.terrainGenController = this;

        terrain.noiseController = noiseController;
        terrain.waterLevelHeight = waterLevelHeight;
        terrain.terrainTrianglesSize = terrainTrianglesSize;

        terrain.Scale = new Vector3(terrainScale, terrainScale, terrainScale);

        spawnPoint = new(positionOnAGrid.X * RealTerrainSize, terrainBaseHeight, positionOnAGrid.Y * RealTerrainSize);
        terrain.Position = spawnPoint;

        terrain.UpdateMesh();
    }


}
