using System;
using UnityEngine;

public abstract class EnemyAttackCore : MonoBehaviour
{
    [Header("Attack Stats")] 
    [SerializeField] protected AttackStats stats;

    protected Rigidbody2D _rb;
    protected Collider2D _col;
    protected bool _active = true;
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        if (stats != null && stats.lifetime > 0)
        {
            Destroy(gameObject, stats.lifetime);
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // damage the player
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("PlayerParry") && stats.parryable)
        {
            // parry the attack if parryable
            OnParried(other.transform);
        }
    }
    protected abstract void OnParried(Transform parrySource);
}
