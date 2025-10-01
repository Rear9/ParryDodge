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

    [Header("Layers")]
    private int _playerLayer;
    private int _playerInvulnLayer;
    private int _enemyAttackLayer;
    
    [Header("Debugging")]
    [SerializeField] private bool parrying;
    [SerializeField] private bool parryCdActive = false;
    [SerializeField] private bool dodging;
    [SerializeField] private bool dodgeCdActive = false;
    private bool _parryHit;
    private Collider2D _collidedAttack; // for debugging
    void Awake()
    {
        _playerLayer =  LayerMask.NameToLayer("Player");
        _playerInvulnLayer =  LayerMask.NameToLayer("PlayerInvuln");
        _enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");
    }
    public void StartParry()
    {
        if (!parrying && !parryCdActive) StartCoroutine(ParryRoutine());
    }
    
    public void StartDodge()
    {
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
        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        parryCdActive = true;
        yield return new WaitForSeconds(parryFailCd); // Counter parry spam
        parryCdActive = false;
    }
    private IEnumerator DodgeRoutine() // Player Dodge Input
    {
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
        StartCoroutine(plrColor.ColorSprite(plrColor.neutralColor));
        dodging = false; // end of player invulnerability
        gameObject.layer = _playerLayer;
        
        dodgeCdActive = true; // start dodge cooldown
        yield return new WaitForSeconds(dodgeCd);
        dodgeCdActive = false;
        _dodgeCoroutine = null; // reset coroutine and stop dodge cooldown
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != _enemyAttackLayer) return; // Check if collided with attack
        _collidedAttack = other;
        if (parrying)
        {
            _parryHit = true;
            Debug.Log("Parried " + other.name);
        }

        if (dodging)
        {
            Debug.Log("Dodged " + other.name);
        }
        if (!parrying && !dodging && !_parryHit)
        {
            Debug.Log("Player hit by " + other.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_parryHit || other.gameObject.layer != _enemyAttackLayer) return;
        parrying = false;
        parryCdActive = false;
    }
}
