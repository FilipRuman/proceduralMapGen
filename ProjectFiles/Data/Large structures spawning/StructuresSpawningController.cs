using Godot;
using System.Collections.Generic;
[Tool]
public partial class StructuresSpawningController : Node {

	[Export] public TerrainGenController terrainGenController;
	[Export] Structure[] structuresPool;

	[Export] bool regenerateAllStructureInstances = false;
	//WARN: this uses position on terrain grid not a position on structure grid!
	Dictionary<Vector2, StructureInstance> structuresInstancesSortedByPositionOnTerrainGenGrid = new();

	[Export] public bool displayHeightCheckRange;
	[Export] public float structureGenerationRange;
	[Export] public float structureTileSize;
	HashSet<Vector2> structureTileCheckStatus = new();
	[Export] float structureGenerationTimeOffsetS;

	[Export] uint structureInstancingTriesPerTile;
	[Export] Node3D Parent;
	public class StructureInstance {
		public Vector3 position;
		public Structure structure;

		public float scale;
		public float rotation;
	}

	private float generationOffsetTimer;
	public override void _Process(double delta) {
		if (regenerateAllStructureInstances) {
			regenerateAllStructureInstances = false;
			RegenerateStructureInstances();
		}
		generationOffsetTimer += (float)delta;
		if (generationOffsetTimer > structureGenerationTimeOffsetS) {
			generationOffsetTimer = 0;
			GenerateStructures();
		}
		base._Process(delta);
	}
	private void RegenerateStructureInstances() {
		foreach (Node node in Parent.GetChildren()) {
			node.QueueFree();
		}
		structuresInstancesSortedByPositionOnTerrainGenGrid.Clear();
		structureTileCheckStatus.Clear();

		GenerateStructures();
	}
	public override void _Ready() {

		RegenerateStructureInstances();
		base._Ready();
	}

	private void GenerateStructures() {

		WhatTerrainDoYouNeedToLoad(out int maxX, out int minX, out int maxY, out int minY);
		RandomNumberGenerator rng = new();
		for (int x = minX; x < maxX; x++) {
			for (int y = minY; y < maxY; y++) {

				Vector2 positionOnGrid = new(x, y);


				if (structureTileCheckStatus.Contains(positionOnGrid))
					continue;

				structureTileCheckStatus.Add(positionOnGrid);

				rng.Seed = (uint)(x + y + terrainGenController.noiseController.seed);
				var globalPosition = new Vector2(x, y) * structureTileSize;
				for (int i = 0; i < structureInstancingTriesPerTile; i++) {

					var globalSpawnPoint = GetGlobalPositionToSpawn(globalPosition, rng);

					if (!FarEnoughFromStructures(globalSpawnPoint, structure: true))
						continue;

					var viableObjectsList = GetListOfObjectsThatCanBeUsedForThisPoint(globalSpawnPoint);
					if (viableObjectsList.Count == 0) continue;

					Structure validStructure = null;
					foreach (Structure structure in viableObjectsList) {
						if (!IsSpawnPointValid(structure, globalSpawnPoint, false))
							continue;
						validStructure = structure;
						if (displayHeightCheckRange)
							IsSpawnPointValid(structure, globalSpawnPoint, true);
						break;
					}
					if (validStructure == null)
						continue;

					Vector2 posOnTerrainGrid = new(Mathf.FloorToInt(globalPosition.X / terrainGenController.RealTerrainSize), Mathf.FloorToInt(globalPosition.Y / terrainGenController.RealTerrainSize));
					StructureInstance structureInstance = new() {
						structure = validStructure,
						position = globalSpawnPoint,
						scale = rng.RandfRange(validStructure.scaleRange.X,
						validStructure.scaleRange.Y) / terrainGenController.terrainScale,
						rotation = 0
					};
					structuresInstancesSortedByPositionOnTerrainGenGrid.Add(posOnTerrainGrid, structureInstance);
					SpawnStructureScene(structureInstance, Parent);
					break;
				}
			}
		}

		GD.Print($"structuresInstancesSortedByPositionOnTerrainGenGrid.Count {structuresInstancesSortedByPositionOnTerrainGenGrid.Count} {structureTileCheckStatus.Count}");
		// foreach (Vector2 pos in structuresInstancesSortedByPositionOnTerrainGenGrid.Keys) {
		// 	GD.Print($"structure instance pos  {pos}");
		// }
	}
	public bool FarEnoughFromStructures(Vector3 globalPosition, bool structure) {

		foreach (StructureInstance checkedStructureInstance in structuresInstancesSortedByPositionOnTerrainGenGrid.Values) {
			float distBlock = structure ? checkedStructureInstance.structure.structuresSpawningBlockDistance : checkedStructureInstance.structure.objectsSpawningBlockDistance;

			if (globalPosition.DistanceTo(checkedStructureInstance.position) < distBlock)
				return false;
		}
		return true;
	}

