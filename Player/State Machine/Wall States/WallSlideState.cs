using UnityEngine;

public class WallSlideState : PlayerBaseState
{
  public WallSlideState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private float _wallStickTimer;

  public override void EnterState()
  {
    _player.Rigidbody.linearVelocityY = 0f;
    _wallStickTimer = _player.WallStickTime;

    // RESET JUMP COUNT (grabbing wall resets jump state)
    _player.ResetJumpCount();

    // ENSURE WALL JUMP FLAG IS CLEARED
    _player.SetHasWallJumpMomentum(false);
  }

  public override void UpdateState()
  {
    _wallStickTimer -= Time.deltaTime;

    // SWITCH TO IDLE STATE
    if (_player.IsOnGround) { _stateMachine.SwitchState(_stateMachine.IdleState); return; }
    // SWITCH TO FALL STATE
    if (!_player.IsOnWall) { _stateMachine.SwitchState(_stateMachine.FallState); return; }
    // SWITCH TO WALL JUMP STATE
    if (_player.FrameInput.Jump && _player.WallJumpUnlocked) { _stateMachine.SwitchState(_stateMachine.WallJumpState); return; }
  }

  public override void FixedUpdateState()
  {
    // MOMENTARY STICK BEFORE SLIDING
    if (_wallStickTimer > 0f) { _player.Rigidbody.gravityScale = 0f; return; }
    else { _player.Rigidbody.gravityScale = _player.NormalGravity; }

    _player.Rigidbody.linearVelocityY = -_player.WallSlideSpeed;
  }

  public override void ExitState()
  {
    _player.Rigidbody.gravityScale = _player.NormalGravity;
  }
}
