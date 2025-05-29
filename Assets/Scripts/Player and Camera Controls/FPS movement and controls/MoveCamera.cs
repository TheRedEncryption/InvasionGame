using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform cameraPosition;

    void Update()
    {
        if (cameraPosition == null) { return; }
        transform.position = cameraPosition.position;
    }

    public void SetCameraPositionObject(Transform camPos)
    {
        this.cameraPosition = camPos;
    }
}
