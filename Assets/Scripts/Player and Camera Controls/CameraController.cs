using System;
using NUnit.Framework;
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
    [SerializeField][UnityEngine.Range(0.01f, 1f)] private float _scaleSpeed = 0.5f;

    public float zoomSpeed = 10;
    public float planeMoveBaseSpeed = 10;

    private float _desiredHeight;

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
    public static Vector3 CameraForward { get; private set; }

    #region Singleton implementation

    public static CameraController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            Debug.LogWarning("WARNING!: Additional camera object detected!");
        }
    }

    #endregion

    void Start()
    {
        cam = Camera.main;
        SwitchCameraState(currentCameraState); // for testing purposes
        CameraForward = Vector3.forward;
    }

    void Update()
    {
        if (!ValidateCameraOperable()) { return; }

        // TODO: 
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

    /// <summary>
    /// Validates the operability of the camera, based on if its dependant assignments are present within the Scene.
    /// </summary>
    /// <returns> Whether the camera is operable in top-down and FPS views. </returns>
    bool ValidateCameraOperable()
    {
        // for future understanding, add more conditions if you want to bar the camera from being used,
        // as of right now it is checking for the existence of the "Camera Position On Player" and
        // "Orientation" GameObjects.
        return cameraTarget && _playerOrientation;
    }

    #region  TOP-DOWN

    private void LerpBirdHeight()
    {
        if (Math.Abs(currentHeight - _desiredHeight) > 0.05)
            currentHeight = Mathf.Lerp(currentHeight, _desiredHeight, _scaleSpeed);
        else
            currentHeight = _desiredHeight;
    }

    private float GetMinimumHeight()
    {
        Physics.SphereCast(new Vector3(transform.position.x, maximumHeight, transform.position.z), 1.2f, Vector3.down, out RaycastHit hit, maximumHeight - minimumHeight);
        return Mathf.Clamp(hit.point.y + minimumHeight, minimumHeight, maximumHeight);
    }

    private void SwitchToTopDown()
    {
        Debug.Log("[CameraController]: Switched to top-down view!");
        _desiredHeight = currentHeight;
        cam.transform.parent = null;
        transform.SetPositionAndRotation(new Vector3(transform.position.x, currentHeight, transform.position.z), Quaternion.Euler(new Vector3(90f, 0f, 0f)));
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        CameraForward = new Vector3(90f, 0f, 0f);

        GetComponent<ObjectPlacer>().enabled = true;

        // TODO: move these into their own PhaseManager (the script to manage the game phase) unless we want the camera to be in control of displaying/hiding UI elements
        StateActiveList stateActiveList = FindFirstObjectByType<StateActiveList>();
        if (stateActiveList != null)
        {
            Array.ForEach(stateActiveList.activeInFPSStageOnly, obj =>
            {
                var uiDoc = obj.GetComponent<UnityEngine.UIElements.UIDocument>();
                if (uiDoc == null)
                {
                    obj.SetActive(true);
                }
                else
                {
                    // If the object has a UIDocument, we want to disable it
                    uiDoc.rootVisualElement.style.display = UnityEngine.UIElements.DisplayStyle.None;
                }
            });
            Array.ForEach(stateActiveList.activeInBuildStageOnly, obj => obj.SetActive(true));
        }
        else
        {
            Debug.LogWarning("[CameraController]: No StateActiveList found.");
        }
    }

    private void HandleTopDown()
    {
        // Scroll-wheel zoom logic
        _desiredHeight = Mathf.Clamp(_desiredHeight + Input.mouseScrollDelta.y * sensitivity.y / 5f, GetMinimumHeight(), maximumHeight);
        LerpBirdHeight();
        Vector3 targetLocation = new Vector3(transform.position.x, currentHeight, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetLocation, zoomSpeed * Time.deltaTime);

        // Input movement
        float heightSpeedScale = Mathf.Sqrt(currentHeight - minimumHeight + 1f);
        float shiftSpeedScale = PlayerInputHandler.Instance.CrouchDown && !PlayerInputHandler.Instance.AttackDown ? 3f : 1f;

        // Click and drag or WASD
        Vector2 inputVector = PlayerInputHandler.Instance.AttackDown ?
            -PlayerInputHandler.Instance.LookInput / 2f : PlayerInputHandler.Instance.MoveInput;

        Vector2 newPos = shiftSpeedScale * heightSpeedScale * planeMoveBaseSpeed * Time.deltaTime * inputVector + new Vector2(transform.position.x, transform.position.z);

        // Bounds
        float newX = Mathf.Clamp(newPos.x, boundary.minX, boundary.maxX);
        float newZ = Mathf.Clamp(newPos.y, boundary.minZ, boundary.maxZ);

        transform.position = new Vector3(newX, currentHeight, newZ);
    }

    #endregion

    public void OnDrawGizmosSelected()
    {
        Vector3 bottomLeft = new(boundary.minX, transform.position.y, boundary.minZ);
        Vector3 bottomRight = new(boundary.maxX, transform.position.y, boundary.minZ);
        Vector3 topLeft = new(boundary.minX, transform.position.y, boundary.maxZ);
        Vector3 topRight = new(boundary.maxX, transform.position.y, boundary.maxZ);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    #region  FIRST-PERSON

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

        GetComponent<ObjectPlacer>().enabled = false;

        // TODO: move these into their own PhaseManager (the script to manage the game phase) unless we want the camera to be in control of displaying/hiding UI elements
        StateActiveList stateActiveList = FindFirstObjectByType<StateActiveList>();
        if (stateActiveList != null)
        {
            Array.ForEach(stateActiveList.activeInFPSStageOnly, obj =>
            {
                var uiDoc = obj.GetComponent<UnityEngine.UIElements.UIDocument>();
                if (uiDoc == null)
                {
                    obj.SetActive(true);
                }
                else
                {
                    // If the object has a UIDocument, we want to enable it
                    uiDoc.rootVisualElement.style.display = UnityEngine.UIElements.DisplayStyle.Flex;
                }
            });
            Array.ForEach(stateActiveList.activeInBuildStageOnly, obj => obj.SetActive(false));
        }
        else
        {
            Debug.LogWarning("[CameraController]: No StateActiveList found.");
        }

        GetComponent<BirdsEyeNetworkLink>().DoTheSpawning();
    }

    private void HandleFirstPerson()
    {
        // TODO: implement body
        // get mouse input
        Vector2 mouseDelta = GetInput() * sensitivity;

        rotation += mouseDelta * Time.deltaTime;

        rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);

        // rotate cam and orientation
        CameraForward = Quaternion.Euler(new Vector3(rotation.y, rotation.x, 0)) * Vector3.forward;
        cam.transform.rotation = Quaternion.Euler(new Vector3(rotation.y, rotation.x, 0));
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

    #endregion

    #region  CAMERA TARGET

    public void SetCameraTarget(GameObject other)
    {
        cameraTarget = other;
    }

    public GameObject GetCameraTarget()
    {
        return cameraTarget;
    }

    public void SetPlayerOrientationTransform(Transform playerOrientationTransform)
    {
        this._playerOrientation = playerOrientationTransform;
    }

    #endregion
}
