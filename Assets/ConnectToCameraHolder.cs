using UnityEngine;
using Unity.Netcode;

public class ConnectToCameraHolder : NetworkBehaviour
{
    void Start()
    {
        if (!IsOwner) { return; }
        GameObject camHolder = GameObject.FindGameObjectWithTag("CameraHolder");
        MoveCamera moveCam = camHolder.GetComponent<MoveCamera>();
        moveCam.SetCameraPositionObject(this.gameObject.transform);
    }
}
