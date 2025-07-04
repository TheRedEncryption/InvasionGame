using UnityEngine;

public class PlayerShootDemo : ProjectileSpawner
{
    public Transform _cameraTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraController.Instance.currentCameraState != CameraController.CameraState.FirstPerson) return;

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
