using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chromosome", menuName = "TRETerrain/Create New Chromosome", order = 0)]
public class TerrainChromosome : ScriptableObject
{
    // Transform information
    public Vector3 terrainPosition = new Vector3();
    public Vector3 terrainScale = new Vector3();

    // Genetic terrain information
    public List<TerrainGene> terrainGenes = new List<TerrainGene>();
}
