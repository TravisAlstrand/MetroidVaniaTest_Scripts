using UnityEngine;

public class WallJumpState : PlayerBaseState
{
  public WallJumpState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private readonly string _animationName = "Jump";
  private float _wallJumpTimer;
  private float _wallJumpDirection;

  public override void EnterState()
  {
    _wallJumpTimer = _player.WallJumpTime;
    // DETERMINE DIRECTION
    _wallJumpDirection = _player.IsFacingRight ? -1f : 1f;
    // APPLY FORCE
    _player.Rigidbody.AddForce(new Vector2(_wallJumpDirection * _player.WallJumpForce.x, _player.WallJumpForce.y), ForceMode2D.Impulse);
    _player.ForceFlipSprite();
    // PLAY JUMP ANIMATION
    _player.Animator.Play(_animationName);
    // CONSUME JUMP
    _player.UseJump();
    // FLAG WALL JUMP
    _player.SetHasWallJumpMomentum(true);
  }

  public override void UpdateState()
  {
    _wallJumpTimer -= Time.deltaTime;

    // SWITCH TO DASH STATE
    if (_player.CanDash() && _player.FrameInput.Dash) { _stateMachine.SwitchState(_stateMachine.DashState); return; }

    // SWITCH TO FALL STATE
    if (_wallJumpTimer <= 0f) { _stateMachine.SwitchState(_stateMachine.FallState); return; }

    // SWITCH TO CLIMB STATE
    if (_player.IsOnClimbable && _player.CanEnterClimbState() && (_player.FrameInput.Climb > 0.2f || _player.FrameInput.Climb < -0.2f)) { _stateMachine.SwitchState(_stateMachine.ClimbState); return; }
  }

  public override void FixedUpdateState()
  {

  }

  public override void ExitState()
  {

  }
}
