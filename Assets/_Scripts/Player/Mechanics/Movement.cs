using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody2D _rb; // Variable for player rigidbody
    [SerializeField] public float speed = 5f; // Variable for movement speed (configurable)
    
    private Vector2 _moveInput; // To store magnitudes for 2D movement

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>(); // Get the player's rigidbody component and assign it to _rb
    }
    
    public void SetMoveInput(Vector2 input) => _moveInput = input; // Expose this function for movement inputs in InputHandler
    
    private void FixedUpdate() // Use FixedUpdate for physics
    {
        var movement = _moveInput.normalized * speed; // Set movement variable to a magnitude Vector2 and multiply by movement speed
        _rb.linearVelocity = movement; // Set rigidbody's velocity
    }
}