	void WhatTerrainDoYouNeedToLoad(out int maxX, out int minX, out int maxY, out int minY) {
		var playerPos = terrainGenController.Player.GlobalPosition;

		maxX = Mathf.CeilToInt((playerPos.X + structureGenerationRange) / structureTileSize);
		minX = Mathf.CeilToInt((playerPos.X - structureGenerationRange) / structureTileSize);

		maxY = Mathf.CeilToInt((playerPos.Z + structureGenerationRange) / structureTileSize);
		minY = Mathf.CeilToInt((playerPos.Z - structureGenerationRange) / structureTileSize);
	}

	// public void SpawnStructureScenesOnTerrain(Vector2 positionOnATerrainGrid, Node parent) {
	//     //
	//     // var globalPos = positionOnATerrainGrid * terrainGenController.RealTerrainSize;
	//     // Vector2 structureGridPosition = globalPos / structureTileSize;
	//     // structureGridPosition.X = Mathf.FloorToInt(structureGridPosition.X);
	//     // structureGridPosition.Y = Mathf.FloorToInt(structureGridPosition.Y);
	//     //
	//     //
	//     // GD.Print($"positionOnATerrainGrid {positionOnATerrainGrid}");
	//     if (structuresInstancesSortedByPositionOnTerrainGenGrid.TryGetValue(positionOnATerrainGrid, out StructureInstance structureOnThisTile))
	//         SpawnStructureScene(structureOnThisTile, parent);
	// }
	private void SpawnStructureScene(StructureInstance structureInstance, Node parent) {
		GD.Print($"SpawnStructureScene {structureInstance.position}");
		var node = (Node3D)structureInstance.structure.scene.Instantiate();
		parent.AddChild(node);

		node.GlobalPosition = structureInstance.position;
		node.Scale = Vector3.One * structureInstance.scale;
		// node.RotationDegrees = new(
		//         rng.RandfRange(-streucture.rotationRange.X, streucture.rotationRange.X),
		//         rng.RandfRange(-streucture.rotationRange.Y, streucture.rotationRange.Y),
		//         rng.RandfRange(-streucture.rotationRange.Z, streucture.rotationRange.Z)
		//         );

	}


	private List<Structure> GetListOfObjectsThatCanBeUsedForThisPoint(Vector3 point) {
		List<Structure> output = new();

		float heightPercentage = Mathf.Clamp(
				Mathf.InverseLerp(terrainGenController.minMaxHeightForMaterialsAndObjects.X, terrainGenController.minMaxHeightForMaterialsAndObjects.Y, point.Y * terrainGenController.terrainScale)
				, 0, 1);

		foreach (Structure structure in structuresPool) {
			if (heightPercentage < structure.heightPercentageRange.X || heightPercentage > structure.heightPercentageRange.Y)
				continue;
			output.Add(structure);
		}
		return output;
	}

	private bool IsSpawnPointValid(Structure structure, Vector3 startPoint, bool debug) {
		float xStepSize = structure.dimensions.X / structure.heightCheckDensity;
		float zStepSize = structure.dimensions.Y / structure.heightCheckDensity;

		float totalVariaton = 0;
		for (int x = 0; x < structure.heightCheckDensity; x++) {
			for (int z = 0; z < structure.heightCheckDensity; z++) {
				Vector2 currentPossition = new Vector2(startPoint.X, startPoint.Z) + new Vector2(x * xStepSize, z * zStepSize) - structure.dimensions / 2f;

				float height = terrainGenController.terrainScale * terrainGenController.noiseController.GetValue(-Vector2.One * terrainGenController.terrainOffset, new Vector3(currentPossition.X, 0, currentPossition.Y));
				if (debug)
					SpawnSphereMesh(new(currentPossition.X, height, currentPossition.Y));
				totalVariaton += Mathf.Abs(startPoint.Y - height);

			}
		}

		if (totalVariaton > structure.maxHeightVariation) {
			return false;
		}
		GD.Print($" height difference  {Mathf.Abs(startPoint.Y - totalVariaton)} at {startPoint}");

		return true;
	}
	void SpawnSphereMesh(Vector3 pos) {
		var ins = new MeshInstance3D();
		Parent.AddChild(ins);
		ins.GlobalPosition = pos;
		var sphere = new SphereMesh();
		sphere.Radius = 100f;
		sphere.Height = 100 * 2f;
		ins.Mesh = sphere;

	}
	Vector3 GetGlobalPositionToSpawn(Vector2 globalPosStartPoint, RandomNumberGenerator rng) {

		float rangeOffset = structureTileSize / 2f;
		float x = rng.RandfRange(globalPosStartPoint.X - rangeOffset, globalPosStartPoint.X + rangeOffset);
		float z = rng.RandfRange(globalPosStartPoint.Y - rangeOffset, globalPosStartPoint.Y + rangeOffset);
		x -= terrainGenController.terrainOffset;
		z -= terrainGenController.terrainOffset;




		float height = terrainGenController.noiseController.GetValue(-Vector2.One * terrainGenController.terrainOffset, new Vector3(x, 0, z));
		height *= terrainGenController.terrainScale;
		return new(x, height, z);
	}


	private Vector2 GetRandomPointToCheck(Vector2 startPoint, RandomNumberGenerator rng) {

		float offset = structureTileSize / 2f;
		return startPoint + new Vector2(rng.RandfRange(offset, -offset), rng.RandfRange(offset, -offset));
	}
}
