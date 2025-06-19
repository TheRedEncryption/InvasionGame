using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TerrainSeed is responsible for procedural terrain mesh generation using a gene-based system.
/// It supports noise, plane, and island (falloff) modifications, and assigns submeshes for different terrain materials.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainSeed : MonoBehaviour
{
    // Mesh variables
    private MeshFilter meshFilter;    // Reference to the MeshFilter component
    private Mesh mesh;                // The generated mesh
    private MeshCollider meshCollider;// Reference to the MeshCollider component

    // Width and depth of the plane to generate over
    public int Width { get; private set; }
    public int Depth { get; private set; }

    // Minimum and maximum heights for the terrain

    // Vertical boundary bottom
    [SerializeField] private float verticalBoundaryBottom = -50;

    private float rootArea;           // Used for falloff calculations
    private Vector2 origin;           // Center of the terrain

    // To control the terrain
    [Header("Terrain Controls")]
    public int seed;                        // Seed for randomization
    [SerializeField] private Bounds2DInt terrainBoundary;     // Terrain bounds

    // Terrain Chromosomes (ScriptableObject lists of TerrainGenes)
    private TerrainChromosome _terrainChromosome;
    [SerializeField]
    public TerrainChromosome terrainChromosome
    {
        get => _terrainChromosome;
        set
        {
            _terrainChromosome = value;
            terrainGenes = value.terrainGenes;
        }
    }
    private List<TerrainGene> terrainGenes;                    // List of genes to apply

    private int offset; // Noise offset for Perlin noise

    // Lists for the vertices and triangles that will then be fed into the array
    private List<Vector3> vertexList = new List<Vector3>(); // List of mesh vertices
    private List<int> triangleList = new List<int>();       // List of mesh triangle indices
    private List<Vector2> uvList = new List<Vector2>();     // List of UV coordinates

    // To visualize the mesh values in the inspector
    [Header("Mesh Value Visualization")]
    [SerializeField] private Vector3[] vertexArray;           // Array of mesh vertices
    [SerializeField] private int[] triangleArray;             // Array of mesh triangles
    [SerializeField] private Vector2[] uvArray;               // Array of mesh UVs

    // Generation Enums
    public enum GeneType
    {
        none,
        noise,
        plane,
        island
    }

    [Header("Material Controls")]

    // Material cutoff points for submesh assignment
    [SerializeField] private float rockToSnow = 50;           // Elevation cutoff for snow
    [SerializeField] private float grassToRock = 25;          // Elevation cutoff for rock
    [SerializeField] private float sandCreepInland = 10;      // Elevation cutoff for sand
    [SerializeField] private int vertexCutoffTolerance = 1;   // Number of vertices below cutoff to assign triangle

    [SerializeField, Range(0f, 1f)] private float percentHeightRockThreshold = 0.5f; // If any side of the triangle is longer than this percentage of the height of the terrain, then it will be assigned "rock" regardless of elevation.

    /// <summary>
    /// Initializes the terrain seed, sets up mesh filter, and generates the terrain mesh.
    /// </summary>
    void Start()
    {
        UnityEngine.Random.InitState(seed); // Allows for reproducibility
        offset = UnityEngine.Random.Range(0, 65535); // Simulate seeding for noise

        meshFilter = GetComponent<MeshFilter>();

        // GenerateTerrainMesh(); // For testing; should be called by map loading system in the future
    }

    #region GENERATION

    /// <summary>
    /// Generates the terrain mesh by initializing vertices, applying genes, and creating triangles and submeshes.
    /// </summary>
    public void GenerateTerrainMesh()
    {
        UpdateSizeProperties();

        // Center the terrain object
        transform.position = new Vector3(origin.x, transform.position.y, origin.y);

        meshFilter.mesh.Clear(false);
        mesh = new Mesh();

        // clear lists in case of re-generation
        vertexList.Clear();
        triangleList.Clear();
        uvList.Clear();

        InitializeVertexGrid();

        EvaluateAllGenes();

        InitializeTriangleArray();

        ConstructUVList();

        CopyListsToArrays();

        mesh.subMeshCount = 4;

        CreateSubmeshes();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    /// <summary>
    /// Copy lists to arrays for mesh assignment.
    /// </summary>
    private void CopyListsToArrays()
    {
        vertexArray = new Vector3[vertexList.Count];
        mesh.vertices = new Vector3[vertexList.Count];
        vertexList.CopyTo(vertexArray);

        triangleArray = new int[triangleList.Count];
        mesh.triangles = new int[triangleList.Count];
        triangleList.CopyTo(triangleArray);

        uvArray = new Vector2[uvList.Count];
        mesh.uv = new Vector2[uvList.Count];
        uvList.CopyTo(uvArray);

        // Final assignment
        mesh.vertices = vertexArray;
        mesh.triangles = triangleArray;
        mesh.uv = uvArray;
    }

    #region > INITIALIZATION

    /// <summary>
    /// Updates width, depth, root area, and origin based on terrain boundaries.
    /// </summary>
    private void UpdateSizeProperties()
    {
        Width = terrainBoundary.Size.x + 1;
        Depth = terrainBoundary.Size.y + 1; // Y channel for Z values
        rootArea = Mathf.Sqrt(Width * Depth);
        origin = new Vector2((terrainBoundary.maxX + terrainBoundary.minX) / 2, (terrainBoundary.maxZ + terrainBoundary.minZ) / 2);
    }

    /// <summary>
    /// Initializes the grid of vertices for the mesh.
    /// </summary>
    private void InitializeVertexGrid()
    {
        vertexList.Clear(); // Ensure list is empty

        for (int zRow = 0; zRow <= Depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= Width - 1; xCol++)
            {
                vertexList.Add(new Vector3(xCol - Width / 2, transform.position.y, zRow - Depth / 2));
            }
        }
    }

    /// <summary>
    /// Initializes the triangle indices for the mesh.
    /// </summary>
    private void InitializeTriangleArray()
    {
        for (int zRow = 0; zRow < Depth - 1; zRow++)
        {
            for (int xCol = 0; xCol < Width - 1; xCol++)
            {
                int k = zRow * Width + xCol;

                GenerateSingleSquare(k);
            }
        }
    }

    /// <summary>
    /// Generates two triangles for a single square in the grid.
    /// </summary>
    private void GenerateSingleSquare(int k)
    {
        triangleList.Add(k);             // A
        triangleList.Add(k + Width);     // B
        triangleList.Add(k + 1);         // C

        triangleList.Add(k + 1);         // C
        triangleList.Add(k + Width);     // B
        triangleList.Add(k + Width + 1); // D
    }

    #endregion > INITIALIZATION

    #region > GENETICS

    /// <summary>
    /// Applies all terrain genes to each vertex in the grid.
    /// </summary>
    private void EvaluateAllGenes()
    {
        for (int zRow = 0; zRow <= Depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= Width - 1; xCol++)
            {
                for (int currentGene = 0; currentGene < terrainGenes.Count; currentGene++)
                {
                    EvaluateGene(terrainGenes[currentGene], xCol, zRow);
                }
            }
        }
    }

    /// <summary>
    /// Applies a single gene to a vertex at (x, z).
    /// </summary>
    private void EvaluateGene(TerrainGene gene, int x, int z)
    {
        switch (gene.terrainGeneType)
        {
            case GeneType.noise:
                ApplyNoise(gene.noiseScale, gene.heightSteps, x, z);
                break;
            case GeneType.plane:
                ApplyPlane(gene.planeHeight, gene.planeBounds, gene.relativeToSeed, gene.plateaus, x, z);
                break;
            case GeneType.island:
                ApplyFalloff(gene.islandFalloff, x, z);
                break;
            case GeneType.none:
            default:
                break;
        }
    }
    #endregion > GENETICS

    #region > NOISE

    /// <summary>
    /// Applies Perlin noise to a vertex at (xCol, zRow).
    /// </summary>
    private void ApplyNoise(float noiseScale, int heightSteps, int zRow, int xCol)
    {
        Vector3 curr = vertexList[zRow * Width + xCol];
        int newVertexElevation = GenerateNoiseAt(xCol, zRow, noiseScale, heightSteps);
        vertexList[zRow * Width + xCol] = new Vector3(curr.x, curr.y + newVertexElevation, curr.z);

    }

    /// <summary>
    /// Generates a noise-based elevation value for a given position.
    /// </summary>
    private int GenerateNoiseAt(int xPos, int zPos, float noiseScale, int heightSteps)
    {
        // Convert from world to noise space
        float xCoord = xPos * noiseScale + offset;
        float zCoord = zPos * noiseScale + offset;

        // Get Perlin value and scale it, then floor
        float perlinValue = Mathf.PerlinNoise(xCoord, zCoord);
        int newY = Mathf.FloorToInt(perlinValue * heightSteps + 1);

        return newY;
    }

    #endregion > NOISE

    #region > ISLAND

    /// <summary>
    /// Applies a falloff (island effect) to a vertex at (xCol, zRow).
    /// </summary>
    private void ApplyFalloff(float islandFalloffScale, int zRow, int xCol)
    {
        vertexList[zRow * Width + xCol] -= new Vector3(0f, Falloff(xCol + (int)origin.x - (Width / 2), zRow + (int)origin.y - (Depth / 2), islandFalloffScale), 0f);
    }

    /// <summary>
    /// Calculates the falloff value based on distance from the origin.
    /// </summary>
    private int Falloff(int x, int z, float falloffScale)
    {
        Vector2 point = new Vector2Int(x, z);
        float distance = (point - origin).magnitude;
        return (int)(distance * distance * falloffScale / rootArea);
    }

    #endregion > ISLAND

    #region > PLANE

    /// <summary>
    /// Applies a flat plane elevation to vertices within the specified bounds.
    /// </summary>
    private void ApplyPlane(int planeElevation, Bounds2DInt planeBounds, bool isRelativeToSeed, bool plateaus, int zRow, int xCol)
    {
        Vector3 current = vertexList[zRow * Width + xCol];
        Vector3 adjusted = current + transform.position;

        if (plateaus)
        {
            if (isRelativeToSeed)
            {
                if (current.y + transform.position.y <= planeElevation)
                {
                    return;
                }
            }
            else
            {
                if (current.y <= planeElevation)
                {
                    return;
                }
            }
        }

        if (adjusted.x >= planeBounds.minX && adjusted.x <= planeBounds.maxX && adjusted.z >= planeBounds.minZ && adjusted.z <= planeBounds.maxZ)
        {
            if (isRelativeToSeed)
            {
                vertexList[zRow * Width + xCol] = new Vector3(current.x, planeElevation - transform.position.y, current.z);
            }
            else
            {
                vertexList[zRow * Width + xCol] = new Vector3(current.x, planeElevation, current.z);
            }
        }
    }

    #endregion > PLANE

    #region > MATERIALS
    // Based on elevation, assign the four materials to the mesh (grass = 0, mountain rock = 1, snow caps = 2, sand = 3) and use Mesh.SetTriangles to assign submeshes.

    /// <summary>
    /// Assigns triangles to submeshes based on vertex elevation for material blending.
    /// </summary>
    private void CreateSubmeshes()
    {
        // Prepare lists for each submesh
        List<int> sandTriangles = new List<int>();
        List<int> grassTriangles = new List<int>();
        List<int> rockTriangles = new List<int>();
        List<int> snowTriangles = new List<int>();

        // Find min and max elevation for normalization (optional, or use your cutoffs)
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var v in mesh.vertices)
        {
            if (v.y < minY) minY = v.y;
            if (v.y > maxY) maxY = v.y;
        }

        // Loop through triangles
        for (int i = 0; i < triangleArray.Length; i += 3)
        {
            int i0 = triangleArray[i];
            int i1 = triangleArray[i + 1];
            int i2 = triangleArray[i + 2];

            Vector3 v0 = mesh.vertices[i0];
            Vector3 v1 = mesh.vertices[i1];
            Vector3 v2 = mesh.vertices[i2];

            // Assign to submesh based on elevation using TriangleBelowCutoff
            if (TriangleBelowCutoff(v0, v1, v2, sandCreepInland))
            {
                sandTriangles.Add(i0); sandTriangles.Add(i1); sandTriangles.Add(i2);
            }
            else if (TriangleBelowCutoff(v0, v1, v2, grassToRock))
            {
                grassTriangles.Add(i0); grassTriangles.Add(i1); grassTriangles.Add(i2);
            }
            else if (TriangleBelowCutoff(v0, v1, v2, rockToSnow))
            {
                rockTriangles.Add(i0); rockTriangles.Add(i1); rockTriangles.Add(i2);
            }
            else
            {
                snowTriangles.Add(i0); snowTriangles.Add(i1); snowTriangles.Add(i2);
            }
        }

        mesh.subMeshCount = 4;
        mesh.SetTriangles(grassTriangles, 0);
        mesh.SetTriangles(rockTriangles, 1);
        mesh.SetTriangles(snowTriangles, 2);
        mesh.SetTriangles(sandTriangles, 3);
    }

    /// <summary>
    /// Determines if a triangle should be assigned to a submesh based on how many vertices are below a cutoff.
    /// </summary>
    private bool TriangleBelowCutoff(Vector3 v0, Vector3 v1, Vector3 v2, float cutoff)
    {
        int belowCount = 0;
        if (v0.y < cutoff) belowCount++;
        if (v1.y < cutoff) belowCount++;
        if (v2.y < cutoff) belowCount++;

        return belowCount >= vertexCutoffTolerance;
    }

    #endregion > MATERIALS

    #region > UV MAPPING

    /// <summary>
    /// Loops over every vertex and adds the necessary UVs.
    /// </summary>
    private void ConstructUVList()
    {
        for (int i = 0; i < vertexList.Count; i++)
        {
            InsertUVAtEnd(i);
        }
    }

    /// <summary>
    /// Inserts UV coordinates at the end of uvList based on the vertices provided.
    /// </summary>
    private void InsertUVAtEnd(int vertexIndex)
    {
        int x = vertexIndex % Width;
        int z = vertexIndex / Width;
        uvList.Add(new Vector2(x, z));
    }

    #endregion > UV MAPPING

    #endregion GENERATION

    #region DEBUG

    /// <summary>
    /// Draws the terrain boundary in the editor for visualization.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Vector3 bottomLeft = new Vector3(terrainBoundary.minX, transform.position.y, terrainBoundary.minZ);
        Vector3 bottomRight = new Vector3(terrainBoundary.maxX, transform.position.y, terrainBoundary.minZ);
        Vector3 topLeft = new Vector3(terrainBoundary.minX, transform.position.y, terrainBoundary.maxZ);
        Vector3 topRight = new Vector3(terrainBoundary.maxX, transform.position.y, terrainBoundary.maxZ);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    #endregion
}