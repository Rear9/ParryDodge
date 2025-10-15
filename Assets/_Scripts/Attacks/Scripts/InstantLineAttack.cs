using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class InstantLineAttack : EnemyAttackCore
{
    [Header("Overrides")] 
    [SerializeField] private float chargeTime = -1f;
    [SerializeField] private SpriteRenderer sr;
    
    private Transform _player;
    private bool _moving;
    private float _speed;
    private Vector2 _moveDir;
    private Color _baseColor;
    private readonly Color _pulseColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        _baseColor = sr.color;
    }
    
    public void InitAttack(Transform player)
    {
        _player = player;
        if (_player == null) return;

        StopAllCoroutines();
        _moving = false;
        _active = false;
        sr.color = _baseColor; // Reset visual
        
        // Rotate toward player
        Vector2 dir = (_player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        _moveDir = transform.up;

        // Start behaviour
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        _active = false;
        if (chargeTime > 0)
        {
            float timer = 0f;
            float pulseSpeed = 2f;
            while (timer < chargeTime)
            {
                float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                sr.color = Color.Lerp(_baseColor, _pulseColor, t);
                timer += Time.deltaTime;
                yield return null;
            }

        }

        sr.color = _baseColor;
        _active = true;
        _speed = stats != null ? stats.attackSpeed : 5f;
        _moving = true;
    }

    private void FixedUpdate()
    {
        if (_moving && _active)
        {
            _rb.MovePosition(_rb.position + _moveDir * (_speed * Time.fixedDeltaTime));
        }
    }
    
    protected override void OnParried(Transform parrySource)
    {
        _moving = false;
        Debug.Log($"{stats.attackName} parried.");
        base.OnParried(parrySource);
        
        // Custom behaviour to replace .OnParried possible
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        _moving = false;
        _active = false;
        StopAllCoroutines(); // ensure no leftover behavior
        sr.color = _baseColor;
    }
}
