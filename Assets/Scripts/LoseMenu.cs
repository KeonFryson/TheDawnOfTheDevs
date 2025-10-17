using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject canvasRoot; // assign the parent canvas / panel (disable by default)
    public TMP_Text titleText; // large centered title
    public TMP_Text waveText;
    public TMP_Text enemiesText;
    public TMP_Text damageDealtText;
    public TMP_Text damageReceivedText;
    public TMP_Text timeSurvivedText;
    public Button resetButton;
    public Button menuButton;

    private void Awake()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetPressed);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuPressed);
    }

    // Call this to show the lose menu (records final stats, pauses time)
    public void ShowLoseMenu()
    {
        var summary = GameStats.EndSession();

        if (canvasRoot != null) canvasRoot.SetActive(true);

        if (titleText != null)
            titleText.text = $"Defeat\nWave {summary.highestWave} Reached";

        if (waveText != null)
            waveText.text = $"Highest wave cleared: {summary.highestWave}";

        if (enemiesText != null)
            enemiesText.text = $"Total enemies defeated: {summary.totalEnemiesDefeated}";

        if (damageDealtText != null)
            damageDealtText.text = $"Total damage dealt: {Mathf.RoundToInt(summary.totalDamageDealt)}";

        if (damageReceivedText != null)
            damageReceivedText.text = $"Total damage received: {Mathf.RoundToInt(summary.totalDamageReceived)}";

        if (timeSurvivedText != null)
        {
            TimeSpan t = TimeSpan.FromSeconds(summary.timeSurvived);
            timeSurvivedText.text = $"Time survived: {t.Minutes:D2}:{t.Seconds:D2}";
        }

        Time.timeScale = 0f; // pause game while on lose screen
    }

    public void OnResetPressed()
    {
        Time.timeScale = 1f;
        // Reset to main scene (index 0). Change if your main scene index differs.
        SceneManager.LoadScene(1);
    }
    public void OnMenuPressed()
    {
        Time.timeScale = 1f;
        // Reset to main scene (index 0). Change if your main scene index differs.
        SceneManager.LoadScene(0);
    }

}