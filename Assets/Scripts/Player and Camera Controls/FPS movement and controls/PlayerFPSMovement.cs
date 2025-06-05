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
    public bool _canMove = true;

    public float crouchOffset;
    public float cameraMoveSpeed;

    public float _slideSpeed;
    public float _defaultSlideTime;
    public float _slopeSpeedModifier = 1f;
    private float slideTime = 5f;
    private bool _isSliding;
    public float _uphillSlideModifier;
    public float _downhillSlideModifier;
    private float _lastY;
    private int deltaYDirection; // -1, 0, 1 (Down, Neutral, Up)

    public Transform cameraPositionOnPlayer;
    private Vector3 camStartLocalPos;

    [Header("Quake 3 air controllers")]
    public float _moveAccel;
    public float _moveDeccel;
    public float _airControl;

    private float MoveSpeed
    {
        get => MoveState switch
        {
            PlayerMoveState.walking => _walkSpeed,
            PlayerMoveState.slow => _slowSpeed,
            PlayerMoveState.sliding => _slowSpeed,
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
            PlayerMoveState.sliding => _slowDrag,
            PlayerMoveState.air => _airDrag,
            _ => Mathf.Infinity,
        };
    }

    // *******************
    // -- Up down movement
    // *******************
    [Header("Jumping")]
    public float _coyotyeTime;
    float _coyotyeTimeCurr;

    public float _jumpCooldown;
    bool _readyToJump = true;

    [Header("Other")]
    public float _slopeExchangeAngle;
    public float _scanHeight;
    public float groundedSphereRadius = 0.48f;
    public Transform _orientation;

    RaycastHit _slopeHit;
    Rigidbody _rb;

    // *******************
    // -- Player state variables
    // *******************
    public enum PlayerMoveState { slow, walking, air, sliding }
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

    /// <summary>
    /// Called on start to initialize variables.
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        camStartLocalPos = cameraPositionOnPlayer.localPosition;
        _lastY = transform.position.y;
        deltaYDirection = 0;
    }

    /// <summary>
    /// Handles per-frame checks
    /// </summary>
    void Update()
    {
        deltaYDirection = (_lastY < transform.position.y && Mathf.Abs(_lastY - transform.position.y) > 0.01) ? 1 : ((_lastY > transform.position.y && Mathf.Abs(_lastY - transform.position.y) > 0.01) ? -1 : 0);
        _lastY = transform.position.y;

        if (PlayerInputHandler.Instance.JumpDown)
        {
            JumpHandler();
        }

        GroundActions();
    }

    /// <summary>
    /// Draws a wire sphere in the editor to visualize the grounded check area.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - Vector3.up * (1 + _scanHeight), groundedSphereRadius);
    }

    /// <summary>
    /// Handles physics-based movement, grounded checks, and state transitions.
    /// </summary>
    private void FixedUpdate()
    {
        Grounded = Physics.SphereCast(transform.position, groundedSphereRadius, Vector3.down, out _slopeHit, 1 + _scanHeight, 1 << 10);

        //Decrease slide time
        if (_isSliding && MoveState == PlayerMoveState.air)
        {
            slideTime -= Time.deltaTime;
            if(slideTime <= _defaultSlideTime)
            {
                _isSliding = false;
            }
        }
        else if (PlayerMoveState.sliding != MoveState)
        {
            slideTime = 0f;
        }
        else if (_isSliding)
            slideTime -= Time.deltaTime + deltaYDirection * (0.02f + Vector3.Angle(Vector3.up, _slopeHit.normal) / 90f * 0.03f) * _slopeSpeedModifier;
        if (slideTime <= 0f)
            _isSliding = false;

        _rb.useGravity = !OnLevelGround;
        StateHandler();

        CrouchHandle();

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

            case PlayerMoveState.sliding:
                MovePlayerSlide(moveDirection);
                break;

            case PlayerMoveState.slow:
                MovePlayerSlow(moveDirection);
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

    /// <summary>
    /// Returns a vector with the Y component set to zero, preserving X and Z.
    /// </summary>
    private Vector3 PlanarVector(Vector3 original) => new Vector3(original.x, 0, original.z);

    /// <summary>
    /// Updates coyote time and last grounded position based on grounded state.
    /// </summary>
    private void GroundActions()
    {
        if (Grounded)
        {
            _coyotyeTimeCurr = _coyotyeTime;

            LastGroundedPosition = transform.position;
        }
        else
        {
            _coyotyeTimeCurr -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Updates the player's movement and slope state, and applies drag.
    /// </summary>
    private void StateHandler()
    {
        PlayerSlopeState = OnSlope();

        PlayerMoveState previousMoveState = MoveState;

        Vector3 planarVel = PlanarVector(_rb.linearVelocity);
        if (!Grounded)
        {
            MoveState = PlayerMoveState.air;
        }
        else if (PlayerInputHandler.Instance.CrouchDown && (planarVel.magnitude) > _slowSpeed-1f)
        {
            MoveState = PlayerMoveState.sliding;
        }
        else if (PlayerInputHandler.Instance.CrouchDown)
        {
            MoveState = PlayerMoveState.slow;
        }
        else
        {
            MoveState = PlayerMoveState.walking;
        }

        if(previousMoveState != MoveState)
        {
            Debug.Log($"Player move state changed from {previousMoveState} to {MoveState} with velocity {planarVel.magnitude}");
        }

        _rb.linearDamping = Drag;
    }

    #region WASD movement

    /// <summary>
    /// Moves the player on the ground using Quake-style acceleration.
    /// </summary>
    private void MovePlayerGroundQuake(Vector3 moveDirection)
    {
        var wishdir = moveDirection;
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();

        var wishspeed = wishdir.magnitude;
        wishspeed *= MoveSpeed;

        Accelerate(wishdir, wishspeed, _moveAccel);

        /*
        // Reset the gravity velocity
        m_PlayerVelocity.y = -m_Gravity * Time.deltaTime;

        if (m_JumpQueued)
        {
            m_PlayerVelocity.y = m_JumpForce;
            m_JumpQueued = false;
        }
        */
    }

    // Calculates acceleration based on desired speed and direction.
    private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
    {
        float currentspeed = Vector3.Dot(_rb.linearVelocity, targetDir);
        float addspeed = targetSpeed - currentspeed;

        if (addspeed <= 0) return;

        float accelspeed = accel * Time.deltaTime * targetSpeed;

        if (accelspeed > addspeed)
            accelspeed = addspeed;

        Vector3 addPos = targetDir * accelspeed;
        _rb.linearVelocity += addPos;
        //m_PlayerVelocity.x += accelspeed * targetDir.x;
        //m_PlayerVelocity.z += accelspeed * targetDir.z;
    }

    /// <summary>
    /// Handles player movement when grounded on level or steep surfaces.
    /// </summary>
    private void MovePlayerGround(Vector3 moveDirection)
    {
        if((PlayerSlopeState == SlopeState.flat || PlayerSlopeState == SlopeState.minorSlope) && _readyToJump)
        {
            if(_rb.linearVelocity.y != 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        // First two check for ground conditions
        // Last two check for air conditions
        if (OnLevelGround) // If on a small slope
        {
            TryMoveXY(MoveSpeed * GetSlopeMoveDirection(moveDirection));
            Debug.DrawRay(transform.position, MoveSpeed * GetSlopeMoveDirection(moveDirection), Color.green);
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

    private void MovePlayerSlide(Vector3 moveDirection)
    {
        if ((PlayerSlopeState == SlopeState.flat || PlayerSlopeState == SlopeState.minorSlope || PlayerSlopeState == SlopeState.steepSlope) && _readyToJump)
        {
            if (_rb.linearVelocity.y != 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        if (!_isSliding)
        {
            slideTime = _defaultSlideTime;
            _isSliding = true;
        }
        else
        {
            //Debug.Log(Vector3.Angle(Vector3.up, _slopeHit.normal));
            float slopeEquation;
            if (deltaYDirection == -1f)
            {
                slopeEquation = _slopeSpeedModifier * 0.1f * _downhillSlideModifier * Mathf.Pow(Vector3.Angle(Vector3.up, _slopeHit.normal) / 90f - 0, 2) + 1;
            }
            else if (deltaYDirection == 1f)
            {
                slopeEquation = _slopeSpeedModifier * -0.1f * _uphillSlideModifier * Mathf.Pow(Vector3.Angle(Vector3.up, _slopeHit.normal) / 90f - 0, 2) + 1;
            }
            else
            {
                slopeEquation = 1f;
            }
            TryMoveXY(GetSlopeMoveDirection(moveDirection) * (_slideSpeed) * slideTime / _defaultSlideTime * slopeEquation, ForceMode.Impulse);
            return;
        }
    }

    /// <summary>
    /// Handles player movement when crouching or sliding.
    /// </summary>
    private void MovePlayerSlow(Vector3 moveDirection)
    {
        if ((PlayerSlopeState == SlopeState.flat || PlayerSlopeState == SlopeState.minorSlope || PlayerSlopeState == SlopeState.steepSlope) && _readyToJump)
        {
            if (_rb.linearVelocity.y != 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
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

    /// <summary>
    /// Handles player movement while airborne, including speed limiting and direction changes.
    /// </summary>
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

    // "Handle air movement"

    /// <summary>
    /// Handles air movement using Quake/CPM-style air control.
    /// </summary>
    private void MovePlayerAirNew(Vector3 moveDirection)
    {
        float accel;

        var wishdir = moveDirection;
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= MoveSpeed;

        wishdir = wishdir.normalized;

        // CPM Air control.
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(_rb.linearVelocity, wishdir) < 0)
        {
            accel = _moveDeccel;
        }
        else
        {
            accel = _moveAccel;
        }

        /*
        // If the player is ONLY strafing left or right
        if (PlayerInputHandler.Instance.MoveInput.y == 0 && PlayerInputHandler.Instance.MoveInput.x != 0)
        {
            if (wishspeed > m_StrafeSettings.MaxSpeed)
            {
                wishspeed = m_StrafeSettings.MaxSpeed;
            }

            accel = m_StrafeSettings.Acceleration;
        }
        */

        Accelerate(wishdir, wishspeed, accel);
        if (_airControl > 0)
        {
            AirControl(wishdir, wishspeed2);
        }
    }

    // Air control occurs when the player is in the air, it allows players to move side 
    // to side much faster rather than being 'sluggish' when it comes to cornering.

    /// <summary>
    /// Provides additional air control for sharper turns while airborne.
    /// </summary>
    private void AirControl(Vector3 targetDir, float targetSpeed)
    {
        // Only control air movement when moving forward or backward.
        if (Mathf.Abs(PlayerInputHandler.Instance.MoveInput.y) < 0.001 || Mathf.Abs(targetSpeed) < 0.001) return;

        float zSpeed = _rb.linearVelocity.y;
        _rb.linearVelocity -= new Vector3(0, _rb.linearVelocity.y, 0);
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        float speed = _rb.linearVelocity.magnitude;
        _rb.linearVelocity = _rb.linearVelocity.normalized;

        float dot = Vector3.Dot(_rb.linearVelocity, targetDir);
        float k = 32;
        k *= _airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down.
        if (dot > 0)
        {
            _rb.linearVelocity = new( _rb.linearVelocity.x * (speed + targetDir.x * k), _rb.linearVelocity.y * (speed + targetDir.y * k), _rb.linearVelocity.z * (speed + targetDir.z * k) );

            _rb.linearVelocity = _rb.linearVelocity.normalized;
        }

        Vector3 playSpeed = _rb.linearVelocity;

        playSpeed.x *= speed;
        playSpeed.y = zSpeed; // Note this line
        playSpeed.z *= speed;

        _rb.linearVelocity = playSpeed;
    }

    /// <summary>
    /// Attempts to move the player in the XZ plane, applying speed limits and slope corrections.
    /// </summary>
    private void TryMoveXY(Vector3 amount, ForceMode forceMode = ForceMode.VelocityChange)
    {
        if (!_canMove) { return; }
        Vector3 preVelocityX = PlanarVector(_rb.linearVelocity);
        Vector3 amountVelX = new(amount.x, 0, amount.z);
        Vector3 amountOrigY = new Vector3(0, amount.y, 0);

        if ((preVelocityX + amountVelX).magnitude > MoveSpeed && forceMode == ForceMode.VelocityChange)
        {
            Vector3 goalX = NormalizeToMoveSpeed(preVelocityX + amountVelX, preVelocityX.magnitude);
            amount = goalX - preVelocityX;
        }

        _rb.AddForce(Grounded ? Vector3.ProjectOnPlane(amount, _slopeHit.normal) : amount, forceMode);
    }

    /// <summary>
    /// Normalizes a vector to the current move speed, preserving direction.
    /// </summary>
    private Vector3 NormalizeToMoveSpeed(Vector3 amount, float speed) => amount.normalized * Mathf.Max(MoveSpeed, speed);

    #endregion

    #region Jumping

    /// <summary>
    /// Handles jump input, coyote time, and jump cooldown logic.
    /// </summary>
    private void JumpHandler()
    {
        if (_readyToJump && _coyotyeTimeCurr > 0)
        {
            _readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    /// <summary>
    /// Applies an upward force to the Rigidbody to make the player jump.
    /// </summary>
    private void Jump()
    {

        if (_coyotyeTimeCurr > 0)
        {
            _coyotyeTimeCurr = 0;
        }

        // reset y velocity
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        if (OnSteepGround)
        {
            Vector3 across = Vector3.Cross(_slopeHit.normal, Vector3.down).normalized;
            Vector3 vecProjection = Vector3.Project(InputDir * MoveSpeed, across).normalized;
            _canMove = false;
            _jumpCooldown = 0.5f;
            Invoke("ResetMovement", 0.15f);
            _rb.AddForce(new Vector3(_slopeHit.normal.x, 0.15f, _slopeHit.normal.z) * _jumpSpeed * 1.5f + vecProjection, ForceMode.VelocityChange);
        }
        else
        {
            _rb.AddForce(transform.up * _jumpSpeed, ForceMode.VelocityChange);
        }
    }

    private void ResetMovement()
    {
        _canMove = true;
    }

    /// <summary>
    /// Resets jump state and coyote time after the jump cooldown.
    /// </summary>
    private void ResetJump()
    {
        _coyotyeTimeCurr = 0;
        _readyToJump = true;
    }
    #endregion

    #region Couching

    /// <summary>
    /// Handles crouch and slide state, including camera position adjustments.
    /// </summary>
    private void CrouchHandle()
    {
        if (!PlayerInputHandler.Instance.CrouchDown)
        {
            _isSliding = false;
            cameraPositionOnPlayer.localPosition = Vector3.Lerp(
                cameraPositionOnPlayer.localPosition,
                camStartLocalPos,
                Time.deltaTime * cameraMoveSpeed
            );
        }
        else
        {
            // decide target local position
            Vector3 target = camStartLocalPos + Vector3.down * crouchOffset;
            // smoothly move towards it
            cameraPositionOnPlayer.localPosition = Vector3.Lerp(
                cameraPositionOnPlayer.localPosition,
                target,
                Time.deltaTime * cameraMoveSpeed
            );
        }
    }
    #endregion

    #region Slope Handling

    /// <summary>
    /// Returns the current slope state based on the ground normal and angle.
    /// </summary>
    /// <returns>The current slopeState.</returns>
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

    /// <summary>
    /// Projects a direction vector onto the slope plane and normalizes it.
    /// </summary>
    /// <param name="direction">The direction to project.</param>
    /// <returns>Normalized direction projected onto the slope.</returns>
    public Vector3 GetSlopeMoveDirection(Vector3 direction) => Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;

    #endregion
}

