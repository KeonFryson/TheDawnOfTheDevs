
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int baseEnemiesPerWave = 5;
    public float difficultyMultiplier = 1.25f;
    public float enemySpawnRadius = 5f;
    [Tooltip("Delay in seconds between enemy spawns")]
    public float enemySpawnDelay = 0.5f;

    [Header("Enemy Scaling")]
    [Tooltip("Per-wave multiplier applied to enemy stats (health/speed/attackDamage)")]
    public float enemyStrengthMultiplier = 1.12f;

    [Header("References")]
    public PowerUpUI powerUpUI;

    // Audio for wave completion
    [Header("Audio")]
    [Tooltip("Optional AudioSource to play the wave completion sound.")]
    public AudioSource audioSource;
    [Tooltip("Clips played when a wave is completed (all enemies defeated). One of these will be chosen at random).")]
    public AudioClip[] waveCompleteClips;
    [Range(0f, 1f)]
    public float waveCompleteVolume = 1f;

    private int currentWave = 0;
    public int enemiesAlive = 0;
    private bool waveActive = false;

    public TMP_Text NumberOFEnemies;

    [Header("Wave Number Display")]
    public TMP_Text waveNumberText;
    public Image waveNumberImage;
    [Tooltip("Seconds the wave number stays fully visible before fading")]
    public float waveDisplayDuration = 2f;
    [Tooltip("Seconds taken to fade out the wave number UI")]
    public float waveFadeDuration = 1f;

    private Coroutine waveDisplayCoroutine;
    // Flag used to let callers wait until the display coroutine finishes
    private bool waveNumberDisplayDone = true;


    public void Update()
    {
        if (NumberOFEnemies != null)
        {
            NumberOFEnemies.text = enemiesAlive.ToString();
        }
    }

    private void Start()
    {
        // Start a new stats session when waves start
        GameStats.StartSession();
        StartWave();
    }

    public void StartWave()
    {
        if (waveActive) return; // Prevent duplicate wave starts
        waveActive = true;

        currentWave++;
        if (waveNumberText != null)
        {
            waveNumberText.text = $"Wave {currentWave}";
        }

        // record highest wave reached so far
        GameStats.RecordWaveReached(currentWave);

        int enemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(difficultyMultiplier, currentWave - 1));
        enemiesAlive = enemyCount;

        // Shuffle spawn points for random distribution
        List<Transform> shuffledSpawns = new List<Transform>(spawnPoints);
        for (int i = 0; i < shuffledSpawns.Count; i++)
        {
            int swapIdx = Random.Range(i, shuffledSpawns.Count);
            (shuffledSpawns[i], shuffledSpawns[swapIdx]) = (shuffledSpawns[swapIdx], shuffledSpawns[i]);
        }

        // Start the wave routine that shows the wave UI, waits until it is gone, then spawns enemies
        StartCoroutine(StartWaveRoutine(enemyCount, shuffledSpawns));
        Debug.Log($"Wave {currentWave} started with {enemyCount} enemies (spawning will begin after wave UI hides).");
    }

    // Orchestrates display -> wait -> spawn
    private IEnumerator StartWaveRoutine(int enemyCount, List<Transform> shuffledSpawns)
    {
        // Show wave number text + image and start hide/fade coroutine
        StartWaveNumberDisplay();

        // Wait until the display coroutine signals completion
        yield return new WaitUntil(() => waveNumberDisplayDone);

        // Now begin spawning enemies
        StartCoroutine(SpawnEnemiesCoroutine(enemyCount, shuffledSpawns));
    }

    private void StartWaveNumberDisplay()
    {
        if (waveNumberText == null && waveNumberImage == null)
        {
            // Nothing to show; ensure flag is set so StartWaveRoutine won't wait forever
            waveNumberDisplayDone = true;
            return;
        }

        // Stop any running display coroutine so it restarts cleanly each wave
        if (waveDisplayCoroutine != null)
        {
            StopCoroutine(waveDisplayCoroutine);
            waveDisplayCoroutine = null;
        }

        waveNumberDisplayDone = false;
        waveDisplayCoroutine = StartCoroutine(WaveNumberDisplayCoroutine());
    }

    private IEnumerator WaveNumberDisplayCoroutine()
    {
        // Ensure visible and fully opaque
        if (waveNumberText != null)
        {
            waveNumberText.gameObject.SetActive(true);
            Color c = waveNumberText.color;
            c.a = 1f;
            waveNumberText.color = c;
        }
        if (waveNumberImage != null)
        {
            waveNumberImage.gameObject.SetActive(true);
            Color ci = waveNumberImage.color;
            ci.a = 1f;
            waveNumberImage.color = ci;
        }

        // Stay visible for configured duration
        yield return new WaitForSeconds(Mathf.Max(0f, waveDisplayDuration));

        // Fade out over configured duration
        float elapsed = 0f;
        float fadeDur = Mathf.Max(0.001f, waveFadeDuration);
        while (elapsed < fadeDur)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDur);
            if (waveNumberText != null)
            {
                Color c = waveNumberText.color;
                c.a = alpha;
                waveNumberText.color = c;
            }
            if (waveNumberImage != null)
            {
                Color ci = waveNumberImage.color;
                ci.a = alpha;
                waveNumberImage.color = ci;
            }
            yield return null;
        }

        // Ensure fully hidden and optionally deactivate GameObjects
        if (waveNumberText != null)
        {
            Color c = waveNumberText.color;
            c.a = 0f;
            waveNumberText.color = c;
            waveNumberText.gameObject.SetActive(false);
        }
        if (waveNumberImage != null)
        {
            Color ci = waveNumberImage.color;
            ci.a = 0f;
            waveNumberImage.color = ci;
            waveNumberImage.gameObject.SetActive(false);
        }

        waveDisplayCoroutine = null;
        waveNumberDisplayDone = true;
    }

    private IEnumerator SpawnEnemiesCoroutine(int enemyCount, List<Transform> shuffledSpawns)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var spawn = shuffledSpawns[i % shuffledSpawns.Count];

            // Calculate position in a circle around the spawn point
            float angle = (360f / enemyCount) * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * enemySpawnRadius;
            Vector3 spawnPos = spawn.position + offset;

            var enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (enemy.TryGetComponent(out Blue_Enemy enemyScript))
            {
                // Subscribe to death event
                enemyScript.OnDeath += OnEnemyDeath;

                // Apply per-wave scaling: raise multiplier to (currentWave - 1)
                float waveMul = Mathf.Pow(enemyStrengthMultiplier, Mathf.Max(0, currentWave - 1));

                enemyScript.health *= waveMul;
                enemyScript.speed *= waveMul;

                // Round scaled values to whole numbers
                enemyScript.health = Mathf.Round(enemyScript.health);
                enemyScript.speed = Mathf.Round(enemyScript.speed);

                // Cap speed to a maximum (45)
                enemyScript.speed = Mathf.Min(enemyScript.speed, 45f);

                enemyScript.attackDamage *= waveMul;
                enemyScript.attackDamage = Mathf.Round(enemyScript.attackDamage);
            }
            else
            {
                Debug.LogWarning($"Enemy prefab {enemy.name} missing Enemy script!");
            }

            yield return new WaitForSeconds(enemySpawnDelay);
        }
    }

    private void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            waveActive = false;

            // Play one of the configured wave completion clips (choose randomly)
            if (waveCompleteClips != null && waveCompleteClips.Length > 0)
            {
                AudioClip clipToPlay = waveCompleteClips[Random.Range(0, waveCompleteClips.Length)];
                if (clipToPlay != null)
                {
                    if (audioSource != null)
                    {
                        audioSource.PlayOneShot(clipToPlay, waveCompleteVolume);
                    }
                    else
                    {
                        Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
                        AudioSource.PlayClipAtPoint(clipToPlay, pos, waveCompleteVolume);
                    }
                }
            }

            ShowPowerUpCards();
        }
    }

    private void ShowPowerUpCards()
    {
        if (powerUpUI == null)
        {
            Debug.LogError("PowerUpUI not assigned to WaveManager!");
            return;
        }

        powerUpUI.gameObject.SetActive(true);

        powerUpUI.ShowCardsSafe();
    }
}