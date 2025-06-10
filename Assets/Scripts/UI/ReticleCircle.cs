using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ReticleCircle : ReticlePart
{
    [Min(0)] public float radius = 50f;
    [Min(0)] public float innerRadius = 40f;
    public bool fill = true;
    [Range(0f, 360f)] public float sweepAngle = 360f;
    [Range(0f, 360f)] float startDeg = 0f;

    [Header("Appearance")]
    public Color circleColor = Color.black;

    // Default constructor to set any runtime defaults
    public ReticleCircle()
    {
        radius = 3f;
        innerRadius = 1f;
        fill = true;
        sweepAngle = 360f;
        startDeg = 0f;
        circleColor = Color.black;
    }

    public override void Draw(VertexHelper vh, int segments = DefaultSegments)
    {
        float inner = this.fill ? 0f : Mathf.Max(0f, this.innerRadius);
        // remember where our verts begin
        int startIndex = vh.currentVertCount;

        float startRad = Mathf.Clamp(this.startDeg, 0f, 360f) / 360f * Mathf.PI * 2f;
        float sweepRad = Mathf.Clamp01(this.sweepAngle/ 360f) * Mathf.PI * 2f;
        float deltaTheta = sweepRad / segments;

        if (inner <= 0f)
        {
            // filled fan
            vh.AddVert(Vector2.zero, circleColor, Vector2.zero);
            for (int i = 0; i <= segments; i++)
            {
                float theta = startRad + i * deltaTheta;
                Vector2 pos = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * this.radius;
                vh.AddVert(pos, circleColor, Vector2.zero);

                if (i > 0)
                {
                    // connect center (startIndex) to the two outer verts
                    vh.AddTriangle(
                        startIndex,
                        startIndex + i,
                        startIndex + i + 1
                    );
                }
            }
        }
        else
        {
            // hollow ring
            for (int i = 0; i < segments; i++)
            {
                float thetaA = startRad + i * deltaTheta;
                float thetaB = startRad + (i + 1) * deltaTheta;

                Vector2 oA = new Vector2(Mathf.Cos(thetaA), Mathf.Sin(thetaA)) * this.radius;
                Vector2 oB = new Vector2(Mathf.Cos(thetaB), Mathf.Sin(thetaB)) * this.radius;
                Vector2 iA = new Vector2(Mathf.Cos(thetaA), Mathf.Sin(thetaA)) * inner;
                Vector2 iB = new Vector2(Mathf.Cos(thetaB), Mathf.Sin(thetaB)) * inner;

                int idx = vh.currentVertCount;
                vh.AddVert(oA, circleColor, Vector2.zero);
                vh.AddVert(oB, circleColor, Vector2.zero);
                vh.AddVert(iB, circleColor, Vector2.zero);
                vh.AddVert(iA, circleColor, Vector2.zero);

                vh.AddTriangle(idx + 0, idx + 1, idx + 2);
                vh.AddTriangle(idx + 2, idx + 3, idx + 0);
            }
        }
    }
    public override string ToString()
    {
        return $"Circle: R={radius}, innerR={innerRadius}, fill={fill}, sweep={sweepAngle}, start={startDeg}";
    }
}
