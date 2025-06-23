using System;
using UnityEngine;

[Serializable]
public class Grid
{
    /// <summary>
    /// A vector limited to only represent integers
    /// </summary>
    [Serializable]
    public struct Point
    {
        /// <summary>
        /// The X component of this points offset
        /// </summary>
        public int X;

        /// <summary>
        /// The Y component of this points offset
        /// </summary>
        public int Y;

        /// <summary>
        /// The Z component of this points offset
        /// </summary>
        public int Z;

        /// <summary>
        /// The offset of this point as an array of ints.
        /// </summary>
        public int[] Offset => new int[] { X, Y, Z };

        /// <summary>
        /// The offset of this point as a Vector3.
        /// </summary>
        public Vector3 OffsetVector => new(X, Y, Z);

        public Point(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"({X},{Y},{Z})";
        }

        public static implicit operator Vector3(Point p) => new(p.X, p.Y, p.Z);
        public static explicit operator Point(Vector3 v) => new((int)v.x, (int)v.y, (int)v.z);
    }
    
    /// <summary>
    /// The dimensions of one "cube" in the grid
    /// </summary>
    [SerializeField] public Vector3 Scale = new (1,1,1);

    /// <summary>
    /// Dimensions of the overall grid.
    /// </summary>
    [SerializeField] private Point _dimensions = new (1,1,1);

    public Point Dimensions
    {
        get => _dimensions;
        set
        {
            _dimensions = value;
            _points = new Point[_dimensions.X * _dimensions.Y * _dimensions.Z];
            MakeGrid();

            Debug.Log(NumPoints);
        }
    }

    [HideInInspector] [SerializeField] private Point[] _points;


    public Vector3 this[int index]
    {
        get
        {
            Point p = _points[index];
            return new Vector3(p.X * Scale.x, p.Y * Scale.y, p.Z * Scale.z);
        }
    }

    /// <summary>
    /// Get a vector in the world based on a point in the grid.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>A Vector from </returns>
    public Vector3 this[int x, int y, int z]
    {
        get
        {
            Point p = _points[x + _dimensions.X * (y + z * _dimensions.Y)];
            return new Vector3(p.X * Scale.x, p.Y * Scale.y, p.Z * Scale.z);
        }
    }

    public int NumPoints => _points.Length;

    public Grid(Point dimensions, Vector3 scale)
    {
        _dimensions = dimensions;
        Scale = scale;

        _points = new Point[dimensions.X * dimensions.Y * dimensions.Z];

        MakeGrid();
    }

    public void MakeGrid()
    {
        for (int i = 0; i < _dimensions.X; i++)
        {
            for (int j = 0; j < _dimensions.Y; j++)
            {
                for (int k = 0; k < _dimensions.Z; k++)
                {
                    Point newPoint = new(i, j, k);

                    SetPoint(i, j, k, newPoint);
                }
            }
        }
    }

    private Point SetPoint(int x, int y, int z, Point newPoint) => _points[x + _dimensions.X * (y + z * _dimensions.Y)] = newPoint;
    public void SetScale(Vector3 scale) => Scale = scale;
}
