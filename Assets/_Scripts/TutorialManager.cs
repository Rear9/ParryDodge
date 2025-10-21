using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private string attackOneKey;
    [SerializeField] private string attackTwoKey;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private InputHandler inputHandler;
    
    [Header("Settings")]
    [SerializeField] private float slowTimeDuration = 2f;
    [SerializeField] private float targetTimeScale = 0f;

    // Tutorial Stages
    public enum TutorialState { None, Parry, Dodge }
    private TutorialState currentStep = TutorialState.None;
    private bool inputReceived = false;
    
    private void OnEnable()
    {
        var input = FindFirstObjectByType<InputHandler>();
        if (!input) return;
        input.OnParryPressed += HandleParryInput;
        input.OnDodgePressed += HandleDodgeInput;
    }

    private void OnDisable()
    {
        var input = FindFirstObjectByType<InputHandler>();
        if (!input) return;
        input.OnParryPressed -= HandleParryInput;
        input.OnDodgePressed -= HandleDodgeInput;
    }
    private void HandleParryInput()
    {
        if (currentStep == TutorialState.Parry)
            inputReceived = true;
    }

    private void HandleDodgeInput()
    {
        if (currentStep == TutorialState.Dodge)
            inputReceived = true;
    }
    
    public void StartTutorial()
    {
        if (waveSpawner != null && waveSpawner.enabled) waveSpawner.enabled = false;
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        inputHandler.SetTutorialMode(true,0);
        yield return new WaitForSecondsRealtime(2f);
        // --- Parry Tutorial ---
        SpawnAttack(attackOneKey, spawnPoints[0].position);
        
        yield return new WaitForSecondsRealtime(.8f);
        yield return StartCoroutine(SmoothTimeScale(Time.timeScale, targetTimeScale, slowTimeDuration));
        yield return new WaitForSecondsRealtime(1f); // so you can't parry too early
        currentStep = TutorialState.Parry;
        inputHandler.SetTutorialMode(true,1);
        while (!inputReceived) yield return null;
        inputHandler.SetTutorialMode(true,0);
        inputReceived = false;
        yield return StartCoroutine(SmoothTimeScale(Time.timeScale, 1f, slowTimeDuration));
        yield return new WaitForSecondsRealtime(1f);
        
        
        // --- Dodge Tutorial ---
        SpawnAttack(attackTwoKey, spawnPoints[1].position);
        
        yield return new WaitForSecondsRealtime(1.3f);
        yield return StartCoroutine(SmoothTimeScale(Time.timeScale, targetTimeScale, slowTimeDuration));
        yield return new WaitForSecondsRealtime(1f); // so you can't dodge too early
        currentStep = TutorialState.Dodge;
        inputHandler.SetTutorialMode(true,2);
        while (!inputReceived) yield return null;
        inputHandler.SetTutorialMode(false,0);
        inputReceived = false;
        yield return StartCoroutine(SmoothTimeScale(Time.timeScale, 1f, slowTimeDuration));
        
        // --- Normal Gameplay ---
        currentStep = TutorialState.None;
        waveSpawner.enabled = true;
        print("Tutorial done.");
    }
    
    private void SpawnAttack(string attackKey, Vector3 position)
    {
        GameObject attackObj = AttackPoolManager.Instance.SpawnFromPool(attackKey, position, Quaternion.identity);
        if (!attackObj) return;

        if (attackObj.TryGetComponent(out EnemyAttackCore core))
            core.SetPoolKey(attackKey);

        if (attackObj.TryGetComponent(out IEnemyAttack attack))
            attack.InitAttack(GameObject.FindGameObjectWithTag("Player")?.transform);
    }
    private IEnumerator SmoothTimeScale(float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float normalizedTime = t / duration;
            float easedTime = EaseOut(normalizedTime);
            
            Time.timeScale = Mathf.Lerp(start, end, easedTime);
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // keep physics in sync
            yield return null;
        }
        Time.timeScale = end;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    
    private float EaseOut(float t)
    {
        return t * (2f - t);
    }
    
    public void OnPlayerTutorialInput()
    {
        inputReceived = true;
    }
}
