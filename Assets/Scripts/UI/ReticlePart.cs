using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ReticlePart
{
    public static readonly int DefaultSegments = 128;
    [Min(0)] public float radius = 3;
    [Min(0)] public float innerRadius = 3;
    public bool fill = true;
    [Range(0f, 360f)] public float sweepAngle = 360f;
    public float startDeg = 0;

    [Header("Appearance")]
    [SerializeField] private Color circleColor = Color.black;
    public Color CircleColor
    {
        get => circleColor;
        set
        {
            circleColor = value;
        }
    }

    // constructors
    public ReticlePart()
    {
        this.radius = 50f;
        this.innerRadius = 40f;
        this.fill = true;
        this.sweepAngle = 360f;
        this.startDeg = 0f;
        this.CircleColor = Color.black;
    }

    override public string ToString()
    {
        return $"ReticlePart: innerRadius: {innerRadius}, fill: {fill}, sweepAngle: {sweepAngle}, startDeg: {startDeg}, color: {CircleColor}";
    }

}
