using System;
using UnityEngine;


[RequireComponent(typeof(StateMachine), typeof(Rigidbody))]
public class Entity : MonoBehaviour
{
    [SerializeField] private CappedFloat _health;
    public CappedFloat Health
    {
        get => _health;
        set
        {
            _health = value;
            OnHealthChanged();
        }
    }
    [HideInInspector] public StateMachine _stateMachine;
    [HideInInspector] public Rigidbody _rb;

    [HideInInspector] public class HealthEventArgs : EventArgs
    {
        public int healthArg { get; set; }
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void TakeDamage(float amount)
    {
        Health -= amount;
    }

    public virtual void Regenerate(float amount)
    {
        Health += amount;
    }

    public virtual void OnDeath()
    {
        
    }
    
    protected virtual void OnHealthChanged()
    {
        HealthChanged?.Invoke(this, new HealthEventArgs { healthArg = (int)Health.CurrValue });
    }
}
