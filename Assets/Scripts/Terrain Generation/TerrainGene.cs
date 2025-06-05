using UnityEngine;

[System.Serializable]
public class TerrainGene
{
    public TerrainSeed.GeneType terrainGeneType;

    // "noise"
    public float noiseScale;
    public int heightSteps;

    // "plane"
    public Bounds2DInt planeBounds;
    public int planeHeight;
    public bool relativeToSeed;

    // "island"
    [Range(0f, 1f)] public float islandFalloff;

    // constructors
    public TerrainGene()
    {
        terrainGeneType = TerrainSeed.GeneType.none;
        noiseScale = 0.1f;
        heightSteps = 10;
        planeHeight = 0;
        islandFalloff = 0.5f;
        relativeToSeed = false;
    }
}
