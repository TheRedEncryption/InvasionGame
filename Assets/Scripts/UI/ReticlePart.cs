using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public abstract class ReticlePart
{
    public const int DefaultSegments = 128;

    /// <summary>
    /// Draw yourself into the vertex helper.
    /// </summary>
    /// <param name="vh"> MaskableGraphic’s mesh builder </param>
    /// <param name="segments"> Level of circle‐resolution for round parts </param>
    public abstract void Draw(VertexHelper vh, int segments = DefaultSegments);
}