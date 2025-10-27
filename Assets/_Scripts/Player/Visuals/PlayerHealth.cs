using System.Collections;
using UnityEngine;
using UnityEngine.UI; // for Image reference (hit screen)

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHP = 10;
    private float _currentHP;

    [Header("References")]
    [SerializeField] private Image hitScreen; // assign red overlay image
    [SerializeField] private float hitScreenDuration = 0.5f;
    
    private UIManager _ui;

    private void Awake() // update HP objects to show when player loads in
    {
        _currentHP = maxHP;
        _ui = FindFirstObjectByType<UIManager>();
        if (_ui != null) _ui.UpdateHP(_currentHP, maxHP);
        if (hitScreen != null) hitScreen.enabled = false;
    }

    public void TakeDamage(float dmg)
    {
        _currentHP -= dmg;
        _currentHP = Mathf.Clamp(_currentHP, 0, maxHP);
        if (_ui != null) _ui.UpdateHP(_currentHP, maxHP);
        StartCoroutine(HitFlash());
        if (_currentHP <= 0) Die();
    }

    private IEnumerator HitFlash()
    {
        if (hitScreen == null) yield break;

        Color c = hitScreen.color;
        c.a = 0f;
        hitScreen.enabled = true;

        float half = hitScreenDuration * 0.5f;
        float t = 0f;

        // fade in
        while (t < half)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, .5f, t / half);
            hitScreen.color = c;
            yield return null;
        }

        t = 0f;
        // fade out
        while (t < half)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(.5f, 0f, t / half);
            hitScreen.color = c;
            yield return null;
        }

        hitScreen.enabled = false;
    }


    private void Die() // record death in stats and send player back to main menu for now
    {
        if (StatsManager.Instance != null && _ui != null)
        {
            string currentWave = _ui != null ? _ui.GetCurrentWaveName() : "N/A";
            StatsManager.Instance.RecordDeath(currentWave);
            StatsManager.Instance.RecordFull(currentWave);
        }
        
        // Return to menu after death
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
    }
}