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
    /// <summary>
    /// The falloff value for the island gene, which determines how quickly the terrain height decreases towards the edges of the island. Must be at least 0 and preferably capped at 1, but may be exceeded if desired for extreme effects.
    /// </summary>
    public float islandFalloff;

    // constructors
    public TerrainGene()
    {
        terrainGeneType = TerrainSeed.GeneType.noise;
        noiseScale = 0.1f;
        heightSteps = 10;
        planeHeight = 0;
        islandFalloff = 0.5f;
        relativeToSeed = false;
    }
}
