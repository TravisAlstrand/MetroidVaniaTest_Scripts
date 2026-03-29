using UnityEngine;

public class JumpState : PlayerBaseState
{
  public JumpState(PlayerStateMachine stateMachine, PlayerManager player)
    : base(stateMachine, player)
  {
  }

  private readonly string _animationName = "Jump";
  private bool _startedJumpTouchingWall = false;

  public override void EnterState()
  {
    // PREVENT VERTICAL STACKING FROM SPAMMING
    _player.Rigidbody.linearVelocityY = 0f;
    // APPLY JUMP FORCE
    _player.Rigidbody.AddForceY(_player.JumpForce, ForceMode2D.Impulse);
    // PLAY JUMP ANIMATION
    _player.Animator.Play(_animationName);
    // CONSUME JUMP
    _player.UseJump();

    // CHECK IF TOUCHING WALL AT START TO PREVENT AUTO WALL SLIDE
    if (_player.IsOnWall) { _startedJumpTouchingWall = true; }
  }

  public override void UpdateState()
  {
    // SWITCH TO DASH STATE
    if (_player.CanDash() && _player.FrameInput.Dash) { _stateMachine.SwitchState(_stateMachine.DashState); return; }

    // VARIABLE JUMP HEIGHT ON BUTTON RELEASE
    if (_player.Rigidbody.linearVelocityY > 0f && !_player.FrameInput.JumpHeld) { _stateMachine.SwitchState(_stateMachine.FallState); return; }

    // SWITCH TO FALL STATE
    if (_player.Rigidbody.linearVelocityY < 0f) { _stateMachine.SwitchState(_stateMachine.FallState); return; }

    // HANDLE WALL INTERACTION
    if (_player.IsOnWall)
    {
      if (_startedJumpTouchingWall)
      {
        // DON'T AUTO STICK TO WALL IF TOUCHING WHEN JUMP STARTED
        if (_player.Rigidbody.linearVelocityY > 0f) { return; }
      }
      else
      {
        // SWITCH TO WALL SLIDE STATE
        _stateMachine.SwitchState(_stateMachine.WallSlideState);
        return;
      }
    }

    // SWITCH TO CLIMB STATE
    if (_player.IsOnClimbable && _player.CanEnterClimbState() && (_player.FrameInput.Climb > 0.2f || _player.FrameInput.Climb < -0.2f)) { _stateMachine.SwitchState(_stateMachine.ClimbState); return; }
  }

  public override void FixedUpdateState()
  {
    // CONTINUE HORIZONTAL MOVEMENT FROM WALL JUMP
    if (_player.HasWallJumpMomentum && _player.FrameInput.Move == 0f) return;
    // ALLOW HORIZONTAL CONTROL
    _player.Rigidbody.linearVelocityX = _player.FrameInput.Move * _player.MoveSpeed;
  }

  public override void ExitState()
  {
    _startedJumpTouchingWall = false;
  }
}
