using UnityEngine;

public class LandingState : PlayerBaseState
{
  public LandingState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private readonly string _animationName = "Land";
  private float _animationDuration;
  private float _stateTimer;

  public override void EnterState()
  {
    // SINCE JUST TOUCHING GROUND, RESET JUMP COUNT
    _player.ResetJumpCount();

    // ENSURE WALL JUMP FLAG IS CLEARED
    _player.SetHasWallJumpMomentum(false);

    // CHECK IF JUMP WAS BUFFERED & COOLDOWN HAS PASSED
    if (_player.JumpBufferTimer > 0f && _player.IsOnGround && _player.CanJump())
    {
      // SWITCH TO BUFFERED JUMP (BUFFER RESET HAPPENS IN JUMP STATE)
      _stateMachine.SwitchState(_stateMachine.JumpState);
      return;
    }

    // GET DURATION OF LANDING ANIMATION
    AnimationClip clip = _player.GetAnimationClipByName(_animationName);
    _animationDuration = clip.length;
    _stateTimer = _animationDuration;
    // PLAY LANDING ANIMATION
    _player.Animator.Play(_animationName);
  }

  public override void UpdateState()
  {
    _stateTimer -= Time.deltaTime;

    // SWITCH TO DASH STATE
    if (_player.CanDash() && _player.FrameInput.Dash) { _stateMachine.SwitchState(_stateMachine.DashState); return; }

    // SWITCH TO JUMP STATE 
    if (_player.FrameInput.Jump && _player.IsOnGround && _player.CanJump()) { _stateMachine.SwitchState(_stateMachine.JumpState); }

    if (_stateTimer <= 0f)
    {
      // SWITCH TO MOVE STATE
      if (_player.FrameInput.Move != 0f && _player.IsOnGround) { _stateMachine.SwitchState(_stateMachine.MoveState); return; }
      // SWITCH TO IDLE STATE
      else { _stateMachine.SwitchState(_stateMachine.IdleState); return; }
    }
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
