using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    [Header("General Settings")]
    [SerializeField] private GameObject cameraTarget;

    // TODO: replace with proper State functionality once StateMachine and State are implemented 
    public enum CameraState
    {
        TopDown,
        FirstPerson
    }

    public CameraState currentCameraState;
    // end TODO

    [Header("Top-Down View Controls")]
    [SerializeField] private float minimumHeight = 1;  // placeholder values that will get programatically decided
    [SerializeField] private float maximumHeight = 100; // either by an algorithm per the map generation, or
    [SerializeField] private float currentHeight = 5;  // through some other heuristic

    public float zoomSpeed = 10;
    public float planeMoveSpeed = 10;

    [Header("First-Person View Controls")]
    [SerializeField] private float sensitivity; // (or whatever list of parameters works)

    void Start()
    {
        cam = Camera.main;
        SwitchCameraState(currentCameraState); // for testing purposes
    }

    void Update()
    {
        if (currentCameraState == CameraState.TopDown)
        {
            HandleTopDown();
        }
        else if (currentCameraState == CameraState.FirstPerson)
        {
            HandleFirstPerson();
        }
    }

    public void SwitchCameraState(CameraState camState)
    {
        currentCameraState = camState;

        if (currentCameraState == CameraState.TopDown)
        {
            SwitchToTopDown();
        }
        else if (currentCameraState == CameraState.FirstPerson)
        {
            SwitchToFirstPerson();
        }
    }

    // TOP-DOWN/BIRDS

    private void SwitchToTopDown()
    {
        Debug.Log("[CameraController]: Switched to top-down view!");
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f)); // look down
    }

    private void HandleTopDown()
    {
        // Scroll-wheel zoom logic
        currentHeight = Mathf.Clamp(currentHeight + -Input.mouseScrollDelta.y, minimumHeight, maximumHeight);
        Vector3 targetLocation = new Vector3(transform.position.x, currentHeight, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetLocation, zoomSpeed * Time.deltaTime);

        // WASD movement
        Vector3 moveXY = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        transform.position += moveXY * planeMoveSpeed * Time.deltaTime;
    }

    // FIRST-PERSON

    private void SwitchToFirstPerson()
    {
        // NOTE: This method is for changing the camera's settings to the FPS upon state change.
        // All input handling concerning the control of the camera MUST be handled in HandleFirstPerson()
        Debug.Log("[CameraController]: Switched to FPS view!");
    }


    private void HandleFirstPerson()
    {
        //
    }

    // CAMERA TARGET

    public void SetCameraTarget(GameObject other)
    {
        cameraTarget = other;
    }

    public GameObject GetCameraTarget()
    {
        return cameraTarget;
    }
}
