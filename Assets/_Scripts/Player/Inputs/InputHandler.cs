using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private PlayerInputCode _controls;
    private Movement _movement;
    private Actions _actions;

    private void Awake()
    {
        _controls = new PlayerInputCode();
        _movement = GetComponent<Movement>();
        _actions = GetComponent<Actions>();
    }

    private void OnEnable()  => _controls.Enable();
    private void OnDisable() => _controls.Disable();

    private void Start()
    {
        // Movement
        _controls.Player.Move.performed += ctx => _movement.SetMoveInput(ctx.ReadValue<Vector2>()); // Read exposed function to move player once input starts
        _controls.Player.Move.canceled += ctx => _movement.SetMoveInput(Vector2.zero); // Stop player once input ends
        
        // Parry & Dodge
        _controls.Player.Parry.performed += ctx => _actions.StartParry(); //
        _controls.Player.Dodge.performed += ctx => _actions.StartDodge(); // Linked to functions in Actions script
        _controls.Player.Dodge.canceled += ctx => _actions.CancelDodge(); //
    }
}
