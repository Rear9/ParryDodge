using UnityEngine;
using System.Collections;

public class AreaAttack : EnemyAttackCore, IEnemyAttack
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color pulseColor = new Color(255,255,255,.2f);

    [Header("Timings")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float activeDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;

    private Color _baseColor;
    private Collider2D _collider;

    protected override void Awake()
    {
        base.Awake();
        _baseColor = sr.color;
        _collider = GetComponent<Collider2D>();
        if (_collider) _collider.enabled = false;
    }

    public void InitAttack(Transform player)
    {
        _active = false;
        
        StopAllCoroutines();
        if (_collider) _collider.enabled = false;
        sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f); // invisible at start

        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        // fade in + telegraph
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            float t = Mathf.PingPong(timer * 2f, 1f);
            sr.color = Color.Lerp(new Color(_baseColor.r, _baseColor.g, _baseColor.b, .2f), pulseColor, t);
            timer += Time.deltaTime;
            yield return null;
        }

        sr.color = _baseColor;

        // enable/disable collisions
        if (_collider) _collider.enabled = true;
        _active = true;
        yield return new WaitForSeconds(activeDuration);
        _active = false;
        if (_collider) _collider.enabled = false;

        // fade out
        timer = 0f;
        Color startColor = sr.color;
        while (timer < fadeOutDuration)
        {
            float progress = timer / fadeOutDuration;
            sr.color = Color.Lerp(startColor, new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f), progress);
            timer += Time.deltaTime;
            yield return null;
        }

        sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f);
        ReturnToPool();
    }
}
