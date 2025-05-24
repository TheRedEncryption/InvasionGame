using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StateMachine : MonoBehaviour
{
    /// <summary>
    /// The behvariors this brain can access.
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private Dictionary<string, Behavior> _behaviors = new();

    // --- Current State ---
    private Behavior _currState;

    private Behavior CurrState
    {
        get
        {
            if (_currState != null)
                return _currState;
            else
                return Behavior.EmptyBehavior;
        }
    }

    // --- Variables storage ---
    private readonly Dictionary<string, object> _brainVarRefs = new();

    /// <summary>
    /// Indexes to the brain to "remember" objects between states
    /// </summary>
    /// <remarks>
    /// This should only be used under very strict circumstances.
    /// </remarks>
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

    /// <summary>
    /// Save each component into the interal list.
    /// </summary>
    private Behavior CacheBehaviors()
    {
        Behavior[] _myBehaviors = GetComponents<Behavior>();

        foreach (Behavior behavior in _myBehaviors)
            _behaviors[behavior.Name] = behavior;

        // Return the first behavior
        return _myBehaviors[0];
    }

    #region Unity MonoBehavior methods

    void Start() => _currState = CacheBehaviors();

    void OnEnable() => CurrState.Enter(this);

    void Update() => CurrState.Flex(this);

    void FixedUpdate() => CurrState.FixedFlex(this);

    void LateUpdate() => CurrState.LateFlex(this);

    void OnCollisionEnter(Collision collision) => CurrState.OnCollision(this, collision);

    void OnCollisionExit(Collision collision) => CurrState.OffCollision(this, collision);

    void OnTriggerEnter(Collider other) => CurrState.OnTrigger(this, other);

    void OnTriggerExit(Collider other) => CurrState.OffTrigger(this, other);

    void OnDrawGizmos() => CurrState.DrawThing(this);

    #endregion

    #region Switch State

    /// <summary>
    /// Switches to a new state based on input name and type
    /// </summary>
    /// <param name="newState">Name of behavior to search for</param>
    public void SwitchState(string newState)
    {
        newState = newState.ToUpper().Trim();
        Behavior replaceState;

        try
        {
            replaceState = _behaviors[newState];
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
        behavior.Enter(this);
        _currState = behavior;
    }

    #endregion
}