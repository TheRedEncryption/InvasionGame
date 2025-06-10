using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ReticleLineArray : ReticlePart
{
    [Min(1)]
    public int lineCount = 8; // Number of lines
    public int lineLength = 1;
    public int lineThickness = 1;
    public int distanceFromCenter = 1;
    public Color lineColor = Color.black;
    public float startAngle = 0f; // Angle to rotate the whole array
    public float lineAngle = 0f;  // Extra rotation for each line about its midpoint

    // Internal list of lines (not serialized)
    [NonSerialized]
    private List<ReticleLine> generatedLines = new List<ReticleLine>();

    // Generate the array of lines
    public void GenerateLines()
    {
        generatedLines.Clear();
        float angleStep = 360f / lineCount;

        for (int i = 0; i < lineCount; i++)
        {
            float angle = startAngle + i * angleStep;

            ReticleLine line = new ReticleLine
            {
                lineLength = lineLength,
                lineThickness = lineThickness,
                distanceFromCenter = distanceFromCenter,
                lineColor = lineColor,
                lineAngleFromCenter = angle,
                lineAngle = lineAngle
            };
            generatedLines.Add(line);
        }
    }

    public override void Draw(VertexHelper vh, int segments = DefaultSegments)
    {
        // Always regenerate in case user changes settings
        GenerateLines();

        foreach (var line in generatedLines)
        {
            line.Draw(vh, segments);
        }
    }

    public override string ToString()
    {
        return $"LineArray: {lineCount} lines, len={lineLength}, thick={lineThickness}, dist={distanceFromCenter}";
    }
}
