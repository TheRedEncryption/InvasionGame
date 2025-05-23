using System.Collections.Generic;
using UnityEngine;

public class DemoPropSpammer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The possible shapes I can spawn.")]
    private GameObject[] _options;

    [SerializeField]
    [Tooltip("The min / max possible scale factor for the shape.")]
    private Vector2 _maxScale;

    [SerializeField]
    [Tooltip("The weight of the randomized scale selection (0-1)")]
    private AnimationCurve _scaleCurve;

    [SerializeField]
    [Tooltip("Defines the Top-Forward-Left and Bottom-Back-Right most part in a volume used to spawn shapes.")]
    private Vector3 _maxPositionSpawn;

    [SerializeField]
    [Tooltip("The magnitude of the velocity the shapes will spawn with")]
    private float _maxVelocity;

    [SerializeField]
    [Tooltip("The maximum amount of shapes I can spawn.")]
    private int _maxProps;

    [SerializeField]
    [Tooltip("The amount of shapes I spawn per frame.")]
    private int _spawnPerFram;

    [SerializeField]
    [Tooltip("The bounds before objects are deleted")]
    private Transform _basePlate;

    private List<GameObject> _children = new();
    private int _propCount;
    private int _index = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _spawnPerFram; i++)
        {
            SpawnObject();
            
        }
        RemoveBadChildren();
    }

    private void RemoveBadChildren()
    {
        for (int i = 0; i < _children.Count; i++)
        {
            Vector3 child = _children[i].transform.position;
            Vector3 basePos = _basePlate.position;

            if (child.y < basePos.y)
            {
                Destroy(_children[i]);
                _children.RemoveAt(i);
                i--;
                _propCount--;
            }
        }
    }

    private float Rand(float max) => Random.Range(-max, max);

    private void SpawnObject()
    {
        if (_maxProps <= _propCount) return;
        else _propCount++;

        if (_index == _options.Length) _index = 0;

        // Get the random position
        Vector3 propPos = new Vector3(Rand(_maxPositionSpawn.x), Rand(_maxPositionSpawn.y), Rand(_maxPositionSpawn.z)) + transform.position;

        // Get the random scale
        float newScale = Random.Range(0f, 1f);                             // Get random float between 0 & 1.
        newScale = Mathf.Clamp(_scaleCurve.Evaluate(newScale), 0, 1);      // Clamp and adjust according to user weight selection.
        newScale = newScale * (_maxScale.y - _maxScale.x) + _maxScale.x;   // Scale according to user inputs.
        

        Vector3 propScale = new(newScale, newScale, newScale);

        // Make the new prop
        GameObject newProp = Instantiate(_options[_index], propPos, Random.rotation, transform);
        newProp.transform.localScale = propScale;

        // Add a random velocity in there
        Vector3 propVelocity = new Vector3(Rand(1), Rand(1), Rand(1)).normalized * _maxVelocity;

        newProp.GetComponent<Rigidbody>().linearVelocity = propVelocity;

        _children.Add(newProp);

        _index++;
    }
}
