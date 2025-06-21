using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        public static Point Zero = new Point(0);
        public static Point One = new Point(1);

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
        public readonly int[] Offset => new int[] { X, Y, Z };

        /// <summary>
        /// The offset of this point as a Vector3.
        /// </summary>
        public readonly Vector3 OffsetVector => new(X, Y, Z);

        public Point(int num)
        {
            X = num;
            Y = num;
            Z = num;
        }

        public Point(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Limits each axes of the point "p" to be between the values set in "min" and "max"
        /// </summary>
        /// <param name="p"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Point Clamp(Point p, Point min, Point max)
        {
            // Check for X irregularities
            if (p.X < min.X)
            {
                p.X = min.X;
            }
            else if (p.X > max.X)
            {
                p.X = max.X;
            }

            // Check for Y irregularities
            if (p.Y < min.Y)
            {
                p.Y = min.Y;
            }
            else if (p.Y > max.Y)
            {
                p.Y = max.Y;
            }

            // Check for Z irregularities
            if (p.Z < min.Z)
            {
                p.Z = min.Z;
            }
            else if (p.Z > max.Z)
            {
                p.Z = max.Z;
            }

            return p;
        }

        public override readonly string ToString() => $"({X}, {Y}, {Z})";

        public override readonly bool Equals(object obj) => (obj is Point point) && point == this;

        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

        public static implicit operator Vector3(Point p) => new(p.X, p.Y, p.Z);
        public static explicit operator Point(Vector3 v) => new((int)v.x, (int)v.y, (int)v.z);

        public static bool operator ==(Point left, Point right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        public static bool operator !=(Point left, Point right) => !(left.X == right.X && left.Y == right.Y && left.Z == right.Z);

        public static Point operator *(Point left, int right) => new Point(left.X * right, left.Y * right, left.Z * right);

        /// <summary>
        /// Divides each value in a point by a given integer
        /// </summary>
        /// <param name="left">The point</param>
        /// <param name="right">The integer</param>
        /// <returns>A point that represents the integer division of all three axes of the point</returns>
        public static Point operator /(Point left, int right)
        {
            Point p;

            try
            {
                p = new Point(left.X / right, left.Y / right, left.Z / right);
            }
            catch (DivideByZeroException)
            {
                // Set p to Zero since current most common usage for division is for indexing purposes; removes future potential errors
                p = Zero;
                Debug.LogWarning("Cannot divide a point by zero; defaulting to 0");
            }
            catch (Exception e)
            {
                p = Zero;
                Debug.LogError("Unchecked error; defaulting to zero: " + e.Message);
            }

            return p;
        } 
        
    }
    
    /// <summary>
    /// The dimensions of one "cube" in the grid
    /// </summary>
    [SerializeField] public Vector3 Scale = new (1,1,1);

    /// <summary>
    /// Dimensions of the overall grid.
    /// </summary>
    [SerializeField] protected Point _dimensions = new (1,1,1);

    /// <summary>
    /// The number of points along each axis.
    /// </summary>
    public Point Dimensions
    {
        get => _dimensions;
        set
        {
            if (_dimensions != value)
            {
                _dimensions = value;
                _points = new Point[_dimensions.X * _dimensions.Y * _dimensions.Z];
                MakeGrid();
            }
        }
    }

    /// <summary>
    /// An array of the points in the grid.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    protected Point[] _points;

    /// <summary>
    /// Get a vector from the origin to a point in the grid VIA direct index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 this[int index]
    {
        get
        {
            Point p = _points[index];
            return new Vector3(p.X * Scale.x, p.Y * Scale.y, p.Z * Scale.z);
        }
    }

    /// <summary>
    /// Get a vector from the origin to a point in the grid.
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

    public virtual void MakeGrid()
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

    protected virtual Point SetPoint(int x, int y, int z, Point newPoint) => _points[x + _dimensions.X * (y + z * _dimensions.Y)] = newPoint;
    public void SetScale(Vector3 scale) => Scale = scale;
}
