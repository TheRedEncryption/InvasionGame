using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainSeed : MonoBehaviour
{
    // Mesh variables
    private MeshFilter meshFilter;
    private Mesh mesh;

    // Lists for the vertices and triangles that will then be fed into the array
    private List<Vector3> vertexGrid = new List<Vector3>();
    private List<int> triangleList = new List<int>();

    // width and height of the plane to generate over
    private int width;
    private int height;

    // To control the terrain
    [Header("Terrain Controls")]
    [SerializeField] private int seed;
    [SerializeField] private Bounds2DInt terrainBoundary;
    [SerializeField] private float scale = 1;

    // To visualize the mesh values
    [Header("Mesh Value Visualization")]
    [SerializeField] private Vector3[] vertexArray;
    [SerializeField] private int[] triangleArray;

    void Start()
    {
        UnityEngine.Random.InitState(seed); // allows for reproducibility

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        width = terrainBoundary.Size.x + 1;
        height = terrainBoundary.Size.y + 1; // we are using the Y channel for Z values

        GenerateTerrainMesh();

        meshFilter.mesh = mesh;

        // TODO: set bounds and center of mesh using code
    }

    #region GENERATION

    public void GenerateTerrainMesh()
    {
        InitializeVertexGrid();

        InitializeTriangleArray();

        vertexArray = new Vector3[vertexGrid.Count];
        mesh.vertices = new Vector3[vertexGrid.Count];
        vertexGrid.CopyTo(vertexArray);

        triangleArray = new int[triangleList.Count];
        mesh.triangles = new int[triangleList.Count];
        triangleList.CopyTo(triangleArray);

        mesh.vertices = vertexArray;
        mesh.triangles = triangleArray;
    }

    private void InitializeVertexGrid()
    {
        for (int zRow = 0; zRow <= height - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                vertexGrid.Add(new Vector3Int(xCol, (int)UnityEngine.Random.Range(0, scale + 1), zRow));
            }
        }
    }

    private void InitializeTriangleArray()
    {
        for (int zRow = 0; zRow < height - 1; zRow++)
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

    private Vector3Int GetVertexFromXZ(int x, int z)
    {
        if ((z * width + x) <= vertexGrid.Count)
        {
            Vector3 element = vertexGrid[z * width + x];
            return new Vector3Int((int)element.x, (int)element.y, (int)element.z);
        }
        return new Vector3Int(0, 0, 0); // TODO: do something about this later 
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