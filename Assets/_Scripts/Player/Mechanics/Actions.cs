using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
public class Actions : MonoBehaviour
{
    [SerializeField] private Movement movement;
    [SerializeField] private PlayerColor plrColor;
    
    [Header("Parry Settings")]
    public float parryWindow;
    public float parryFailCd;
    
    [Header("Dodge Settings")]
    public float dodgeWindow;
    public float dodgeCd;
    public float dodgeSpeedMult;

    [Header("Colours")] 
    public Color neutralColor = new Color(1, 0, 0);
    public Color parryColor = new Color(1, .65f, 0);
    public Color dodgeColor = new Color(.3f, .6f, 1);

    [Header("Components")]
    private SpriteRenderer _sr;
    private Coroutine _dodgeCoroutine;
    
    private bool _parrying;
    private bool _parryCdActive = false;
    private bool _dodging;
    private bool _dodgeCdActive = false;
    private bool _parryHit;

    private Collider2D _parriedAttack; // for debugging
    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    public void StartParry()
    {
        if (_parrying || _parryCdActive) return;
        gameObject.layer = LayerMask.NameToLayer("PlayerParry");
        StartCoroutine(ParryRoutine());

    }
    public void StartDodge()
    {
        if (_dodging || _dodgeCdActive) return;
        gameObject.layer = LayerMask.NameToLayer("PlayerDodge");
        _dodgeCoroutine = StartCoroutine(DodgeRoutine());

    }
    
    public void CancelDodge()
    {
        if (_dodging) _dodging = false;
    }

    private IEnumerator ParryRoutine() // Player Parry Input
    {
        _parryHit = false;
        _parrying = true;
        
        gameObject.layer = LayerMask.NameToLayer("PlayerParry");
        StartCoroutine(plrColor.ColorSprite(plrColor.parryColor));
        
        float timer = 0f; // Start timer
        while (timer < parryWindow && !_parryHit)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (_parryHit)
        {
            Debug.Log("Parried " + _parriedAttack.name);
            gameObject.layer = LayerMask.NameToLayer("Player");
            _parrying = false;
            //StopAllCoroutines();
            StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
            _parryHit = false;
            gameObject.layer = LayerMask.NameToLayer("Player");
            yield break;
        }
        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        _parrying = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
        _parryCdActive = true;
        yield return new WaitForSeconds(parryFailCd); // Counter parry spam
        _parryCdActive = false;
    }

    public void ParrySuccess()
    {
        _parryHit = true;
    }

    private IEnumerator DodgeRoutine() // Player Dodge Input
    {
        _dodging = true; // Prevent multiple dodges activating at once, start of player invulnerability
        StartCoroutine(plrColor.ColorSprite(plrColor.dodgeColor));
        
        var originalSpeed = movement.speed;
        movement.speed += movement.speed * dodgeSpeedMult; // Increase player speed on dodge

        float time = 0f;
        while (time < dodgeWindow && _dodging)
        {
            time += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        _dodging = false; // end of player invulnerability
        gameObject.layer = LayerMask.NameToLayer("Player");
        _dodgeCdActive = true; // start dodge cooldown
        yield return new WaitForSeconds(dodgeCd);
        _dodgeCdActive = false;
        _dodgeCoroutine = null; // reset coroutine and stop dodge cooldown
    }
}
