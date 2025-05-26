using System;
using Unity.VisualScripting;
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
    [SerializeField] private Bounds2D boundary;

    public float zoomSpeed = 10;
    public float planeMoveBaseSpeed = 10;

    [Header("FPS View Controls")]
    [SerializeField] private GameObject _cameraOrientation;
    [SerializeField] private Transform _playerOrientation;
    [SerializeField] private Vector2 sensitivity;
    [SerializeField] private Vector2 acceleration;
    [SerializeField] private float inputLagPeriod;
    private Vector2 rotation;
    private float inputLagTimer;
    private Vector2 lastInputEvent;

    // TODO: implement boundaries

    void Start()
    {
        cam = Camera.main;
        SwitchCameraState(currentCameraState); // for testing purposes
    }

    void Update()
    {

        if (PlayerInputHandler.Instance.CamSwitchTriggered)
        {
            if (currentCameraState == CameraState.TopDown)
            {
                SwitchCameraState(CameraState.FirstPerson);
            }
            else if (currentCameraState == CameraState.FirstPerson)
            {
                SwitchCameraState(CameraState.TopDown);
            }
        }

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

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void HandleTopDown()
    {
        // Scroll-wheel zoom logic
        currentHeight = Mathf.Clamp(currentHeight + -Input.mouseScrollDelta.y, minimumHeight, maximumHeight);
        Vector3 targetLocation = new Vector3(transform.position.x, currentHeight, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetLocation, zoomSpeed * Time.deltaTime);

        // WASD movement
        Vector3 moveXY = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float heightSpeedScale = Mathf.Sqrt(currentHeight - minimumHeight + 1f);
        transform.position += heightSpeedScale * planeMoveBaseSpeed * Time.deltaTime * moveXY;

        // Bounds
        float newX = Mathf.Clamp(transform.position.x, boundary.minX, boundary.maxX);
        float newZ = Mathf.Clamp(transform.position.z, boundary.minZ, boundary.maxZ);

        transform.position = new Vector3(newX, currentHeight, newZ);
    }

    public void OnDrawGizmosSelected()
    {
        Vector3 bottomLeft = new Vector3(boundary.minX, transform.position.y, boundary.minZ);
        Vector3 bottomRight = new Vector3(boundary.maxX, transform.position.y, boundary.minZ);
        Vector3 topLeft = new Vector3(boundary.minX, transform.position.y, boundary.maxZ);
        Vector3 topRight = new Vector3(boundary.maxX, transform.position.y, boundary.maxZ);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    // FIRST-PERSON

    private void SwitchToFirstPerson()
    {
        // NOTE: This method is for changing the camera's settings to the FPS upon state change.
        // All input handling concerning the control of the camera MUST be handled in HandleFirstPerson()
        Debug.Log("[CameraController]: Switched to FPS view!");

        // TODO: implement body
        cam.transform.parent = _cameraOrientation.transform;
        cam.transform.position = _cameraOrientation.transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void HandleFirstPerson()
    {
        // TODO: implement body
        // get mouse input
        Vector2 mouseDelta = GetInput() * sensitivity;

        rotation += mouseDelta * Time.deltaTime;

        rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);

        // rotate cam and orientation
        cam.transform.rotation = Quaternion.Euler(rotation.y, rotation.x, 0);
        _playerOrientation.transform.rotation = Quaternion.Euler(0, rotation.x, 0);
    }

    private Vector2 GetInput()
    {
        inputLagTimer += Time.deltaTime;
        Vector2 input = PlayerInputHandler.Instance.LookInput;

        if ((Mathf.Approximately(0, input.x) && Mathf.Approximately(0, input.y)) == false || inputLagTimer >= inputLagPeriod)
        {
            lastInputEvent = input;
            inputLagTimer = 0;
        }

        return lastInputEvent;
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
