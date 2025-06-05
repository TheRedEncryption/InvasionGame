using UnityEngine;

public class PlayerShootDemo : ProjectileSpawner
{
    public Transform _cameraTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cameraTransform = Camera.main.transform;
        Debug.Log(_cameraTransform);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerInputHandler.Instance.AttackTriggered)
        {
            SpawnProjectile();
        }
    }

    protected override Vector3 GetSpawnPoint()
    {
        return _cameraTransform.position + GetSpawnDir() * 1f;
    }
    protected override Vector3 GetSpawnDir()
    {
        return _cameraTransform.forward;
    }

    public override void SpawnProjectile()
    {
        base.SpawnProjectile();
    }
}
