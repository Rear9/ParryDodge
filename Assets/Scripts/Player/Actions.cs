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

    private Coroutine _dodgeCoroutine;

    private void OnEnable() // Once player is active, enable controls
    {
        _controls.Enable();
    }

    private void OnDisable() // Reverse ^
    {
        _controls.Disable();
    }

    void Awake() // Initialize player inputs and subscribe actions to methods/respective coroutines
    {
        _rb = GetComponent<Rigidbody2D>();
        _controls = new PlayerControls();
        _controls.Player.Parry.performed += ctx => StartCoroutine(Parry());
        _controls.Player.Dodge.performed += ctx => StartCoroutine(Dodge());

        _controls.Player.Dodge.canceled += ctx => StartCoroutine(DodgeCancel());
    }

    private bool CheckParrySuccess()
    {
        // if parrying & hit collider => return true
        return false;
    }

    private IEnumerator Dodge() // Player Dodge Input
    {
        if (_dodging) { yield return null; } 
        _dodging = true; // Prevent multiple dodges activating at once
        //logic
        yield return new WaitForSeconds(dodgeWindow);
        _dodging = false;
        _dodgeCoroutine = null;
        Debug.Log("Dodge");
    }

    private IEnumerator Parry() // Player Parry Input
    {
        if (_parrying) { yield return null; }
        _parrying = true; // Prevent multiple parries activating at once
        bool parrySuccess = false;
        float timer = 0f; // Start timer
        while (timer < parryWindow && !parrySuccess)
        {
            timer += Time.deltaTime;
            if (CheckParrySuccess())
            {
                parrySuccess = true;
            }
            yield return null;
        }
        _parrying = false;
        if (parrySuccess) {
            Debug.Log("Parry");
        }
    }

    private IEnumerator DodgeCancel()
    {
        if (_dodging && _dodgeCoroutine != null)
        {
            StopCoroutine(Dodge());
            //cleanup
            Debug.Log("DodgeCancel");
        }
        yield break;
    }
}
