using UnityEngine;

[System.Serializable]
public struct Bounds2DInt
{
    public int minX;
    public int minZ;
    public int maxX;
    public int maxZ;

    public Vector2Int Min => new Vector2Int(minX, minZ);
    public Vector2Int Max => new Vector2Int(maxX, maxZ);
    public Vector2Int Size => Max - Min;
}
