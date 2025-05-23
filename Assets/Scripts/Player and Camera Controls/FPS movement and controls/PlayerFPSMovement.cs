using UnityEngine;

public class PlayerFPSMovement : MonoBehaviour
{
    // *******************
    // -- Planar Movement
    // *******************
    [Header("Move speed")]

    // Move speeds
    public float _slowSpeed;
    public float _walkSpeed;
    public float _jumpSpeed;
    public float _moveSpeedError;

    private float MoveSpeed
    {
        get => MoveState switch
        {
            PlayerMoveState.walking => _walkSpeed,
            PlayerMoveState.slow => _slowSpeed,
            PlayerMoveState.air => _walkSpeed,
            _ => Mathf.Infinity,
        };
    }
    public Vector3 InputDir => _orientation.forward * PlayerInputHandler.Instance.MoveInput.y + _orientation.right * PlayerInputHandler.Instance.MoveInput.x;

    [Header("Drag")]
    public float _groundDrag;
    public float _airDrag = 0;
    public float _slowDrag;
    private float Drag
    {
        get => MoveState switch
        {
            PlayerMoveState.walking => _groundDrag,
            PlayerMoveState.slow => _slowDrag,
            PlayerMoveState.air => _airDrag,
            _ => Mathf.Infinity,
        };
    }

    // *******************
    // -- Up down movement
    // *******************
    [Header("Jumping")]
    public float _cyotyeTime;
    float _cyotyeTimeCurr;

    public float _jumpCooldown;
    bool _readyToJump = true;

    [Header("Other")]
    public float _slopeExchangeAngle;
    public float _scanHeight;
    public Transform _orientation;

    RaycastHit _slopeHit;
    Rigidbody _rb;

    // *******************
    // -- Player state variables
    // *******************
    public enum PlayerMoveState { slow, walking, air }
    public enum SlopeState { flat = -2, minorSlope = -1, none = 0, steepSlope = 1, vertical = 2 }
    public PlayerMoveState MoveState { get; private set; }
    public SlopeState PlayerSlopeState { get; private set; }

    // player slope state data helpers
    public bool OnLevelGround { get => PlayerSlopeState < 0; }
    public bool OnSteepGround { get => PlayerSlopeState > 0; }
    public bool Grounded { get; private set; }
    public Vector3 LastGroundedPosition { get; private set; }


    // *******************
    // -- Unity Mono Methods
    // *******************

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    void Update()
    {
        if (PlayerInputHandler.Instance.JumpDown)
        {
            JumpHandler();
        }

        GroundActions();
    }

    private void FixedUpdate()
    {
        Grounded = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out _slopeHit, 1 + _scanHeight, 1 << 10);


        _rb.useGravity = !OnLevelGround;
        StateHandler();

        // calculate movement direction
        Vector3 moveDirection = InputDir;

        if (moveDirection.magnitude == 0)
            return;

