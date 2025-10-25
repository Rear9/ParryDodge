using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private PlayerInputCode _controls;
    private Movement _movement;
    private Actions _actions;
    private bool _allowMovement = true;
    private bool _allowParry = true;
    private bool _allowDodge = true;
    
    public event Action OnParryPressed;
    public event Action OnDodgePressed;
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
        _controls.Player.Move.performed += ctx => {
            if (_allowMovement) _movement.SetMoveInput(ctx.ReadValue<Vector2>()); // Read exposed function to move player once input starts
        }; 
        _controls.Player.Move.canceled += ctx =>
        {
            if (_allowMovement) _movement.SetMoveInput(Vector2.zero); // Stop player once input ends
        }; 
        
        // Parry & Dodge
        _controls.Player.Parry.performed += ctx =>
        {
            if (_allowParry)
            {
                _actions.StartParry();
                OnParryPressed?.Invoke();
                if (StatsManager.Instance != null)
                {
                    StatsManager.Instance.RecordParry();
                }
            }
        };
        _controls.Player.Dodge.performed += ctx =>
        {
            if (_allowDodge)
            {
                _actions.StartDodge();
                OnDodgePressed?.Invoke();
                if (StatsManager.Instance != null)
                {
                    StatsManager.Instance.RecordDodge();
                }
            }
        };
        _controls.Player.Dodge.canceled += ctx =>
        {
            _actions.CancelDodge();
        }; 
    }

    public void SetTutorialMode(bool enable, int stage)
    {
        _allowMovement = !enable;
        _allowParry = !enable;
        _allowDodge = !enable;
        if (stage == 1)
        {
            _allowDodge = !enable;
            _allowParry = enable;
        }
        else if (stage == 2)
        {
            _allowParry = !enable;
            _allowDodge = enable;
        }
    }
    
}
