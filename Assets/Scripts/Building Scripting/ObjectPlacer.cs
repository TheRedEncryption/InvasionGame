using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    public PlacementGrid _grid = new(new Grid.Point(50,20,50), 1);

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
#if !UNITY_SERVER
        _debugSphereInstance = Instantiate(_debugSphere, Vector3.positiveInfinity, Quaternion.identity);
        DebugSphereScript deScript = _debugSphereInstance.GetComponent<DebugSphereScript>();
        deScript.Collided += OnCollide;
        deScript.DeCollided += OnDeCollide;
#else
        return;
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_SERVER
        // Shoot a raycast down
        Vector3 screenPosition = Mouse.current.position.ReadValue();
        Ray placeDirection = Camera.main.ScreenPointToRay(screenPosition);

        Physics.Raycast(placeDirection, out _hit, 320f);

        _debugSphereInstance.transform.position = CastRayIntoGrid(_hit.point);

        Evaluation = Evaluate();

        if (Evaluation && PlayerInputHandler.Instance.AttackTriggered && !EventSystem.current.IsPointerOverGameObject())
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                GameObject instance = Instantiate(_selectedGameObject, _debugEditiedRayPos, Quaternion.identity);
                _grid.SetPointState(_currIndex.X, _currIndex.Y, _currIndex.Z, PlacementGrid.GridVoxelState.occupied);
            }
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
#else
        return;
#endif
    }

    private bool Evaluate() =>
    !_collideTower && _grid.GetPointState(_currIndex) == PlacementGrid.GridVoxelState.unoccupied;

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
    private Grid.Point _currIndex = new();

    private Vector3 CastRayIntoGrid(Vector3 point)
    {
        // Method #1
        Vector3 realignmentOffset = new(_grid.Scale.x / 2, _grid.Scale.y / 2, _grid.Scale.z / 2);
        Vector3 hitPosition = _hit.point - realignmentOffset - _gridPos;
        Vector3 gridOffset = new(hitPosition.x % _grid.Scale.x, hitPosition.y % _grid.Scale.y, hitPosition.z % _grid.Scale.z);

        //Vector3 hitAdjusted = _hit.point - gridOffset;
        //_debugEditiedRayPos = hitAdjusted + realignmentOffset;

        Grid.Point gridIndex = (Grid.Point)(_hit.point - gridOffset + realignmentOffset);
        Grid.Point cappedPoint = Grid.Point.Clamp(gridIndex, (Grid.Point)_grid[0], (Grid.Point)_grid[_grid.NumPoints - 1]);

        // For DEMO purposes EXCLUSIVELY; USE SEPERATE
        while (_grid.GetPointState(cappedPoint) == PlacementGrid.GridVoxelState.occupied && cappedPoint.Y <= _grid.Dimensions.Y) cappedPoint.Y++;

        _debugEditiedRayPos = cappedPoint;
        _currIndex = cappedPoint;

        return _debugEditiedRayPos;
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

        for (int i = 0; i < _grid.NumPoints; i++)
        {
            if (_grid.GetPointState(i) == PlacementGrid.GridVoxelState.unoccupied)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_grid[i] + _gridPos, 0.1f);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(_grid[i] + _gridPos, 0.1f);
            }
        }
    }
}
