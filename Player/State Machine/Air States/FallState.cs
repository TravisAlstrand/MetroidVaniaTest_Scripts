using UnityEngine;

public class FallState : PlayerBaseState
{
  public FallState(PlayerStateMachine stateMachine, PlayerManager player)
    : base(stateMachine, player)
  {
  }

  private readonly string _fallAnimationName = "Fall";
  private readonly string _jumpAnimationName = "Jump";

  public override void EnterState()
  {

  }

  public override void UpdateState()
  {
    // SELECT ANIMATION
    if (_player.Rigidbody.linearVelocityY >= 0f) { _player.Animator.Play(_jumpAnimationName); }
    else { _player.Animator.Play(_fallAnimationName); }

    // SWITCH TO DASH STATE
    if (_player.CanDash() && _player.FrameInput.Dash) { _stateMachine.SwitchState(_stateMachine.DashState); return; }

    // SWITCH TO LANDING STATE
    if (_player.IsOnGround) { _stateMachine.SwitchState(_stateMachine.LandingState); return; }
    else
    {
      // COYOTE JUMP
      if (_player.CoyoteTimer >= 0f && _player.FrameInput.Jump && _player.CanJump())
      {
        // PREVENT COYOTE WALL JUMP IF WALL JUMP NOT UNLOCKED
        if (_stateMachine.PreviousState == _stateMachine.WallSlideState && !_player.WallJumpUnlocked) return;
        // SET CLIMB COOLDOWN IF JUMPING FROM CLIMBABLE
        if (_stateMachine.PreviousState == _stateMachine.ClimbState) _player.SetClimbCooldown();
        // PERFORM JUMP
        _stateMachine.SwitchState(_stateMachine.JumpState);
        return;
      }
      // DOUBLE JUMP
      else if (_player.JumpsUsed > 0 && _player.FrameInput.Jump && _player.CanPerformAirJump())
      {
        _stateMachine.SwitchState(_stateMachine.JumpState);
        return;
      }
      // SWITCH TO WALL SLIDE STATE
      if (_player.IsOnWall && !_player.IsOnGround) { _stateMachine.SwitchState(_stateMachine.WallSlideState); return; }

      // SWITCH TO CLIMB STATE
      if (_player.IsOnClimbable && _player.CanEnterClimbState() && (_player.FrameInput.Climb > 0.2f || _player.FrameInput.Climb < -0.2f)) { _stateMachine.SwitchState(_stateMachine.ClimbState); return; }
    }
  }

  public override void FixedUpdateState()
  {
    // CLAMP FALL SPEED
    _player.Rigidbody.linearVelocityY = Mathf.Max(_player.Rigidbody.linearVelocityY, -_player.MaxFallSpeed);
    // ADD EXTRA GRAVITY
    _player.Rigidbody.linearVelocityY -= _player.ExtraGravity * Time.fixedDeltaTime;

    // CONTINUE HORIZONTAL MOVEMENT FROM WALL JUMP
    if (_player.HasWallJumpMomentum && _player.FrameInput.Move == 0f) return;
    // ALLOW HORIZONTAL CONTROL
    else { _player.Rigidbody.linearVelocityX = _player.FrameInput.Move * _player.MoveSpeed; }
  }

  public override void ExitState()
  {

  }
}
