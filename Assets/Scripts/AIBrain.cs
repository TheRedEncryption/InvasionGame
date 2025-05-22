using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AIBrain : MonoBehaviour
{
    // *******************
    // Unity Inspector elements
    // *******************
    // Behavior management
    [Header("Important Behaviors")]
    public Behavior Inital;
    public Behavior Death;


    [HideInInspector] [SerializeField] private Dictionary<string, Behavior> _allBehaviors = new();
    [HideInInspector] public Transform _startingPosition;

    // *******************
    // Descriptor Properties
    // *******************
    public bool IsAttacking { get; set; }


    // Data about currentState
    private Behavior _state;

    // Variables storage
    private readonly Dictionary<string, object> _brainVarRefs = new();

    public object this[string key]
    {
        get 
        {
            if (_brainVarRefs.ContainsKey(key))
            {
                return _brainVarRefs[key];
            }
            else
            {
                _brainVarRefs[key] = null;
                return null;
            }
        }

        set
        {
            _brainVarRefs[key] = value;
        }
    }
    


    // *******************
    // Setup methods
    // *******************

    /// <summary>
    /// Save each component into the interal list.
    /// </summary>
    private void CacheBehaviors()
    {
        Behavior[] _myBehaviors = GetComponents<Behavior>();

        foreach (Behavior behavior in _myBehaviors)
        {
            _allBehaviors[behavior.GetName] = behavior;
        }
    }

    void Start()
    {
        IsAttacking = false;

        CacheBehaviors();
    }


    void OnEnable()
    {
        transform.SetPositionAndRotation(_startingPosition.position, _startingPosition.rotation);

        _state = Inital;
        IsAttacking = false;

        _state.EnterState(this);
    }

    void Update()
    {
        _state?.FlexMuscle(this);
    }

    void FixedUpdate()
    {
        _state?.FlexFixed(this);
    }

    #region State Switching and Helpers

    /// <summary>
    /// Switches to a new state based on input name and type
    /// </summary>
    /// <param name="newState">Name of trait to search for</param>
    public void SwitchState(string newState)
    {
        newState = newState.ToUpper().Trim();
        Behavior replaceState;

        try
        {
            replaceState = _allBehaviors[newState];
            SwitchState(replaceState);
        }
        catch (Exception)
        {
            Debug.LogError($"No such behavior  in {gameObject.name} called {newState}");
            return;
        }
    }

    /// <summary>
    /// Switches to a new state based on input name and type
    /// </summary>
    /// <param name="behavior">The new behavior to use</param>
    public void SwitchState(Behavior behavior)
    {
        behavior.EnterState(this);
        _state = behavior;
    }

    #endregion
}