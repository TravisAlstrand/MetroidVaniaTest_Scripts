using UnityEngine;

public class ClimbState : PlayerBaseState
{
  public ClimbState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private readonly string _climbingAnimationName = "Climb";
  private readonly string _climbIdleAnimationName = "ClimbIdle";
  private float _minClimbTimer;
  private bool _isClimbing;

  public override void EnterState()
  {
    _player.Rigidbody.linearVelocityX = 0f;
    _player.Rigidbody.gravityScale = 0f;
    _player.ClampToClimbable();
    _player.Animator.Play(_climbIdleAnimationName);
    _minClimbTimer = _player.MinimumClimbTime;
    _isClimbing = true;
    _player.ResetJumpCount();
  }

  public override void UpdateState()
  {
    if (_player.IsOnClimbable)
    {
      _minClimbTimer -= Time.deltaTime;

      // SELECT ANIMATION
      if (_player.Rigidbody.linearVelocityY != 0f) { _player.Animator.Play(_climbingAnimationName); }
      else { _player.Animator.Play(_climbIdleAnimationName); }
      // SWITCH TO IDLE STATE
      // ADDED INPUT CHECK TO AVOID GLITCHY TAKE-OFF
      if (_player.IsOnGround && _player.FrameInput.Climb <= 0f) { EnterIdleState(); return; }

      // JUMP OFF CLIMBABLE
      if (_minClimbTimer <= 0f && _player.JumpBufferTimer > 0f && _player.CanJump())
      {
        _isClimbing = false;

        // RE-ENABLE GRAVITY IMMEDIATELY
        _player.Rigidbody.gravityScale = _player.NormalGravity;

        // SET COOLDOWN TO PREVENT IMMEDIATE RE-ENTRY
        _player.SetClimbCooldown();

        // APPLY JUMP FORCE
        _player.Rigidbody.linearVelocity = new Vector2(_player.Rigidbody.linearVelocityX, _player.JumpForce);
        _player.UseJump();

        // SWITCH TO JUMP STATE
        EnterJumpState(); return;
      }

      // MOVE OFF CLIMBABLE
      if (_minClimbTimer <= 0f && _player.FrameInput.Move != 0f)
      {
        _isClimbing = false;

        // RE-ENABLE GRAVITY IMMEDIATELY
        _player.Rigidbody.gravityScale = _player.NormalGravity;

        // SET COOLDOWN TO PREVENT IMMEDIATE RE-ENTRY
        _player.SetClimbCooldown();

        // SET DIRECTION TO EXIT LADDER
        float xDirection = _player.FrameInput.Move > 0f ? 1f : -1f;

        // CLEAR VERTICAL VELOCITY AND SET NEW VELOCITY DIRECTLY
        _player.Rigidbody.linearVelocity = new Vector2(xDirection * _player.MoveOffClimbableForce.x, _player.MoveOffClimbableForce.y);

        // SWITCH TO FALL STATE
        EnterFallState(); return;
      }
    }
    // IF NOT ON CLIMBABLE ANYMORE
    else
    {
      // ENTER IDLE STATE
      if (_player.IsOnGround) { EnterIdleState(); return; }
      // ENTER FALL STATE
      else { EnterFallState(); return; }
    }
  }

  public override void FixedUpdateState()
  {
    // VERTICAL MOVEMENT
    if (_isClimbing) { _player.Rigidbody.linearVelocityY = _player.FrameInput.Climb * _player.ClimbSpeed; }
  }

  public override void ExitState()
  {
    _isClimbing = false;
    _player.Rigidbody.gravityScale = _player.NormalGravity;
  }

  private void EnterIdleState()
  {
    _stateMachine.SwitchState(_stateMachine.IdleState);
  }

  private void EnterFallState()
  {
    _stateMachine.SwitchState(_stateMachine.FallState);
  }

  private void EnterJumpState()
  {
    _stateMachine.SwitchState(_stateMachine.JumpState);
  }
}
