using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
  [Header("Ground")]
  [SerializeField] private LayerMask _groundLayer;
  [SerializeField] private Transform _groundRayLeft;
  [SerializeField] private Transform _groundRayRight;
  [SerializeField] private float _groundRayLength = 0.05f;
  [SerializeField] private bool _showGroundRays;
  private bool _isOnGround;

  [Header("Wall")]
  [SerializeField] private LayerMask _wallLayer;
  [SerializeField] private Transform _wallRayTop;
  [SerializeField] private Transform _wallRayBottom;
  [SerializeField] private float _wallRayLength = 0.15f;
  [SerializeField] private bool _showWallRays;
  private bool _isOnWall;

  [Header("Movement")]
  [SerializeField] private float _moveSpeed = 6f;
  private bool _isFacingRight;

  [Header("Jumping")]
  [SerializeField] private float _jumpForce = 10f;
  [SerializeField] private float _coyoteTime = 0.12f;
  [SerializeField] private float _jumpBufferTime = 0.2f;
  private float _coyoteTimer;
  private float _jumpBufferTimer;
  private float _lastJumpTime;
  private int _jumpsUsed = 0;
  private const int MAX_JUMPS = 2;

  [Header("Falling")]
  [SerializeField] private float _extraGravity = 25f;
  [SerializeField] private float _maxFallSpeed = 25f;
  private float _normalGravity;

  [Header("Wall Slide")]
  [SerializeField] private float _wallStickTime = 0.125f;
  [SerializeField] private float _wallSlideSpeed = 0.5f;

  [Header("Wall Jump")]
  [SerializeField] private Vector2 _wallJumpForce = new(5f, 10f);
  [SerializeField] private float _wallJumpTime = .15f;
  private bool _hasWallJumpMomentum;

  [Header("Climbing")]
  [SerializeField] private float _climbSpeed = 3f;
  [SerializeField] private float _clampToClimbableDuration = 0.075f;
  [SerializeField] private float _minimumClimbTime = 0.5f;
  [SerializeField] private Vector2 _moveOffClimbableForce = new(2f, 2f);
  [SerializeField] private float _climbReEntryCooldown = 0.3f;
  private float _climbPositionX;
  private bool _isOnClimbable;
  private float _climbCooldownTimer;

  [Header("Dash")]
  [SerializeField] private float _dashForce = 20f;
  [SerializeField] private float _maxDashDuration = 0.25f;
  [SerializeField] private float _dashCooldownTime = 0.5f;
  private float _dashCooldownTimer;

  [Header("Abilities")]
  [SerializeField] private bool _doubleJumpUnlocked = true;
  [SerializeField] private bool _wallJumpUnlocked = true;
  [SerializeField] private bool _dashUnlocked = true;

  #region Components
  public Rigidbody2D Rigidbody { get; private set; }
  public Animator Animator { get; private set; }
  public FrameInput FrameInput { get; private set; }
  private PlayerInputManager _playerInputManager;
  #endregion

  #region Getters
  public bool IsOnGround => _isOnGround;
  public bool IsOnWall => _isOnWall;
  public bool IsOnClimbable => _isOnClimbable;
  public bool IsFacingRight => _isFacingRight;
  public float MoveSpeed => _moveSpeed;
  public float JumpForce => _jumpForce;
  public float CoyoteTime => _coyoteTime;
  public float CoyoteTimer => _coyoteTimer;
  public float JumpBufferTimer => _jumpBufferTimer;
  public float ExtraGravity => _extraGravity;
  public float MaxFallSpeed => _maxFallSpeed;
  public float NormalGravity => _normalGravity;
  public int JumpsUsed => _jumpsUsed;
  public float WallStickTime => _wallStickTime;
  public float WallSlideSpeed => _wallSlideSpeed;
  public Vector2 WallJumpForce => _wallJumpForce;
  public float WallJumpTime => _wallJumpTime;
  public bool HasWallJumpMomentum => _hasWallJumpMomentum;
  public float ClimbSpeed => _climbSpeed;
  public float MinimumClimbTime => _minimumClimbTime;
  public Vector2 MoveOffClimbableForce => _moveOffClimbableForce;
  public float ClimbReEntryCooldown => _climbReEntryCooldown;
  public float DashForce => _dashForce;
  public float MaxDashDuration => _maxDashDuration;
  public float DashCooldownTime => _dashCooldownTime;
  public bool DoubleJumpUnlocked => _doubleJumpUnlocked;
  public bool WallJumpUnlocked => _wallJumpUnlocked;
  #endregion

  private void Awake()
  {
    Rigidbody = GetComponent<Rigidbody2D>();
    Animator = GetComponent<Animator>();
    _playerInputManager = GetComponent<PlayerInputManager>();
  }

  private void Start()
  {
    _isFacingRight = transform.rotation.y == 0f;
    _normalGravity = Rigidbody.gravityScale;
    // ENSURE CAN DASH FROM START OF SCENE
    ResetDashCooldown();
  }

  private void Update()
  {
    GatherInput();
    HandleJumpBuffer();
    CountTimers();
    FlipSprite();
  }

  private void FixedUpdate()
  {
    CheckCollisions();
  }

  private void GatherInput()
  {
    FrameInput = _playerInputManager.FrameInput;
  }

  #region Timers
  private void CountTimers()
  {
    // JUMP BUFFER
    _jumpBufferTimer -= Time.deltaTime;

    // COYOTE TIMER
    if (_isOnGround || _isOnWall || _isOnClimbable) { _coyoteTimer = _coyoteTime; }
    else { _coyoteTimer -= Time.deltaTime; }

    // DASH COOLDOWN
    if (_dashUnlocked) { _dashCooldownTimer -= Time.deltaTime; }

    // CLIMB COOLDOWN
    _climbCooldownTimer -= Time.deltaTime;
  }
  #endregion

  #region Jumping
  public bool CanJump()
  {
    // PREVENT HUGE JUMPS FROM SPAMMING (COOLDOWN)
    return Time.time - _lastJumpTime > 0.1f;
  }

  public void UseJump()
  {
    _jumpsUsed++;
    ResetJumpBuffer();
    _lastJumpTime = Time.time;
  }

  public bool CanPerformAirJump()
  {
    // CAN AIR JUMP IF DOUBLE JUMP IS UNLOCKED AND HAVEN'T USED MAX JUMPS
    return _doubleJumpUnlocked && _jumpsUsed < MAX_JUMPS && CanJump();
  }

  private void HandleJumpBuffer()
  {
    // SET BUFFER TIMER WHEN JUMP IS PRESSED
    if (FrameInput.Jump) { _jumpBufferTimer = _jumpBufferTime; }
  }

  public void SetHasWallJumpMomentum(bool value)
  {
    _hasWallJumpMomentum = value;
  }

  public void ResetJumpBuffer()
  {
    _jumpBufferTimer = 0f;
    _lastJumpTime = Time.time;
  }

  public void ResetJumpCount()
  {
    _jumpsUsed = 0;
  }
  #endregion

  #region Dashing
  public bool CanDash()
  {
    return _dashUnlocked && _dashCooldownTimer <= 0f;
  }

  public void ResetDashCooldown()
  {
    _dashCooldownTimer = _dashCooldownTime;
  }
  #endregion

  #region Climbing
  public bool CanEnterClimbState()
  {
    return _climbCooldownTimer <= 0f;
  }

  public void SetClimbCooldown()
  {
    _climbCooldownTimer = _climbReEntryCooldown;
  }
  #endregion

  #region Manipulation
  private void FlipSprite()
  {
    if ((_isFacingRight && FrameInput.Move < 0f) ||
        !_isFacingRight && FrameInput.Move > 0f)
    {
      transform.Rotate(0f, 180f, 0f);
      _isFacingRight = !_isFacingRight;
    }
  }

  public void ForceFlipSprite()
  {
    transform.Rotate(0f, 180f, 0f);
    _isFacingRight = !_isFacingRight;
  }

  public void ClampToClimbable()
  {
    StartCoroutine(MovePlayerOverTime(transform.position, new Vector3(_climbPositionX, transform.position.y, transform.position.z), _clampToClimbableDuration));
  }

  IEnumerator MovePlayerOverTime(Vector3 start, Vector3 end, float duration)
  {
    float elapsed = 0f;
    while (elapsed < duration)
    {
      float t = elapsed / duration;
      transform.position = Vector3.Lerp(start, end, t);

      elapsed += Time.deltaTime;
      yield return null;
    }
    transform.position = end;
  }
  #endregion

  #region Helpers
  public AnimationClip GetAnimationClipByName(string clipName)
  {
    AnimationClip[] allClips = Animator.runtimeAnimatorController.animationClips;

    foreach (AnimationClip clip in allClips)
    {
      if (clip.name == clipName) { return clip; }
    }
    return null;
  }
  #endregion

  #region Collision Checks
  private void CheckIfOnGround()
  {
    RaycastHit2D groundHitLeftInfo = Physics2D.Raycast(_groundRayLeft.position, Vector2.down, _groundRayLength, _groundLayer);
    RaycastHit2D groundHitRightInfo = Physics2D.Raycast(_groundRayRight.position, Vector2.down, _groundRayLength, _groundLayer);

    #region Debug Draw Rays
    if (_showGroundRays)
    {
      Debug.DrawRay(_groundRayLeft.position, new Vector3(0f, -_groundRayLength, 0f), Color.green);
      Debug.DrawRay(_groundRayRight.position, new Vector3(0f, -_groundRayLength, 0f), Color.green);
    }
    #endregion

    if (groundHitLeftInfo || groundHitRightInfo)
    {
      _isOnGround = true;
    }
    else { _isOnGround = false; }
  }

  private void CheckIfOnWall()
  {
    RaycastHit2D wallHitTopInfo;
    RaycastHit2D wallHitBottomInfo;

    if (_isFacingRight)
    {
      wallHitTopInfo = Physics2D.Raycast(_wallRayTop.position, Vector2.right, _wallRayLength, _wallLayer);
      wallHitBottomInfo = Physics2D.Raycast(_wallRayBottom.position, Vector2.right, _wallRayLength, _wallLayer);
    }
    else
    {
      wallHitTopInfo = Physics2D.Raycast(_wallRayTop.position, Vector2.left, _wallRayLength, _wallLayer);
      wallHitBottomInfo = Physics2D.Raycast(_wallRayBottom.position, Vector2.left, _wallRayLength, _wallLayer);
    }

    #region Debug Draw Rays
    if (_showWallRays)
    {
      if (_isFacingRight)
      {
        Debug.DrawRay(_wallRayTop.position, new Vector3(_wallRayLength, 0f, 0f), Color.red);
        Debug.DrawRay(_wallRayBottom.position, new Vector3(_wallRayLength, 0f, 0f), Color.red);
      }
      else
      {
        Debug.DrawRay(_wallRayTop.position, new Vector3(-_wallRayLength, 0f, 0f), Color.red);
        Debug.DrawRay(_wallRayBottom.position, new Vector3(-_wallRayLength, 0f, 0f), Color.red);
      }
    }
    #endregion

    if (wallHitTopInfo || wallHitBottomInfo)
    {
      _isOnWall = true;
    }
    else { _isOnWall = false; }
  }

  private void CheckCollisions()
  {
    CheckIfOnGround();
    CheckIfOnWall();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Climbable"))
    {
      _climbPositionX = other.bounds.center.x;
      _isOnClimbable = true;
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.CompareTag("Climbable"))
    {
      _isOnClimbable = false;
    }
  }
  #endregion
}
