using System;
using UnityEngine;

public abstract class EnemyAttackCore : MonoBehaviour
{
    [Header("Attack Stats")] 
    [SerializeField] protected AttackStats stats;

    protected Rigidbody2D _rb;
    protected bool _active = true;
    private string _poolKey;
    
    protected virtual void Awake() => _rb = GetComponent<Rigidbody2D>();
    protected virtual void OnEnable() => _active = true;
    protected virtual void OnDisable()
    {
        _active = false;
        CancelInvoke();
        StopAllCoroutines();
    }
    public void SetPoolKey(string key)
    {
        _poolKey = key;
        if (stats?.lifetime > 0)
        {
            Invoke(nameof(ReturnToPool), stats.lifetime);
        }
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;

        int layer = other.gameObject.layer;
        if (layer == LayerMask.NameToLayer("PlayerParry")) // if parrying an attack that can be parried
        {
            // parry the attack if parryable, refresh player parry cooldown
            if (!stats.parryable || !other.TryGetComponent(out Actions plrActions)) return;
            plrActions.ParrySuccess();
            OnParried(other.transform);
        }
        else if (layer == LayerMask.NameToLayer("Player") && gameObject.layer != LayerMask.NameToLayer("PlayerAttack")) // if player isn't performing an action
        {
            Debug.Log("Hit");
            // dmg player from a health manager
        }
    }
    protected void ReturnToPool()
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
    protected virtual void OnParried(Transform parrySource)
    {
        ReturnToPool();
    }
}
