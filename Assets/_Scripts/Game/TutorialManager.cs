using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private string attackOneKey;
    [SerializeField] private string attackTwoKey;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private InputHandler inputHandler;
    
    [Header("Tutorial UI")]
    [SerializeField] private GameObject parryTutorialImages;
    [SerializeField] private GameObject dodgeTutorialImages;
    [SerializeField] private GameObject moveTutorialImages;
    
    [Header("Settings")]
    [SerializeField] private float slowTimeDuration = 2f;
    [SerializeField] private float targetTimeScale = 0f;
    [SerializeField] private float moveFlashDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;

    // tutorial stages
    public enum TutorialState { None, Parry, Dodge }
    private TutorialState currentStep = TutorialState.None;
    private bool inputReceived = false;
    
    private void Awake()
    {
        // Hide all tutorial images at start and set alpha to 0
        HideAllChildImages(parryTutorialImages);
        HideAllChildImages(dodgeTutorialImages);
        HideAllChildImages(moveTutorialImages);
    }
    
    private void HideAllChildImages(GameObject parent)
    {
        if (parent == null) return;
        
        SpriteRenderer[] images = parent.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer img in images)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
            img.enabled = false;
        }
    }
    
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
        
        // --- parry tutorial ---
        
        SpawnAttack(attackOneKey, spawnPoints[0].position);
        
        yield return new WaitForSecondsRealtime(.8f);
        StartCoroutine(SmoothTimeScale(Time.timeScale, targetTimeScale, slowTimeDuration));
        currentStep = TutorialState.Parry;
        StartCoroutine(FadeInTutorialImages(parryTutorialImages)); // fade in parry tutorial UI
        yield return new WaitForSecondsRealtime(1f); // so you can't parry too early
        inputHandler.SetTutorialMode(true,1);
        while (!inputReceived) yield return null;
        inputHandler.SetTutorialMode(true,0);  
        inputReceived = false;
        StartCoroutine(FadeOutTutorialImages(parryTutorialImages)); // fade out parry tutorial UI
        yield return StartCoroutine(SmoothTimeScale(Time.timeScale, 1f, slowTimeDuration));
        yield return new WaitForSecondsRealtime(1f);
        
        
        // --- dodge tutorial ---
        
        SpawnAttack(attackTwoKey, spawnPoints[1].position);
        
        yield return new WaitForSecondsRealtime(1.3f);
        StartCoroutine(SmoothTimeScale(Time.timeScale, targetTimeScale, slowTimeDuration));
        currentStep = TutorialState.Dodge;
        StartCoroutine(FadeInTutorialImages(dodgeTutorialImages)); // fade in dodge tutorial UI
        yield return new WaitForSecondsRealtime(1f); // so you can't dodge too early
        inputHandler.SetTutorialMode(true,2);
        while (!inputReceived) yield return null;
        StartCoroutine(FadeOutTutorialImages(dodgeTutorialImages)); // fade out dodge tutorial UI\
        inputHandler.SetTutorialMode(false,0);
        inputReceived = false;
        StartCoroutine(SmoothTimeScale(Time.timeScale, 1f, slowTimeDuration));
        yield return new WaitForSecondsRealtime(3f);
        yield return StartCoroutine(MoveTutorial());
        
        // --- normal gameplay ---
        currentStep = TutorialState.None;
        yield return new WaitForSecondsRealtime(5f);
        waveSpawner.enabled = true;
        print("Tutorial done.");
    }
    
    private IEnumerator MoveTutorial()
    {
        yield return StartCoroutine(FadeInTutorialImages(moveTutorialImages));
        yield return new WaitForSeconds(moveFlashDuration);
        yield return StartCoroutine(FadeOutTutorialImages(moveTutorialImages));
    }
    
    private IEnumerator FadeInTutorialImages(GameObject parent)
    {
        if (parent == null) yield break;
        
        SpriteRenderer[] images = parent.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer img in images)
        {
            StartCoroutine(FadeInImage(img)); // Start all fades concurrently
        }
    }
    
    private IEnumerator FadeOutTutorialImages(GameObject parent)
    {
        if (parent == null) yield break;
        
        SpriteRenderer[] images = parent.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer img in images)
        {
            if (img.enabled)
            {
                StartCoroutine(FadeOutImage(img)); // Start all fades concurrently
            }
        }
    }
    
    private IEnumerator FadeInImage(SpriteRenderer img)
    {
        if (img == null) yield break;
        img.enabled = true;
        
        Color c = img.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // unscaled time for UI
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            img.color = c;
            yield return null;
        }
        
        c.a = 1f;
        img.color = c;
    }
    
    private IEnumerator FadeOutImage(SpriteRenderer img)
    {
        if (img == null) yield break;
        
        Color c = img.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            img.color = c;
            yield return null;
        }
        
        c.a = 0f;
        img.color = c;
        img.enabled = false;
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