using System;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public enum TerrainPreset
    {
        hill,
        chasm // consider adding more, but 2 is enough for now
    }

    public TerrainPreset terrainPreset;

    private TerrainSeed terrainSeed;

    [SerializeField] public List<TerrainChromosome> terrainChromosomes;

    void Start()
    {
        terrainSeed = FindFirstObjectByType<TerrainSeed>();
    }

    public void SetTerrainGenetics()
    {
        TerrainChromosome tc = terrainChromosomes[(int)terrainPreset];
        terrainSeed.terrainChromosome = tc;
        terrainSeed.transform.position = tc.terrainPosition;
        terrainSeed.transform.localScale = tc.terrainScale;
    }

    public void SetAndGenerate()
    {
        SetTerrainGenetics();
        terrainSeed.seed = (int)DateTime.Now.Ticks; // seed RNG with time
        terrainSeed.GenerateTerrainMesh();
    }
}