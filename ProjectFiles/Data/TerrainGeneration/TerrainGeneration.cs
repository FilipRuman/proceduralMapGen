
namespace FilipRuman.ProceduralMapGen {
    using Godot;
    [Tool]
    public partial class TerrainGeneration : Node3D {
        public TerrainGenController terrainGenController;
        [Export] ShaderMaterial material;
        [Export] MeshInstance3D mesh;
        [Export] CollisionShape3D collisionShape;
        [Export] public MeshInstance3D water;
        [Export] int waterResolution = 2;
        public int terrainTrianglesSize = 2;
        public NoiseController noiseController;

        public float waterLevelHeight;

        public float GetHeight(Vector2 posOnTerrain) => noiseController.GetValue(posOnTerrain, GlobalPosition);

        public void UpdateMesh() {
            var waterMesh = new PlaneMesh {
                SubdivideDepth = waterResolution / 2,
                SubdivideWidth = waterResolution / 2,
                Size = Vector2.One * terrainGenController.terrainSize
            };
            water.Mesh = waterMesh;
            water.Position += Vector3.Up * (waterLevelHeight - GlobalPosition.Y);

            var arrayMesh = GenerateTerrainMesh();

            mesh.Mesh = arrayMesh;
            collisionShape.Shape = arrayMesh.CreateTrimeshShape();
            mesh.SetSurfaceOverrideMaterial(0, material);
            mesh.Position = new Vector3(terrainGenController.terrainOffset, 0, terrainGenController.terrainOffset);
        }


        private ArrayMesh GenerateTerrainMesh() {
            var st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);

            int loopDimensionSize = Mathf.CeilToInt(terrainGenController.terrainSize / terrainTrianglesSize);

            GenerateVertexes(loopDimensionSize, st);
            GenerateIndexes(st, loopDimensionSize);

            st.GenerateNormals();
            st.GenerateTangents();

            return st.Commit();
        }

        private static void GenerateIndexes(SurfaceTool st, int loopDimensionSize) {
            var vertexIndex = 0;
            for (int z = 0; z < loopDimensionSize - 1; z++) {
                for (int x = 0; x < loopDimensionSize - 1; x++) {
                    st.AddIndex(vertexIndex + loopDimensionSize + 1);
                    st.AddIndex(vertexIndex + 1);
                    st.AddIndex(vertexIndex);

                    st.AddIndex(vertexIndex);
                    st.AddIndex(vertexIndex + loopDimensionSize);
                    st.AddIndex(vertexIndex + loopDimensionSize + 1);

                    vertexIndex++;
                }
                vertexIndex++;

            }
        }

        private void GenerateVertexes(int loopDimensionSize, SurfaceTool st) {
            float distancePerIndex = terrainTrianglesSize;
            for (uint x = 0; x < loopDimensionSize; x++) {
                for (uint z = 0; z < loopDimensionSize; z++) {
                    var uv = new Vector2(Mathf.InverseLerp(0, loopDimensionSize, x), Mathf.InverseLerp(0, loopDimensionSize, z));
                    st.SetUV(uv);

                    var noisePosition = new Vector2(x, z) * distancePerIndex;
                    var vertex = new Vector3(noisePosition.X, GetHeight(noisePosition), noisePosition.Y);
                    st.AddVertex(vertex);

                }
            }
        }
    }
}
