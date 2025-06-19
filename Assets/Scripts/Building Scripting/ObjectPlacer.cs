using System.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
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

    RaycastHit _hit;

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
        Vector3 screenPosition = Mouse.current.position.ReadValue();
        Ray placeDirection = Camera.main.ScreenPointToRay(screenPosition);

        Physics.Raycast(placeDirection, out _hit, 320f);

        _debugSphereInstance.transform.position = _hit.point;

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
}
