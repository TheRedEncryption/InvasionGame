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
        Debug.DrawRay(transform.position, CameraController.CameraForward * 10f, Color.red);

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
        return CameraController.CameraForward;
    }

    public override void SpawnProjectile()
    {
        base.SpawnProjectile();
    }
}
