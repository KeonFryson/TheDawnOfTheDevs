// ============================
// WaveManager.cs
// ============================
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public int baseEnemiesPerWave = 5;
    public float difficultyMultiplier = 1.25f;

    [Header("References")]
    public PowerUpUI powerUpUI;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool waveActive = false;
    public float enemySpawnRadius = 5f;
    private void Start()
    {
        StartWave();
    }

    public void StartWave()
    {
        if (waveActive) return; // Prevent duplicate wave starts
        waveActive = true;

        currentWave++;
        int enemyCount = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(difficultyMultiplier, currentWave - 1));
        enemiesAlive = enemyCount;

        // Shuffle spawn points for random distribution
        List<Transform> shuffledSpawns = new List<Transform>(spawnPoints);
        for (int i = 0; i < shuffledSpawns.Count; i++)
        {
            int swapIdx = Random.Range(i, shuffledSpawns.Count);
            (shuffledSpawns[i], shuffledSpawns[swapIdx]) = (shuffledSpawns[swapIdx], shuffledSpawns[i]);
        }

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
                enemyScript.OnDeath += OnEnemyDeath;
            }
            else
            {
                Debug.LogWarning($"Enemy prefab {enemy.name} missing Enemy script!");
            }
        }

        Debug.Log($"Wave {currentWave} started with {enemyCount} enemies.");
    }

    private void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            waveActive = false;
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
