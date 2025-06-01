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
    private List<Vector3> normalList = new List<Vector3>();

    // width and depth of the plane to generate over
    private int width;
    private int depth;

    // To control the terrain
    [Header("Terrain Controls")]
    [SerializeField] private int seed;
    [SerializeField] private Bounds2DInt terrainBoundary;
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private int heightSteps = 10;
    [SerializeField] private int islandFalloff;
    [SerializeField] private int heightBias;
    private int offset;


    // To visualize the mesh values
    [Header("Mesh Value Visualization")]
    [SerializeField] private Vector3[] vertexArray;
    [SerializeField] private int[] triangleArray;
    [SerializeField] private Vector3[] normalArray;

    void Start()
    {
        UnityEngine.Random.InitState(seed); // allows for reproducibility
        offset = UnityEngine.Random.Range(0, 65535); // simulate seeding!

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        width = terrainBoundary.Size.x + 1;
        depth = terrainBoundary.Size.y + 1; // we are using the Y channel for Z values

        GenerateTerrainMesh();

        meshFilter.mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
    }

    #region GENERATION

    public void GenerateTerrainMesh()
    {
        InitializeVertexGrid();

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
    }

    private void InitializeVertexGrid()
    {
        vertexList.Clear(); // just to be safe

        for (int zRow = 0; zRow <= depth - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                Vector3Int voxelVertex = GenerateVertex(xCol, zRow);
                vertexList.Add(voxelVertex);
            }
        }
    }

    private Vector3Int GenerateVertex(int xPos, int zPos)
    {
        float xCoord = xPos * noiseScale + offset;
        float zCoord = zPos * noiseScale + offset;

        float perlinValue = Mathf.PerlinNoise(xCoord, zCoord);
        int newY = Mathf.FloorToInt(perlinValue * heightSteps);

        Vector2 origin = new Vector2((terrainBoundary.maxX + terrainBoundary.minX) / 2, (terrainBoundary.maxZ + terrainBoundary.minZ) / 2);

        newY -= Falloff(xPos + terrainBoundary.minX, zPos + terrainBoundary.minZ, origin, islandFalloff);

        return new Vector3Int(xPos + terrainBoundary.minX, newY + heightBias, zPos + terrainBoundary.minZ);
    }

    private int Falloff(int x, int z, Vector2 origin, int falloffScale)
    {
        Vector2 point = new Vector2Int(x, z);
        float distance = (point - origin).magnitude;
        return (int)(distance * distance / falloffScale);
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