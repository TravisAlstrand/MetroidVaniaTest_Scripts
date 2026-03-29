using UnityEngine;

public class DashState : PlayerBaseState
{
  public DashState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private float _dashTimer;

  public override void EnterState()
  {
    _dashTimer = _player.MaxDashDuration;
    _player.Rigidbody.gravityScale = 0f;
  }

  public override void UpdateState()
  {
    _dashTimer -= Time.deltaTime;

    if (_dashTimer < 0f)
    {
      if (_player.IsOnGround)
      {
        // SWITCH TO MOVE STATE
        if (_player.FrameInput.Move != 0f) { _stateMachine.SwitchState(_stateMachine.MoveState); return; }
        // SWITCH TO IDLE STATE
        else { _stateMachine.SwitchState(_stateMachine.IdleState); return; }
      }
      // SWITCH TO FALLING STATE
      else { _stateMachine.SwitchState(_stateMachine.FallState); return; }
    }
    // IF PLAYER HITS WALL BEFORE DASH IS OVER
    else
    {
      if (_player.IsOnWall)
      {
        // SWITCH TO IDLE STATE
        if (_player.IsOnGround) { _stateMachine.SwitchState(_stateMachine.IdleState); return; }
        // SWITCH TO WALL SLIDE STATE
        else { _stateMachine.SwitchState(_stateMachine.WallSlideState); return; }
      }
    }
  }

  public override void FixedUpdateState()
  {
    if (_dashTimer >= 0f)
    {
      if (_player.IsFacingRight) { _player.Rigidbody.linearVelocityX = _player.DashForce; }
      else { _player.Rigidbody.linearVelocityX = -_player.DashForce; }
    }
  }

  public override void ExitState()
  {
    _player.Rigidbody.gravityScale = _player.NormalGravity;
    _player.ResetDashCooldown();
  }
}
