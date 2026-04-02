using UnityEngine;

public class MeleeState : PlayerBaseState
{
  public MeleeState(PlayerStateMachine stateMachine, PlayerManager player)
  : base(stateMachine, player)
  {
  }

  private float _testTimer;

  public override void EnterState()
  {
    // STOP RESIDUAL MOVEMENT
    _player.Rigidbody.linearVelocity = Vector2.zero;
    // GET DIRECTION & ACTIVATE HIT BOX
    MeleeDirection _attackDirection = _player.GetMeleeDirection();
    _player.ActivateMeleeHitBox(_attackDirection);
    _testTimer = 1f;
    // TODO: PLAY ANIMATION, INSTANTIATE PARTICLES/SPRITES
    _player.Animator.Play("Idle");
  }

  public override void UpdateState()
  {
    _testTimer -= Time.deltaTime;
    if (_testTimer < 0f) { _stateMachine.SwitchState(_stateMachine.IdleState); }
  }

  public override void FixedUpdateState()
  {

  }

  public override void ExitState()
  {
    _player.DeactivateMeleeHitBoxes();
  }
}