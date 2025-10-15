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

    [Header("Components")]
    private Coroutine _dodgeCoroutine;
    
    [Header("Debugging")]
    [SerializeField] private bool parrying;
    [SerializeField] private bool parryCdActive = false;
    [SerializeField] private bool dodging;
    [SerializeField] private bool dodgeCdActive = false;
    private bool _parryHit;
    private Collider2D _parriedAttack; // for debugging
    void Awake()
    {
        _playerLayer =  LayerMask.NameToLayer("Player");
        _playerInvulnLayer =  LayerMask.NameToLayer("PlayerInvuln");
        _enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");
    }
    public void StartParry()
    {
        if (!_parrying) StartCoroutine(ParryRoutine());

        if (!parrying && !parryCdActive) StartCoroutine(ParryRoutine());

    }
    
    public void StartDodge()
    {
        if (_dodging || _dodgeCdActive) return;
        _dodgeCoroutine = StartCoroutine(DodgeRoutine());

        if (!dodging && !dodgeCdActive) _dodgeCoroutine = StartCoroutine(DodgeRoutine());

    }
    public void CancelDodge()
    {
        if (dodging) dodging = false;
    }

    private IEnumerator ParryRoutine() // Player Parry Input
    {
        parrying = true; // Prevent multiple parries activating at once
        _parryHit = false;

        int originLayer = gameObject.layer;
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
            parrying = false;
            //StopAllCoroutines();
            StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
            _parryHit = false;
            yield break;
        }
        
        parrying = false;
        gameObject.layer = originLayer;
        //StopAllCoroutines();
        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        _parryHit = false;
        yield break;

            Debug.Log("Parried " + _parriedAttack.name);
            // parry logic + visuals
        }

        
        
        parrying = false;
        gameObject.layer = originLayer;

            parrying = false;
            //StopAllCoroutines();
            StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
            _parryHit = false;
            gameObject.layer = LayerMask.NameToLayer("Player");
            yield break;
        }
        parrying = false;
        gameObject.layer = LayerMask.NameToLayer("Player");

        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        parryCdActive = true;
        yield return new WaitForSeconds(parryFailCd); // Counter parry spam
        parryCdActive = false;
    }

    public void ParrySuccess()
    {
        _parryHit = true;
    }

    private IEnumerator DodgeRoutine() // Player Dodge Input
    {
        _dodging = true; // Prevent multiple dodges activating at once, start of player invulnerability
        gameObject.layer = _playerInvulnLayer;
        StartCoroutine(SpriteColor(_sr.color, dodgeColor)); // Lerp player sprite colour  (TO PUSH OUT)

        dodging = true; // Prevent multiple dodges activating at once, start of player invulnerability
        gameObject.layer = _playerInvulnLayer;
        StartCoroutine(plrColor.ColorSprite(plrColor.dodgeColor));

        
        var originalSpeed = movement.speed;
        movement.speed += movement.speed * dodgeSpeedMult; // Increase player speed on dodge

        var time = 0f;
        while (time < dodgeWindow && dodging)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        movement.speed = originalSpeed; // Decrease player speed once outside of dodge window
        StartCoroutine(SpriteColor(_sr.color, neutralColor)); // Return player sprite colour to original  (TO PUSH OUT)
        _dodging = false; // end of player invulnerability
        gameObject.layer = _playerLayer;

        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        dodging = false; // end of player invulnerability
        gameObject.layer = LayerMask.NameToLayer("Player");

        
        dodgeCdActive = true; // start dodge cooldown
        yield return new WaitForSeconds(dodgeCd);
        dodgeCdActive = false;
        _dodgeCoroutine = null; // reset coroutine and stop dodge cooldown
    }
}
