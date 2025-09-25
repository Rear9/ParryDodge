using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
public class Actions : MonoBehaviour
{
    [SerializeField] private Movement movement;
    
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

    [Header("Layers")]
    private int _playerLayer;
    private int _playerInvulnLayer;
    private int _enemyAttackLayer;
    
    private bool _parrying;
    private bool _dodging;
    private bool _dodgeCdActive = false;
    private bool _parryHit;
    private Collider2D _parriedAttack; // for debugging
    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _playerLayer =  LayerMask.NameToLayer("Player");
        _playerInvulnLayer =  LayerMask.NameToLayer("PlayerInvuln");
        _enemyAttackLayer = LayerMask.NameToLayer("EnemyAttack");
    }
    private IEnumerator SpriteColor(Color a, Color b) // Renewable coroutine to change player color (push into a new visuals script)
    {
        float time = 0;
        float duration = .2f;
        _sr.color = a;
        while (time < duration)
        {
            time += Time.deltaTime;
            _sr.color = Color.Lerp(a, b, time / duration);
            yield return null;
        }
    }

    public void StartParry()
    {
        if (!_parrying) StartCoroutine(ParryRoutine());
    }
    
    public void StartDodge()
    {
        if (_dodging || _dodgeCdActive) return;
        _dodgeCoroutine = StartCoroutine(DodgeRoutine());
    }
    
    public void CancelDodge()
    {
        if (_dodging) _dodging = false;
    }
    
    private IEnumerator ParryRoutine() // Player Parry Input
    {
        _parrying = true; // Prevent multiple parries activating at once
        _parryHit = false;
        
        StartCoroutine(SpriteColor(_sr.color, parryColor)); // Lerp player sprite colour  (TO PUSH OUT)
        
        float timer = 0f; // Start timer
        while (timer < parryWindow && !_parryHit)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (_parryHit)
        {
            Debug.Log("Parried " + _parriedAttack.name);
            // parry logic + visuals
        }
        
        StartCoroutine(SpriteColor(_sr.color, neutralColor)); // Return player sprite colour to original  (TO PUSH OUT)
        yield return new WaitForSeconds(parryFailCd); // Counter parry spam
        _parrying = false;
    }
    private IEnumerator DodgeRoutine() // Player Dodge Input
    {
        _dodging = true; // Prevent multiple dodges activating at once, start of player invulnerability
        gameObject.layer = _playerInvulnLayer;
        StartCoroutine(SpriteColor(_sr.color, dodgeColor)); // Lerp player sprite colour  (TO PUSH OUT)
        
        var originalSpeed = movement.speed;
        movement.speed += movement.speed * dodgeSpeedMult; // Increase player speed on dodge

        float time = 0f;
        while (time < dodgeWindow && _dodging)
        {
            time += Time.deltaTime;
            yield return null;
        }
        
        movement.speed = originalSpeed; // Decrease player speed once outside of dodge window
        StartCoroutine(SpriteColor(_sr.color, neutralColor)); // Return player sprite colour to original  (TO PUSH OUT)
        _dodging = false; // end of player invulnerability
        gameObject.layer = _playerLayer;
        
        _dodgeCdActive = true; // start dodge cooldown
        yield return new WaitForSeconds(dodgeCd);
        _dodgeCdActive = false;
        _dodgeCoroutine = null; // reset coroutine and stop dodge cooldown
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_parrying && other.gameObject.layer == _enemyAttackLayer)
        {
            _parryHit = true;
            _parriedAttack = other;
            // parry success logic
        }
    }
}
