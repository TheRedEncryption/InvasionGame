using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public class GridPoint
    {
        private List<GridPoint> _borderingPoints = new(4);
        public GridPoint[] BorderingPoints => _borderingPoints.ToArray();

        private Vector3 _position;
        public Vector3 Position => _position;

        public GridPoint(Vector3 position)
        {
            _position = position;
        }

        public void AddNeighbor(GridPoint p)
        {
            _borderingPoints.Add(p);
        }
    }

    public class GridTile
    {
        private bool _available;
        private float _sideLength;
        private int[] _relativePos = new int[3];
        private GridPoint[] _quadPoints = new GridPoint[4];

        public bool Available => _available;
        public float SideLength => _sideLength;

        /// <summary>
        /// The X position in the grid
        /// </summary>
        public int RelX => _relativePos[0];

        /// <summary>
        /// The Y position in the grid
        /// </summary>
        public int RelY => _relativePos[1];

        /// <summary>
        /// The Z position in the grid
        /// </summary>
        public int RelZ => _relativePos[2];

        /// <summary>
        /// The position of the GridTile in the Grids array
        /// </summary>
        public Vector3 RelativePosition => new Vector3(RelX, RelY, RelZ);

        /// <summary>
        /// The position of this tile relative to the grid's (0,0,0)
        /// </summary>
        public Vector3 LocalWorldPosition => RelativePosition * SideLength;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
