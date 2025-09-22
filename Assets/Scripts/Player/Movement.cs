using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D _rb; // Variable for player rigidbody
    public float speed = 5f; // Variable for movement speed (configurable)

    // Input
    private PlayerControls _controls; // Class for player inputs
    private Vector2 _moveInput; // To store magnitudes for 2D movement

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>(); // Get the player's rigidbody component and assign it to _rb
        _controls = new PlayerControls(); // Initialize player input class
        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>(); // Subscribe anon function to event once Move action triggered, then read return values
        _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero; // Same as above but setting return value to Vector2.zero as the player isn't moving
    }

    private void OnEnable() // Once player is active, enable controls
    {
        _controls.Enable();
    }

    private void OnDisable() // Reverse ^
    {
        _controls.Disable();
    }

    private void FixedUpdate() // Use FixedUpdate for physics
    {
        var movement = _moveInput.normalized * speed; // Set movement variable to a magnitude Vector2 and multiply by movement speed
        _rb.linearVelocity = movement; // Set rigidbody's velocity
    }
}