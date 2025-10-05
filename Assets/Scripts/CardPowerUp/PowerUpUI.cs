// ============================
// PowerUpUI.cs
// ============================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpUI : MonoBehaviour
{
    [Header("UI Setup")]
    public PowerUp[] availablePowerUps;
    public GameObject cardPrefab;
    public Transform cardParent;

    private PlayerInputHandler player;

    private void Awake()
    {
        player = FindFirstObjectByType<PlayerInputHandler>();
        HideCardsInstant();
    }

    public void ShowCardsSafe()
    {
        // Ensures we can safely start coroutine even if this object was inactive
        StartCoroutine(ShowCardsCoroutine());
    }

    private IEnumerator ShowCardsCoroutine()
    {
        yield return null;
        ShowCards();
    }

    public void ShowCards()
    {
        if (availablePowerUps == null || availablePowerUps.Length == 0)
        {
            Debug.LogWarning("No power-ups assigned!");
            FindFirstObjectByType<WaveManager>()?.StartWave();
            return;
        }

        gameObject.SetActive(true);

        int cardCount = Mathf.Min(3, availablePowerUps.Length);
        List<int> chosen = new();

        while (chosen.Count < cardCount)
        {
            int idx = Random.Range(0, availablePowerUps.Length);
            if (!chosen.Contains(idx)) chosen.Add(idx);
        }

        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        foreach (int idx in chosen)
        {
            var cardObj = Instantiate(cardPrefab, cardParent);
            var card = cardObj.GetComponent<PowerUpCard>();
            card.Setup(availablePowerUps[idx], OnCardSelected);
        }
    }

    private void OnCardSelected(PowerUp powerUp)
    {
        Debug.Log("Card selected: " + powerUp.powerUpName);
        powerUp.Apply(player);
        StartCoroutine(HideAndContinue());
    }

    private IEnumerator HideAndContinue()
    {
        HideCards();
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Starting next wave...");
        FindFirstObjectByType<WaveManager>()?.StartWave();
        StartCoroutine(DeactivateNextFrame());
    }

    public void HideCards()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

       
    }

    private void HideCardsInstant()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
        gameObject.SetActive(false);
    }

    private IEnumerator DeactivateNextFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
}
