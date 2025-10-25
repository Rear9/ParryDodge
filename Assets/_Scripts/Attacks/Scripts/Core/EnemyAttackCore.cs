using System;
using UnityEngine;

public abstract class EnemyAttackCore : MonoBehaviour
{
    [Header("Attack Stats")] 
    [SerializeField] protected AttackStats stats;

    protected Rigidbody2D _rb;
    protected bool _active = true;
    protected bool _playerHit = false;
    private string _poolKey;
    
    protected virtual void Awake() => _rb = GetComponent<Rigidbody2D>();
    protected virtual void OnEnable() => _active = true;
    protected virtual void OnDisable() // reset when disabled
    {
        _active = false;
        CancelInvoke();
        StopAllCoroutines();
    }

    protected virtual void
        OnTriggerEnter2D(
            Collider2D other) // default attack collision = if parryable, reset player parry cd && if not, hit player
    {
        if (!_active) return;

        int layer = other.gameObject.layer;
        if (layer == LayerMask.NameToLayer("PlayerParry") &&
            stats.parryable) // if parrying an attack that can be parried
        {
            // parry the attack if parryable, refresh player parry cooldown
            if (!other.TryGetComponent(out Actions plrActions)) return;
            plrActions.ParrySuccess();
            OnParried(other.transform);
        }
        else if (layer == LayerMask.NameToLayer("Player") && gameObject.layer != LayerMask.NameToLayer("PlayerAttack")) // if player isn't performing an action
        {
            if (_playerHit) return;
            _playerHit = true;
            if (other.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage((stats.damage > 0 ? stats.damage : 1));
            }
        }
        else if (layer == LayerMask.NameToLayer("PlayerParry") && !stats.parryable)
        {
            // parrying unparriable attack
            if (_playerHit) return;
            _playerHit = true;
            if (other.TryGetComponent(out PlayerHealth health))
            {
                health.TakeDamage((stats.damage > 0 ? stats.damage : 1));
            }
        }
    }

    protected virtual void OnParried(Transform parrySource) // default = disappear into pool when parried
    {
        ReturnToPool();
    }
    protected void ReturnToPool() // return attack to pool & reset params
    {
        _active = false;
        gameObject.layer = LayerMask.NameToLayer("EnemyAttack");
        StopAllCoroutines();

        if (_rb)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
        
        if(!string.IsNullOrEmpty(_poolKey))
        {
            AttackPoolManager.Instance.ReturnToPool(_poolKey,gameObject);
        }
    }
    public void SetPoolKey(string key) // set key to return after attack's lifetime
    {
        _poolKey = key;
        if (stats?.lifetime > 0)
        {
            Invoke(nameof(ReturnToPool), stats.lifetime);
        }
    }
}
