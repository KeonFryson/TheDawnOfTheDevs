
using System.Linq;
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

    private float cardInputBlockTime = 1f; // seconds
    private float cardInputBlockUntil = 0f;
    private bool showingWeaponReplaceDialog = false;

    private readonly Dictionary<PowerUpTier, float> tierWeights = new()
    {
        { PowerUpTier.Minor, 0.7f },
        { PowerUpTier.Major, 0.25f },
        { PowerUpTier.Ultimate, 0.05f }
    };

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
        Debug.Log("ShowCards called.");

        if (availablePowerUps == null || availablePowerUps.Length == 0)
        {
            Debug.LogWarning("No power-ups assigned!");
            FindFirstObjectByType<WaveManager>()?.StartWave();
            return;
        }

        Debug.Log("Activating PowerUp UI.");
        gameObject.SetActive(true);

        // Wait one frame before pausing and disabling input
        StartCoroutine(PauseAfterFrame());

        int cardCount = Mathf.Min(3, availablePowerUps.Length);
        Debug.Log($"Calculated cardCount: {cardCount}");

        List<PowerUp> chosen = new();

        // Weighted random selection (allow duplicates)
        for (int i = 0; i < cardCount; i++)
        {
            var selected = GetWeightedRandomPowerUp();
            Debug.Log(selected != null
                ? $"Selected power-up: {selected.powerUpName} (Tier: {selected.tier})"
                : "Selected power-up: null");

            if (selected != null)
                chosen.Add(selected);
        }

        Debug.Log($"Total chosen power-ups: {chosen.Count}");

        foreach (Transform child in cardParent)
        {
            Debug.Log($"Destroying card: {child.gameObject.name}");
            Destroy(child.gameObject);
        }

        foreach (var powerUp in chosen)
        {
            Debug.Log($"Instantiating card for power-up: {powerUp.powerUpName}");
            var cardObj = Instantiate(cardPrefab, cardParent);
            var card = cardObj.GetComponent<PowerUpCard>();
            card.Setup(powerUp, OnCardSelected);
        }
        // Block input for a short time to prevent accidental selection
        cardInputBlockUntil = Time.unscaledTime + cardInputBlockTime;

        Debug.Log("ShowCards finished.");
    }

    private IEnumerator PauseAfterFrame()
    {
        yield return null;
        Time.timeScale = 0f;
        if (player != null)
            player.SetInputEnabled(false);
    }

    private PowerUp GetWeightedRandomPowerUp()
    {
        // Group power-ups by tier
        var grouped = availablePowerUps.GroupBy(p => p.tier).ToDictionary(g => g.Key, g => g.ToList());

        // Build weighted list
        List<PowerUp> weightedList = new();
        foreach (var kvp in grouped)
        {
            int count = Mathf.CeilToInt(kvp.Value.Count * tierWeights[kvp.Key] * 100);
            for (int i = 0; i < count; i++)
                weightedList.Add(kvp.Value[Random.Range(0, kvp.Value.Count)]);
        }

        if (weightedList.Count == 0)
            return availablePowerUps[Random.Range(0, availablePowerUps.Length)];

        return weightedList[Random.Range(0, weightedList.Count)];
    }

    private void OnCardSelected(PowerUp powerUp)
    {
        Debug.Log("Card selected: " + powerUp.powerUpName);
        powerUp.Apply(player);
        StartCoroutine(HideAndContinue());
    }

    private IEnumerator HideAndContinue()
    {
        if (showingWeaponReplaceDialog)
        {
            yield break;
        }

        HideCards();
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Starting next wave...");
        FindFirstObjectByType<WaveManager>()?.StartWave();
        StartCoroutine(DeactivateNextFrame());

        Time.timeScale = 1f;
    }

    public void HideCards()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        Time.timeScale = 1f;
        if (player != null)
            player.SetInputEnabled(true);
    }

    private void HideCardsInstant()
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);
        gameObject.SetActive(false);

        Time.timeScale = 1f;
        if (player != null)
            player.SetInputEnabled(true);
    }

    private IEnumerator DeactivateNextFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    // Input block check for PowerUpCard
    public bool CanSelectCard()
    {
        return Time.unscaledTime >= cardInputBlockUntil;
    }

    // Weapon replacement dialog
    public void ShowWeaponReplaceDialog(PlayerWeaponHandler handler, PlayerWeaponHandler.WeaponType newWeapon, GameObject weaponPrefab, WeaponStats weaponStats)
    {
        showingWeaponReplaceDialog = true;
        Debug.Log($"ShowWeaponReplaceDialog called. New weapon: {newWeapon}");

        // Wait one frame before pausing and disabling input
        StartCoroutine(PauseAfterFrame());

        // Destroy any existing cards
        foreach (Transform child in cardParent)
        {
            Debug.Log($"Destroying card: {child.gameObject.name}");
            Destroy(child.gameObject);
        }

        for (int i = 0; i < handler.weaponSlots.Count; i++)
        {
            var weaponType = handler.weaponSlots[i].type;
            int slotIndex = i; // Capture for closure

            var cardObj = Instantiate(cardPrefab, cardParent);
            var card = cardObj.GetComponent<PowerUpCard>();
            card.SetupForWeaponReplace(weaponType, () =>
            {
                Debug.Log($"Replacing weapon in slot {slotIndex}: {weaponType} -> {newWeapon}");
                handler.ReplaceWeaponInSlot(
                    slotIndex,
                    newWeapon,
                    weaponPrefab,
                    weaponStats != null ? weaponStats.maxAmmo : 100,
                    weaponStats != null ? weaponStats.maxAmmo : 100,
                    weaponStats
                );
                showingWeaponReplaceDialog = false;
                StartCoroutine(HideAndContinue());
            });
        }

        // Block input for a short time to prevent accidental selection
        cardInputBlockUntil = Time.unscaledTime + cardInputBlockTime;

        Debug.Log("ShowWeaponReplaceDialog finished.");
    }
}