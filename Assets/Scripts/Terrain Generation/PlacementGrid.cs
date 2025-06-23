using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class PlacementGrid : Grid
{
    /// <summary>
    /// The state of a voxel in the placement grid.
    /// </summary>
    [Serializable]
    public enum GridVoxelState
    {
        // States
        none = 0b_000,
        occupied = 0b_001,
        unoccupied = 0b_010,
        air = 0b_100,
        // Masks
        free = 0b_110
    }

    /// <summary>
    /// Holder value for inital deciding value used in deciding a points state when it's assigned.
    /// </summary>
    private const int GROUND_HEIGHT = 2;

    /// <summary>
    /// A map of each point's state in the grid.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private Dictionary<Point, GridVoxelState> _pointStates;

    public PlacementGrid(Point dimensions, float scale)
    : base(dimensions, new Vector3(scale, scale, scale))
    {
        _dimensions = dimensions;
        Scale = new Vector3(scale, scale, scale);
        _points = new Point[dimensions.X * dimensions.Y * dimensions.Z];
    }

    /// <summary>
    /// Gets the state of a point in the grid.
    /// </summary>
    /// <param name="index">The value indexed into the internal state map</param>
    /// <returns>The state of a given point</returns>
    public GridVoxelState GetPointState(Point index) => _pointStates[index];

    public GridVoxelState GetPointState(int index)
    {
        GridVoxelState state;

        try
        {
            state = _pointStates[(Point)this[index] / (int)Scale.x];
        }
        catch (KeyNotFoundException)
        {
            state = GridVoxelState.none;
            Debug.LogError("Invalid Index; no state provided when asked: " + index);
        }
        catch (Exception e)
        {
            state = GridVoxelState.none;
            Debug.LogError(e);
        }

        return state;
    }
    public GridVoxelState GetPointState(int x, int y, int z) => GetPointState(LinearIndexMapping(x, y, z));

    /// <summary>
    /// Rebuilds the internal grid and point mappings. Avoid calling this more than once.
    /// </summary>
    public override void MakeGrid()
    {
        _pointStates = new(Dimensions.X * Dimensions.Y * Dimensions.Z);

        base.MakeGrid();
    }

    /// <summary>
    /// Assign a point to a place in the grid
    /// </summary>
    /// <param name="x">X cord</param>
    /// <param name="y">Y cord</param>
    /// <param name="z">Z cord</param>
    /// <param name="newPoint">The point in question</param>
    /// <returns>The assigned point</returns>
    protected override Point SetPoint(int x, int y, int z, Point newPoint)
    {
        _points[x + _dimensions.X * (y + z * _dimensions.Y)] = newPoint;

        // Initial height map for basic calculations and the like
        if (y < GROUND_HEIGHT)
            _pointStates[newPoint] = GridVoxelState.occupied;
        else if (y == GROUND_HEIGHT)
            _pointStates[newPoint] = GridVoxelState.unoccupied;
        else
            _pointStates[newPoint] = GridVoxelState.air;

        return newPoint;
    }

    /// <summary>
    /// Set the state of a point in the grid
    /// </summary>
    /// <param name="x">X cord</param>
    /// <param name="y">Y cord</param>
    /// <param name="z">Z cord</param>
    /// <param name="newState"></param>
    public void SetPointState(int x, int y, int z, GridVoxelState newState) => _pointStates[(Point)this[x, y, z]] = newState;

    public int LinearIndexMapping(int x, int y, int z) => x + _dimensions.X * (y + z * _dimensions.Y);
    
    public Point NonLinearIndexMapping(int index)
    {
        int z = index / (_dimensions.X * _dimensions.Y);
        index -= z * _dimensions.X * _dimensions.Y;
        int y = index / _dimensions.X;
        int x = index % _dimensions.X;
        return new Point(x, y, z);
    }
}
