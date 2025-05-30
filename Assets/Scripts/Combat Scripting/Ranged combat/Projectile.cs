using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{

    public delegate void HasBeenDestroyed();
    public event HasBeenDestroyed OnDestroy;

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void Initialize(Vector3 velocity, float lifeSpan)
    {
        Invoke(nameof(Destroy), lifeSpan);
        transform.forward = velocity.normalized;
        GetComponent<Rigidbody>().linearVelocity = velocity;
    }

    public virtual void Destroy()
    {
        ProjectileSpawner.GlobalProjectileCount--;
        OnDestroy?.Invoke();

        Destroy(gameObject);
    }
}
