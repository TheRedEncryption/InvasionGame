using System.Data;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    public PlacementGrid _grid = new(new Grid.Point(20,20,20), 1);

    public Vector3 _gridPos;

    public GameObject _selectedGameObject;
    public GameObject _debugSphere;

    public Color badColor;
    public Color goodColor;
    private GameObject _debugSphereInstance;

    private bool Evaluation
    {
        get
        {
            return _evaulation;
        }
        set
        {
            _evaulationOld = _evaulation;
            _evaulation = value;
        }
    }

    private bool _evaulation = false;
    private bool _evaulationOld = false;


    private Vector3 _objectSpawnPosition;

    RaycastHit _hit = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _debugSphereInstance = Instantiate(_debugSphere, Vector3.positiveInfinity, Quaternion.identity);
        DebugSphereScript deScript = _debugSphereInstance.GetComponent<DebugSphereScript>();
        deScript.Collided += OnCollide;
        deScript.DeCollided += OnDeCollide;
    }

    // Update is called once per frame
    void Update()
    {
        // Shoot a raycast down
        Vector3 screenPosition = Mouse.current.position.ReadValue();
        Ray placeDirection = Camera.main.ScreenPointToRay(screenPosition);

        Physics.Raycast(placeDirection, out _hit, 320f);

        _debugSphereInstance.transform.position = CastRayIntoGrid(_hit.point);

        Evaluation = Evaluate();

        if (Evaluation && PlayerInputHandler.Instance.AttackTriggered && !EventSystem.current.IsPointerOverGameObject())
        {
            _ = Instantiate(_selectedGameObject, _hit.point, Quaternion.identity);
        }

        // Change the color of the debug sphere to match the results of the eval
        if (Evaluation != _evaulationOld)
        {
            if (!Evaluation)
            {
                _debugSphereInstance.GetComponent<Renderer>().material.SetColor("_EmissionColor", badColor);
            }
            else
            {
                _debugSphereInstance.GetComponent<Renderer>().material.SetColor("_EmissionColor", goodColor);
            }
        }
    }

    private bool Evaluate() =>
    !(_collideTower);

    void OnDisable()
    {
        if (_debugSphereInstance != null)
            // Destroy(_debugSphereInstance);
            _debugSphereInstance.SetActive(false);
    }

    void OnEnable()
    {
        if (_debugSphereInstance != null)
            _debugSphereInstance.SetActive(true);
    }

    #region --- Handle collision bool ---

    bool _collideTower => _hit.collider.gameObject.layer == 9;
    int _numTowerTouches = 0;

    private void OnCollide()
    {
        _numTowerTouches++;
    }

    private void OnDeCollide()
    {
        _numTowerTouches--;
    }

    #endregion

    private Vector3 _debugEditiedRayPos = new();

    private Vector3 CastRayIntoGrid(Vector3 point)
    {
        // Method #1
        Vector3 realignmentOffset = new Vector3(_grid.Scale.x / 2, _grid.Scale.y / 2, _grid.Scale.z / 2) + _gridPos;
        Vector3 hitPosition = _hit.point - realignmentOffset;
        Vector3 gridOffset = new Vector3(
            hitPosition.x % _grid.Scale.x,
            hitPosition.y % _grid.Scale.y,
            hitPosition.z % _grid.Scale.z
        );

        Vector3 gridIndex = _hit.point - gridOffset;

        _debugEditiedRayPos = gridIndex + realignmentOffset;
        //Grid.Point.Clamp((Grid.Point)_debugEditiedRayPos, (Grid.Point)_grid[0], (Grid.Point)_grid[_grid.NumPoints - 1]);

        // Method #2 - Unfinished raycasting style
        /*
        // Set up detection planes
        Vector3 direction = placeDirection.direction;

        Vector3[] normals = ComputePossibleNormals(direction);

        if (normals.Length == 0)
        {
            return Vector3.zero;
        }
        */

        return _hit.point;
    }

    /// <summary>
    /// Compute which sides of an (X,Y,Z) normal cube a vector with a direction could intersect
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector3[] ComputePossibleNormals(Vector3 direction)
    {
        List<Vector3> normals = new(3);
        if (direction.x != 0)
        {
            if (direction.x > 0)
                normals.Add(Vector3.right);
            else
                normals.Add(Vector3.left);
        }
        if (direction.y != 0)
        {
            if (direction.y > 0)
                normals.Add(Vector3.up);
            else
                normals.Add(Vector3.down);
        }
        if (direction.z != 0)
        {
            if (direction.z > 0)
                normals.Add(Vector3.forward); 
            else
                normals.Add(Vector3.back);
        }

        return normals.ToArray();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_debugEditiedRayPos, 0.3f);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < _grid.NumPoints; i++)
        {
            if (_grid.GetPointState(i) == PlacementGrid.GridVoxelState.unoccupied)
                Gizmos.DrawSphere(_grid[i] + _gridPos, 0.1f);
        }
    }
}
