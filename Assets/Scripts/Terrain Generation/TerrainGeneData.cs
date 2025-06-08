using UnityEngine;

public struct TerrainGeneData
{
    public int terrainGeneType; // Use int for enum
    public float noiseScale;
    public int heightSteps;
    public Bounds2DInt planeBounds;
    public int planeHeight;
    public int relativeToSeed; // Use int (0/1) for bool
    public int plateaus;       // Use int (0/1) for bool
    public float islandFalloff;
}