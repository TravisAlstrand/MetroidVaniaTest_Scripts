using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
public class PlayerStateMachine : MonoBehaviour
{
  public PlayerBaseState PreviousState;

  private PlayerManager _player;
  private PlayerBaseState _currentState;

  // GROUND STATES
  public IdleState IdleState { get; private set; }
  public MoveState MoveState { get; private set; }
  public LandingState LandingState { get; private set; }

  // AIR STATES
  public JumpState JumpState { get; private set; }
  public FallState FallState { get; private set; }

  // WALL STATES
  public WallSlideState WallSlideState { get; private set; }
  public WallJumpState WallJumpState { get; private set; }

  // CLIMB STATES
  public ClimbState ClimbState { get; private set; }

  // SPECIAL STATES
  public DashState DashState { get; private set; }

  private void Awake()
  {
    _player = GetComponent<PlayerManager>();

    // INITIALIZE STATES
    IdleState = new IdleState(this, _player);
    MoveState = new MoveState(this, _player);
    JumpState = new JumpState(this, _player);
    FallState = new FallState(this, _player);
    LandingState = new LandingState(this, _player);
    WallSlideState = new WallSlideState(this, _player);
    WallJumpState = new WallJumpState(this, _player);
    ClimbState = new ClimbState(this, _player);
    DashState = new DashState(this, _player);
  }

  private void Start()
  {
    SwitchState(IdleState);
  }

  private void Update()
  {
    _currentState?.UpdateState();
  }

  private void FixedUpdate()
  {
    _currentState?.FixedUpdateState();
  }

  public void SwitchState(PlayerBaseState nextState)
  {
    _currentState?.ExitState();
    PreviousState = _currentState;
    _currentState = nextState;
    _currentState.EnterState();
    // Debug.Log($"Entering {_currentState} from {PreviousState}");
  }
}
