using UnityEngine;
using Unity.Netcode;

public class ConnectToMainCamera : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner) { return; }
        CameraController camCon = Camera.main.GetComponent<CameraController>();
        camCon.SetCameraTarget(this.gameObject);
    }
}
