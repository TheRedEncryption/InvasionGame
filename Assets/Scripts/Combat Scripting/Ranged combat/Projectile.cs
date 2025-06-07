using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float _damage;
    private GameObject _host;
    public delegate void HasBeenDestroyed();
    public event HasBeenDestroyed OnDestroy;

    // Update is called once per frame
    void Update()
    {
    }

    public virtual void Initialize(Vector3 velocity, float lifeSpan, GameObject host)
    {
        _host = host;

        Invoke(nameof(Destroy), lifeSpan);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = velocity;

        // Need to assign the rotation twice to remove stutter
        rb.MoveRotation(Quaternion.LookRotation(velocity.normalized));
        transform.forward = velocity.normalized;
    }

    public virtual void Destroy()
    {
        ProjectileSpawner.GlobalProjectileCount--;
        OnDestroy?.Invoke();

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == _host) return;

        collision.gameObject.GetComponent<Entity>()?.TakeDamage(_damage);

        Destroy();
    }
}
