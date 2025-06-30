using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(StateMachine), typeof(Rigidbody), typeof(NetworkObject))]
public class Entity : NetworkBehaviour
{
    [SerializeField] private CappedFloat _health;
    public CappedFloat Health => _health;

    [HideInInspector] public StateMachine _stateMachine;
    [HideInInspector] public Rigidbody _rb;

    [HideInInspector] public class HealthEventArgs : EventArgs
    {
        public int healthArg { get; set; }
        public int maxHealthArg { get; set; }
    }

    [HideInInspector] public delegate void HealthChangedHandler(object source, HealthEventArgs e);
    [HideInInspector] public event HealthChangedHandler HealthChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _stateMachine = GetComponent<StateMachine>();
        _stateMachine["Host"] = this;

        _health.OnEmpty += OnDeath;
        _health.ChangeSuccessful += OnHealthChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void TakeDamage(float amount)
    {
        _health -= amount;
    }

    public virtual void Regenerate(float amount)
    {
        _health += amount;
    }

    public virtual void OnDeath()
    {
        //GetComponent<NetworkObject>().;
        gameObject.SetActive(false);
    }
    
    protected virtual void OnHealthChanged()
    {
        //Debug.Log("HEALTH CHANGED DIPSHIT: " + _health.CurrValue + "/" + _health.MaxValue);
        HealthChanged?.Invoke(this, new HealthEventArgs { healthArg = (int)_health, maxHealthArg = (int)_health.MaxValue });
    }
}
