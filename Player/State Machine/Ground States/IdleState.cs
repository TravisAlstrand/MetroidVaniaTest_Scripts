public class IdleState : PlayerBaseState
{
  public IdleState(PlayerStateMachine stateMachine, PlayerManager player)
    : base(stateMachine, player)
  {
  }

  private readonly string _animationName = "Idle";

  public override void EnterState()
  {
    // STOP HORIZONTAL MOVEMENT
    _player.Rigidbody.linearVelocityX = 0f;
    // PLAY IDLE ANIMATION
    _player.Animator.Play(_animationName);
  }

  public override void UpdateState()
  {
    // SWITCH TO DASH STATE
    if (_player.CanDash() && _player.FrameInput.Dash) { _stateMachine.SwitchState(_stateMachine.DashState); return; }

    if (_player.IsOnGround)
    {
      // NOTE: MAY NEED TO ADJUST ALL THESE MOVE != OR == 0F THROUGHOUT.
      //       TEST WITH GAMEPAD JOYSTICK SLOWLY. DEAD ZONE AT 0.2F IF SO
      // SWITCH TO MOVE STATE
      if (_player.FrameInput.Move != 0f) { _stateMachine.SwitchState(_stateMachine.MoveState); return; }
      // SWITCH TO JUMP STATE
      if (_player.FrameInput.Jump && _player.CanJump()) { _stateMachine.SwitchState(_stateMachine.JumpState); return; }
      // SWITCH TO CLIMB STATE
      if (_player.IsOnClimbable && _player.CanEnterClimbState() && _player.FrameInput.Climb > 0.2f) { _stateMachine.SwitchState(_stateMachine.ClimbState); return; }
    }
    // SWITCH TO FALL STATE
    else { _stateMachine.SwitchState(_stateMachine.FallState); return; }
  }

  public override void FixedUpdateState()
  {

  }

  public override void ExitState()
  {

  }
}
