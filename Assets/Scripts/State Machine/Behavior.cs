using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Xml.Serialization;
using UnityEngine;

public abstract class Behavior : MonoBehaviour
{
    public static Behavior EmptyBehavior;

    /// <summary>
    /// The name by which this behavior can be called.
    /// </summary>
    [SerializeField] protected string _name;

    /// <summary>
    /// The capitalized and trimmed version of the name, 
    /// best used for 
    /// </summary>
    public string Name { get => _name.ToUpper().Trim(); }

    public virtual void Enter(StateMachine brain) { }
    public virtual void Flex(StateMachine brain) { }
    public virtual void FixedFlex(StateMachine brain) { }
    public virtual void LateFlex(StateMachine brain) { }
    public virtual void Leave(StateMachine brain) { }

    public virtual void OnCollision(StateMachine brain, Collision collision) { }
    public virtual void OffCollision(StateMachine brain, Collision collision) { }
    public virtual void OnTrigger(StateMachine brain, Collider collision) { }
    public virtual void OffTrigger(StateMachine brain, Collider collision) { }

    public virtual void DrawThing(StateMachine brain) { }
}
