using System.Collections;
using UnityEngine;

public class Actions : MonoBehaviour
{
    private PlayerControls _controls;

    private bool _parrying;
    private bool _dodging;
    [SerializeField]
    public float parryWindow;
    public float dodgeWindow;
    public float dodgeSpeed;

    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _controls = new PlayerControls();
        _controls.Player.Parry.performed += ctx => StartCoroutine(Parry());
        _controls.Player.Dodge.performed += ctx => StartCoroutine(Dodge());
    }

    private void Dodge_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        throw new System.NotImplementedException();
    }

    private void OnEnable() // Once player is active, enable controls
    {
        _controls.Enable();
    }

    private void OnDisable() // Reverse ^
    {
        _controls.Disable();
    }

    private IEnumerator Dodge()
    {
        _dodging = true;
        yield break;
    }

    private IEnumerator Parry()
    {
        _parrying = true;
        yield break;
    }
}
