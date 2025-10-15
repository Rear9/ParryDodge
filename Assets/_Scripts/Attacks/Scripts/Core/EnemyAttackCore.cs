using System;
using UnityEngine;

public abstract class EnemyAttackCore : MonoBehaviour
{
    [Header("Attack Stats")] 
    [SerializeField] protected AttackStats stats;

    protected Rigidbody2D _rb;
    protected Collider2D _col;
    protected bool _active = true;
    private string _poolKey;
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

    protected virtual void OnEnable()
    {
        _active = true;
    }
    
    protected virtual void OnDisable()
    {
        // Cancel any pending invokes when returned to pool
        CancelInvoke(nameof(ReturnToPool));
    }
    
    public void SetPoolKey(string key)
    {
        _poolKey = key;

        if (stats != null && stats.lifetime > 0)
        {
            Invoke(nameof(ReturnToPool), stats.lifetime);
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Hit");
            ReturnToPool();
            // damage the player
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerParry") && stats.parryable)
        {
            // parry the attack if parryable, refresh player parry cooldown
            if (other.TryGetComponent(out Actions plrActions))
            {
                plrActions.ParrySuccess();
            }

            OnParried(other.transform);
            Debug.Log("Parry");
        }
    }
    
    private void ReturnToPool()
    {
        _active = false;
        StopAllCoroutines();
        if (TryGetComponent(out Rigidbody2D rb)) rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f;
        
        AttackPoolManager.Instance.ReturnToPool(_poolKey, gameObject);
    }

    protected virtual void OnParried(Transform parrySource)
    {
        ReturnToPool();
    }
}
