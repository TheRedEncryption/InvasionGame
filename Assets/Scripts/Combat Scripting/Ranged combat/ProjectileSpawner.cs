using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProjectileSpawner : MonoBehaviour
{
    enum SpreadMode
    { Rectangle = 0, Circle = 1 }

    private const float converterRate = Mathf.PI / 180;

    [Header("Spawn effects")]
    [SerializeField]
    [Tooltip("The prefab to spawn.")]
    private GameObject _projectile;

    [SerializeField]
    [Tooltip("The sound that'll play when an object is spawned.")]
    private AudioClip _sfx;

    [SerializeField]
    [Tooltip("The maximum scale of the pitch when the sound is played")]
    private float _sfxScale;

    [Header("Properties")]
    [SerializeField]
    [Tooltip("The point to spawn the object at.")]
    protected Transform _spawnPoint;

    [SerializeField]
    [Tooltip("The velocity the object will spawn with.")]
    private float _spawnVelocity;

    [SerializeField]
    [Tooltip("The length of time before the projectile destroyes itself.")]
    private float _projectileLifeSpan;

    [Header("Projectile spread")]
    [SerializeField]
    [Range(0f, 90f)]
    [Tooltip("The maximum spread in degrees")]
    private float _vertical;

    [SerializeField]
    [Range(0f, 90f)]
    [Tooltip("The maximum spread in degrees")]
    private float _horizontal;

    [SerializeField] 
    private SpreadMode _mode;

    protected Vector3 _launchDirection;

    // --- Global projectile variables ---
    public const int MAX_PROJECTILES = 128;
    private static int _currProjectiles = 0;
    public static int GlobalProjectileCount
    {
        get => _currProjectiles;
        set => _currProjectiles = Mathf.Clamp(value, 0, MAX_PROJECTILES);
    }

    public static bool CanSpawnProjectile => GlobalProjectileCount < MAX_PROJECTILES;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private float Rand(float max) => Random.Range(-max, max);

    protected virtual Vector3 GetSpawnPoint() => _spawnPoint.position;
    protected virtual Vector3 GetSpawnDir() => transform.forward;

    public virtual void SpawnProjectile()
    {
        // If this spawner is at max capacity, ignore the spawning
        if (!CanSpawnProjectile) return;

        GameObject projectile = Instantiate(_projectile, GetSpawnPoint(), Quaternion.identity, transform);
        projectile.GetComponent<Projectile>().Initialize(GetSpread(GetSpawnDir()) * _spawnVelocity, _projectileLifeSpan, gameObject);
        
        GetComponent<AudioSource>().pitch = 1 + Rand(_sfxScale);
        GetComponent<AudioSource>().PlayOneShot(_sfx);
        GlobalProjectileCount++;
    }

    private float Cos(float t) => Mathf.Cos(t);
    private float Sin(float t) => Mathf.Sin(t);

    protected Vector3 GetSpread(Vector3 dir)
    {
        float x = 0;
        float y = 0;
        float z = 1;

        // Get random degrees
        float θ = _mode == SpreadMode.Circle ? Random.Range(0, Mathf.PI * 2) : Rand(_horizontal) * converterRate;
        float ϕ = Rand(_vertical) * converterRate;

        float cθ = Cos(θ), sθ = Sin(θ), cϕ = Cos(ϕ), sϕ = Sin(ϕ);

        float x2, y2, z2;

        // Calc random point in spread field
        if (_mode == SpreadMode.Circle)
        {
            // Roll based matrix;
            x2 = x * cθ * cϕ + y * -sθ + z * cθ * sϕ;
            y2 = x * sθ * cϕ + y * -cθ + z * sθ * sϕ;
            z2 = x * -sϕ + z * cϕ;
        }
        else if (_mode == SpreadMode.Rectangle)
        {
            // Box rotation matrix
            x2 = x * cθ + y * sθ * sϕ + z * sθ * cϕ;
            y2 = y * -cϕ + z * sϕ;
            z2 = x * -sθ + y * cθ * sϕ + z * cθ * cϕ;
        }
        else
        {
            x2 = Mathf.Infinity;
            y2 = Mathf.Infinity;
            z2 = Mathf.Infinity;

            Debug.LogWarning("Invalid Spread mode!");
        }

        /*
        // Complete rotation matrix
        float x2 =
        x1 * C(a) * C(b) +
        y1 * (C(a) * S(b) * S(g) - S(a) * C(g)) +
        z1 * (C(a) * S(b) * C(g) + S(a) * S(g));

        float y2 =
        x1 * S(a) * C(b) +
        y1 * (S(a) * S(b) * S(g) - C(a) * C(g)) +
        z1 * (S(a) * S(b) * C(g) + C(a) * S(g));

        float z2 =
        x1 * -S(b) + y1 * C(b) * S(g) + z1 * C(b) * C(g);
        */

        return Quaternion.FromToRotation(Vector3.forward, dir) * new Vector3(x2, y2, z2);
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < 2000; i++)
        {
            Debug.DrawRay(transform.position, GetSpread(GetSpawnDir()), Color.white);
        }
    }
}
