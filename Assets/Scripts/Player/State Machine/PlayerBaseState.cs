public class PlayerBaseState
{
  protected PlayerStateMachine _stateMachine;
  protected PlayerManager _player;

  public PlayerBaseState(PlayerStateMachine stateMachine, PlayerManager player)
  {
    _stateMachine = stateMachine;
    _player = player;
  }

  public virtual void EnterState() { }
  public virtual void UpdateState() { }
  public virtual void FixedUpdateState() { }
  public virtual void ExitState() { }
}
