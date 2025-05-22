using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public abstract class Behavior : MonoBehaviour
{
    [SerializeField] protected string _name;
    public string GetName { get{ return _name.ToUpper().Trim(); } }

    public virtual void EnterState(AIBrain brain) {}

    public abstract void FlexMuscle(AIBrain brain);
    public virtual void FlexFixed(AIBrain brain) {}
    public virtual void LeaveState(AIBrain brain) {}
    
}
