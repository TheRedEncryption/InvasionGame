using UnityEngine;


[RequireComponent(typeof(StateMachine), typeof(Rigidbody))]
public class Entity : MonoBehaviour
{
    public CappedFloat _health;
    [HideInInspector] public StateMachine _stateMachine;
    [HideInInspector] public Rigidbody _rb;

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
        _health -= amount;
    }

    public virtual void Regenerate(float amount)
    {
        _health += amount;
    }

    public virtual void OnDeath()
    {
        
    }
    
}
