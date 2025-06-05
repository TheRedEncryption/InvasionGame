using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainSeed : MonoBehaviour
{
    // Mesh variables
    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshCollider meshCollider;

    // Lists for the vertices and triangles that will then be fed into the array
    private List<Vector3> vertexList = new List<Vector3>();
    private List<int> triangleList = new List<int>();

    // width and depth of the plane to generate over
    private int width;
    private int depth;

    private float rootArea;
    private Vector2 origin;

    // To control the terrain
    [Header("Terrain Controls")]
    [SerializeField] private int seed;
    [SerializeField] private Bounds2DInt terrainBoundary;

    public List<TerrainGene> terrainGenes;

    // [SerializeField] private float noiseScale = 0.1f;
    private int offset; // noise offset
    // [SerializeField] private int heightSteps = 10;
    // [SerializeField, Range(0f, 1f)] private float islandFalloff;
    // [SerializeField] private int heightBias;
    // [SerializeField] private bool isIsland = true; // control whether or not this is an island before generation 


    // To visualize the mesh values
    [Header("Mesh Value Visualization")]
    [SerializeField] private Vector3[] vertexArray;
    [SerializeField] private int[] triangleArray;
    [SerializeField] private Vector3[] normalArray;

    // Generation Enums
    public enum GeneType
    {
        none,
        noise,
        plane,
        island
    }

    void Start()
    {
        UnityEngine.Random.InitState(seed); // allows for reproducibility
        offset = UnityEngine.Random.Range(0, 65535); // simulate seeding!

        meshFilter = GetComponent<MeshFilter>();

        GenerateTerrainMesh(); // TODO: remove, for now this is for testing purposes but this must be called by the map 
                               // loading system (to be implemented in the future)
    }

    #region GENERATION

    public void GenerateTerrainMesh()
    {
        UpdateSizeProperties();

        transform.position = new Vector3(origin.x, transform.position.y, origin.y);

        meshFilter.mesh.Clear();
        mesh = new Mesh();

        InitializeVertexGrid();

        for (int currentGene = 0; currentGene < terrainGenes.Count; currentGene++)
        {
            EvaluateGene(terrainGenes[currentGene]);
        }

        InitializeTriangleArray();

        vertexArray = new Vector3[vertexList.Count];
        mesh.vertices = new Vector3[vertexList.Count];
        vertexList.CopyTo(vertexArray);

        triangleArray = new int[triangleList.Count];
        mesh.triangles = new int[triangleList.Count];
        triangleList.CopyTo(triangleArray);

        mesh.vertices = vertexArray;
        mesh.triangles = triangleArray;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    #region > INITIALIZATION

    private void UpdateSizeProperties()
    {
        width = terrainBoundary.Size.x + 1;
        depth = terrainBoundary.Size.y + 1; // we are using the Y channel for Z values
        rootArea = Mathf.Sqrt(width * depth);
        origin = new Vector2((terrainBoundary.maxX + terrainBoundary.minX) / 2, (terrainBoundary.maxZ + terrainBoundary.minZ) / 2);
    }

    private void InitializeVertexGrid()
    {
        vertexList.Clear(); // just to be safe

        for (int zRow = 0; zRow <= depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                vertexList.Add(new Vector3(xCol - width / 2, transform.position.y, zRow - depth / 2));
            }
        }
    }

    private void InitializeTriangleArray()
    {
        for (int zRow = 0; zRow < depth - 1; zRow++)
        {
            for (int xCol = 0; xCol < width - 1; xCol++)
            {
                int k = zRow * width + xCol;

                GenerateSingleSquare(k);
            }
        }
    }

    private void GenerateSingleSquare(int k)
    {
        triangleList.Add(k);             // A
        triangleList.Add(k + width);     // B
        triangleList.Add(k + 1);         // C

        triangleList.Add(k + 1);         // C
        triangleList.Add(k + width);     // B
        triangleList.Add(k + width + 1); // D
    }

    #endregion > INITIALIZATION

    private void EvaluateGene(TerrainGene gene)
    {
        switch (gene.terrainGeneType)
        {
            case GeneType.noise:
                ApplyNoise(gene.noiseScale, gene.heightSteps);
                break;
            case GeneType.plane:
                ApplyPlane(gene.planeHeight, gene.planeBounds, gene.relativeToSeed);
                break;
            case GeneType.island:
                ApplyFalloff(gene.islandFalloff);
                break;
            case GeneType.none:
            default:
                break;
        }
    }

    #region > NOISE

    private void ApplyNoise(float noiseScale, int heightSteps)
    {
        for (int zRow = 0; zRow <= depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                Vector3 curr = vertexList[zRow * width + xCol];
                int newVertexElevation = GenerateNoiseAt(xCol, zRow, noiseScale, heightSteps);
                vertexList[zRow * width + xCol] = new Vector3(curr.x, curr.y + newVertexElevation, curr.z);
            }
        }
    }

    private int GenerateNoiseAt(int xPos, int zPos, float noiseScale, int heightSteps)
    {
        // first, convert from world to noise space
        float xCoord = xPos * noiseScale + offset;
        float zCoord = zPos * noiseScale + offset;

        // get perlin value and scale it, remembering to floor it afterwards
        float perlinValue = Mathf.PerlinNoise(xCoord, zCoord);
        int newY = Mathf.FloorToInt(perlinValue * heightSteps + 1);

        return newY;
    }

    #endregion > NOISE

    #region > ISLAND

    private void ApplyFalloff(float islandFalloffScale)
    {
        for (int zRow = 0; zRow <= depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                vertexList[zRow * width + xCol] -= new Vector3(0f, Falloff(xCol + (int)origin.x - (width / 2), zRow + (int)origin.y - (depth / 2), islandFalloffScale), 0f);
            }
        }
    }

    private int Falloff(int x, int z, float falloffScale)
    {
        Vector2 point = new Vector2Int(x, z);
        float distance = (point - origin).magnitude;
        Debug.Log("Point: " + point + "; Origin: " + origin + ";");
        return (int)(distance * distance * falloffScale / rootArea);
    }

    #endregion > ISLAND

    #region > PLANE

    private void ApplyPlane(int planeElevation, Bounds2DInt planeBounds, bool isRelativeToSeed)
    {
        for (int zRow = 0; zRow <= depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                Vector3 current = vertexList[zRow * width + xCol];
                if (current.x >= planeBounds.minX && current.x <= planeBounds.maxX && current.z >= planeBounds.minZ && current.z <= planeBounds.maxZ)
                {
                    if (isRelativeToSeed)
                    {
                        vertexList[zRow * width + xCol] = new Vector3(current.x, current.y + planeElevation, current.z);
                    }
                    else
                    {
                        vertexList[zRow * width + xCol] = new Vector3(current.x, planeElevation, current.z);
                    }
                }
            }
        }
    }

    #endregion > PLANE

    #endregion GENERATION

    #region DEBUG

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