        // Execute movement based on movestate
        switch (MoveState)
        {
            case PlayerMoveState.walking:
                MovePlayerGround(moveDirection);
                break;

            case PlayerMoveState.air:
                MovePlayerAir(moveDirection);
                break;

            default:
                Debug.LogWarning("Invalid Movestate: " + MoveState);
                break;
        }

    }

    // *******************
    // -- My Methods
    // *******************
    private Vector3 PlanarVector(Vector3 original) => new Vector3(original.x, 0, original.z);
    private void GroundActions()
    {
        if (Grounded)
        {
            _cyotyeTimeCurr = _cyotyeTime;

            LastGroundedPosition = transform.position;
        }
        else
        {
            _cyotyeTimeCurr -= Time.deltaTime;
        }
    }

    private void StateHandler()
    {
        PlayerSlopeState = OnSlope();

        if (!Grounded)
        {
            MoveState = PlayerMoveState.air;
        }
        else
        {
            MoveState = PlayerMoveState.walking;
        }

        _rb.linearDamping = Drag;
    }

    #region WASD movement
    private void MovePlayerGround(Vector3 moveDirection)
    {
        // First two check for ground conditions
        // Last two check for air conditions
        if (OnLevelGround) // If on a small slope
        {
            TryMoveXY(MoveSpeed * GetSlopeMoveDirection(moveDirection));
        }
        else if (OnSteepGround) // If on a big slope
        {
            Vector3 slopePlanarDir = PlanarVector(_slopeHit.normal);
            Vector3 projectedMovement = PlanarVector(moveDirection);

            if (Vector3.Dot(slopePlanarDir, projectedMovement) > 0)
            {
                TryMoveXY(MoveSpeed * projectedMovement);
            }
        }
        else
        {
            Debug.LogWarning("Invalid grounded slope angle.");
        }

    }

    private void MovePlayerAir(Vector3 moveDirection)
    {
        if (PlanarVector(_rb.linearVelocity).magnitude <= MoveSpeed + _moveSpeedError) // If moving normal speeds
        {
            if (moveDirection.magnitude > 0)
            {
                TryMoveXY(MoveSpeed * moveDirection.normalized);
            }
            else
            {
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x * 0.85f, _rb.linearVelocity.y, _rb.linearVelocity.z * 0.85f);
            }

        }
        else if (moveDirection.magnitude > 0) // If soaring
        {
            Vector3 planarVelocity = PlanarVector(_rb.linearVelocity);
            Vector3 yVelocity = new Vector3(0, _rb.linearVelocity.y, 0);

            float planarMag = planarVelocity.magnitude;

            Vector3 weightedDir = (planarVelocity * 0.9f + moveDirection).normalized * planarMag;
            weightedDir += yVelocity;
            _rb.linearVelocity = weightedDir;

        }
        else if (moveDirection.magnitude == 0)
        {
            Vector3 oppositePlanarMoveDirection = PlanarVector(-_rb.linearVelocity).normalized;

            _rb.AddForce(oppositePlanarMoveDirection * 0.1f);

        }
        else
        {
            Debug.LogWarning("Invalid air movement speed: " + moveDirection.magnitude);
        }
    }

    private void TryMoveXY(Vector3 amount)
    {
        Vector3 preVelocityX = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 amountVelX = new(amount.x, 0, amount.z);

        if ((preVelocityX + amountVelX).magnitude > MoveSpeed)
        {
            Vector3 goalX = NormalizeToMoveSpeed(preVelocityX + amountVelX, preVelocityX.magnitude);
            amount = goalX - preVelocityX;
        }

        _rb.AddForce(amount, ForceMode.VelocityChange);
    }

    private Vector3 NormalizeToMoveSpeed(Vector3 amount, float speed) => amount.normalized * Mathf.Max(MoveSpeed, speed);
    #endregion





    #region Jumping
    private void JumpHandler()
    {
        if (_readyToJump && _cyotyeTimeCurr > 0)
        {
            _readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    private void Jump()
    {
        if (_cyotyeTimeCurr > 0)
        {
            _cyotyeTimeCurr = 0;
        }

        // reset y velocity
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        if (OnSteepGround)
        {
            _rb.AddForce(_slopeHit.normal * _jumpSpeed, ForceMode.VelocityChange);
        }
        else
        {
            _rb.AddForce(transform.up * _jumpSpeed, ForceMode.VelocityChange);
        }
    }

    private void ResetJump()
    {
        _cyotyeTimeCurr = 0;
        _readyToJump = true;
    }
    #endregion

    #region Slope Handling

    public SlopeState OnSlope()
    {
        if (!Grounded) return SlopeState.none;

        float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);

        // Determine slope quality based on angle
        if (slopeAngle == 0f)
        {
            return SlopeState.flat;
        }
        else if (slopeAngle == 90f)
        {
            return SlopeState.vertical;
        }
        else if (slopeAngle < _slopeExchangeAngle)
        {
            return SlopeState.minorSlope;
        }
        else
        {
            return SlopeState.steepSlope;
        }
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) => Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;

    #endregion
}

