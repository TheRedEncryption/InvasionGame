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
    private List<Vector3> vertexList = new List<Vector3>();
    private List<int> triangleList = new List<int>();
    private List<Vector3> normalList = new List<Vector3>();

    // width and height of the plane to generate over
    private int width;
    private int height;

    // To control the terrain
    [Header("Terrain Controls")]
    [SerializeField] private int seed;
    [SerializeField] private Bounds2DInt terrainBoundary;
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private int heightSteps = 10;
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
        height = terrainBoundary.Size.y + 1; // we are using the Y channel for Z values

        GenerateTerrainMesh();

        meshFilter.mesh = mesh;

        // TODO: set bounds and center of mesh using code (IMPORTANT OR ELSE MESH DISAPPEARS FROM VIEW)
    }

    #region GENERATION

    public void GenerateTerrainMesh()
    {
        InitializeVertexGrid();

        InitializeTriangleArray();

        // CalculateNormals();

        vertexArray = new Vector3[vertexList.Count];
        mesh.vertices = new Vector3[vertexList.Count];
        vertexList.CopyTo(vertexArray);

        triangleArray = new int[triangleList.Count];
        mesh.triangles = new int[triangleList.Count];
        triangleList.CopyTo(triangleArray);

        // normalArray = new Vector3[normalList.Count];
        // mesh.normals = new Vector3[normalList.Count];
        // normalList.CopyTo(normalArray);

        mesh.vertices = vertexArray;
        mesh.triangles = triangleArray;
        // mesh.normals = normalArray;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void InitializeVertexGrid()
    {
        vertexList.Clear(); // just to be safe

        for (int zRow = 0; zRow <= height - 1; zRow++)
        {
            for (int xCol = 0; xCol <= width - 1; xCol++)
            {
                float xCoord = xCol * noiseScale + offset;
                float zCoord = zRow * noiseScale + offset;

                float perlinValue = Mathf.PerlinNoise(xCoord, zCoord);
                int newY = Mathf.FloorToInt(perlinValue * heightSteps);

                Vector3Int voxelVertex = new Vector3Int(xCol + terrainBoundary.minX, newY, zRow + terrainBoundary.minZ);

                vertexList.Add(voxelVertex);
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

    private void CalculateNormals()
    {
        for (int i = 0; i < triangleList.Count; i += 3)
        {
            int pIndex = triangleList[i];
            int qIndex = triangleList[i + 1];
            int rIndex = triangleList[i + 2];

            Vector3 p = vertexList[pIndex];
            Vector3 q = vertexList[qIndex];
            Vector3 r = vertexList[rIndex];

            Vector3 normal = CalculateClockwiseNormal(p, q, r);

            normalList.Add(normal);
            Debug.DrawRay((p + q + r) / 3f, normal, Color.green, 1f);
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

    public static Vector3 CalculateClockwiseNormal(Vector3 p, Vector3 q, Vector3 r)
    {
        // get edge vectors
        Vector3 pq = q - p;
        Vector3 pr = r - p;

        // flip the cross product because clockwise winding
        return Vector3.Cross(pr, pq).normalized;
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