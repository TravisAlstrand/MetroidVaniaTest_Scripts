using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
  public FrameInput FrameInput { get; private set; }

  private PlayerInputActions _playerInputActions;
  private InputAction _move;
  private InputAction _climb;
  private InputAction _jump;
  private InputAction _dash;
  private InputAction _melee;

  private void Awake()
  {
    _playerInputActions = new PlayerInputActions();
    _move = _playerInputActions.Player.Move;
    _climb = _playerInputActions.Player.Climb;
    _jump = _playerInputActions.Player.Jump;
    _dash = _playerInputActions.Player.Dash;
    _melee = _playerInputActions.Player.Melee;
  }

  private void OnEnable()
  {
    _playerInputActions.Enable();
  }

  private void OnDisable()
  {
    _playerInputActions.Disable();
  }

  private void Update()
  {
    FrameInput = GatherInput();
  }

  private FrameInput GatherInput()
  {
    return new FrameInput
    {
      Move = _move.ReadValue<float>(),
      Climb = _climb.ReadValue<float>(),
      Jump = _jump.WasPressedThisFrame(),
      JumpHeld = _jump.inProgress,
      Dash = _dash.WasPressedThisFrame(),
      Melee = _melee.WasPressedThisFrame()
    };
  }

  public void DisablePlayerControls()
  {
    _playerInputActions.Disable();
  }

  public void EnablePlayerCOntrols()
  {
    _playerInputActions.Disable();
  }
}

public struct FrameInput
{
  public float Move;
  public float Climb;
  public bool Jump;
  public bool JumpHeld;
  public bool Dash;
  public bool Melee;
}
