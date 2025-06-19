using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ReticleLine : ReticlePart
{
    [Min(0)] public int lineLength = 50; // Length of the line
    public int distanceFromCenter = 1; // Distance from the center
    [Range(0f, 360f)] public float lineAngleFromCenter = 0f; // Angle of the line in degrees
    [Range(0f, 360f)] public float lineAngle = 0f; // Angle of the line in degrees
    [Min(0)] public int lineThickness = 2; // Thickness of the line
    public Color lineColor = Color.black; // Color of the line
    public bool fill = true; // Whether to fill the line or not

    // Default constructor to set any runtime defaults
    public ReticleLine()
    {
        
    }

    public override void Draw(VertexHelper vh, int segments = DefaultSegments)
    {
        // 1. Base direction of the line (from center outward)
        float baseRad = lineAngleFromCenter * Mathf.Deg2Rad;
        Vector2 baseDir = new Vector2(Mathf.Cos(baseRad), Mathf.Sin(baseRad));

        // 2. Near and far endpoints
        Vector2 near = baseDir * distanceFromCenter;
        Vector2 far = near + baseDir * lineLength;

        // 3. Midpoint
        Vector2 mid = (near + far) * 0.5f;

        // 4. Perpendicular for thickness
        Vector2 perp = new Vector2(-baseDir.y, baseDir.x);
        float halfThickness = lineThickness / 2f;

        // 5. Quad corners before rotation (aligned with baseDir)
        Vector2 p1 = near + perp * halfThickness;
        Vector2 p2 = near - perp * halfThickness;
        Vector2 p3 = far - perp * halfThickness;
        Vector2 p4 = far + perp * halfThickness;

        // 6. Rotate corners about midpoint by lineAngle
        float rotRad = lineAngle * Mathf.Deg2Rad;

        p1 = RotateAround(p1, mid, rotRad);
        p2 = RotateAround(p2, mid, rotRad);
        p3 = RotateAround(p3, mid, rotRad);
        p4 = RotateAround(p4, mid, rotRad);

        int vertIndex = vh.currentVertCount;

        vh.AddVert(p1, lineColor, Vector2.zero);
        vh.AddVert(p2, lineColor, Vector2.zero);
        vh.AddVert(p3, lineColor, Vector2.zero);
        vh.AddVert(p4, lineColor, Vector2.zero);

        vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
        vh.AddTriangle(vertIndex, vertIndex + 2, vertIndex + 3);
    }

    // Helper for rotating a point about a pivot
    private static Vector2 RotateAround(Vector2 point, Vector2 pivot, float radians)
    {
        float s = Mathf.Sin(radians);
        float c = Mathf.Cos(radians);
        Vector2 pt = point - pivot;
        float xnew = pt.x * c - pt.y * s;
        float ynew = pt.x * s + pt.y * c;
        return new Vector2(xnew, ynew) + pivot;
    }


    public override string ToString()
    {
        return "your mother";
        //return $"Circle: R={radius}, innerR={innerRadius}, fill={fill}, sweep={sweepAngle}, start={startDeg}";
    }
}
