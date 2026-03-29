public class MoveState : PlayerBaseState
{
  public MoveState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private readonly string _animationName = "Move";

  public override void EnterState()
  {
    // PLAY MOVING ANIMATION
    _player.Animator.Play(_animationName);
  }

  public override void UpdateState()
  {
    // SWITCH TO DASH STATE
    if (_player.CanDash() && _player.FrameInput.Dash) { _stateMachine.SwitchState(_stateMachine.DashState); return; }

    if (_player.IsOnGround)
    {
      // SWITCH TO IDLE STATE
      if (_player.FrameInput.Move == 0f) { _stateMachine.SwitchState(_stateMachine.IdleState); return; }
      // SWITCH TO JUMP STATE
      if (_player.FrameInput.Jump) { _stateMachine.SwitchState(_stateMachine.JumpState); return; }
      // SWITCH TO CLIMB STATE
      if (_player.IsOnClimbable && _player.CanEnterClimbState() && _player.FrameInput.Climb > 0.2f) { _stateMachine.SwitchState(_stateMachine.ClimbState); return; }
    }
    // SWITCH TO FALL STATE
    else { _stateMachine.SwitchState(_stateMachine.FallState); return; }
  }

  public override void FixedUpdateState()
  {
    // HORIZONTAL MOVEMENT
    _player.Rigidbody.linearVelocityX = _player.MoveSpeed * _player.FrameInput.Move;
  }

  public override void ExitState()
  {

  }
}
