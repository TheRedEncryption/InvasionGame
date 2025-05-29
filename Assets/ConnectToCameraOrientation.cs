using UnityEngine;
using Unity.Netcode;

public class ConnectToCameraOrientation : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner) { return; }
        CameraController camCon = Camera.main.GetComponent<CameraController>();
        camCon.SetPlayerOrientationTransform(this.gameObject.transform);
    }
}