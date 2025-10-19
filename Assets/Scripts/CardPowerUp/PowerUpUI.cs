
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

        // Filter out weapon power-ups for weapons the player already has
        PowerUp[] filteredPool = availablePowerUps.Where(p => ShouldIncludePowerUp(p)).ToArray();
        if (filteredPool.Length == 0)
        {
            // If nothing left after filtering, fall back to original pool to avoid blocking the flow
            filteredPool = availablePowerUps;
            Debug.Log("All power-ups filtered out (e.g. duplicate weapons). Falling back to full pool.");
        }

        List<PowerUp> chosen = new();

        // Weighted random selection (no duplicates)
        int attempts = 0;
        while (chosen.Count < cardCount && attempts < 100)
        {
            var selected = GetWeightedRandomPowerUp(filteredPool);
            attempts++;
            if (selected != null && !chosen.Contains(selected))
            {
                Debug.Log($"Selected power-up: {selected.powerUpName} (Tier: {selected.tier})");
                chosen.Add(selected);
            }
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

    private PowerUp GetWeightedRandomPowerUp(PowerUp[] pool)
    {
        if (pool == null || pool.Length == 0)
            return null;

        // Group power-ups by tier using the provided pool
        var grouped = pool.GroupBy(p => p.tier).ToDictionary(g => g.Key, g => g.ToList());

        // Build weighted list
        List<PowerUp> weightedList = new();
        foreach (var kvp in grouped)
        {
            int count = Mathf.CeilToInt(kvp.Value.Count * tierWeights.GetValueOrDefault(kvp.Key, 0f) * 100);
            for (int i = 0; i < count; i++)
                weightedList.Add(kvp.Value[Random.Range(0, kvp.Value.Count)]);
        }

        if (weightedList.Count == 0)
            return pool[Random.Range(0, pool.Length)];

        return weightedList[Random.Range(0, weightedList.Count)];
    }

    // New helper: determine whether a power-up should be offered to the player
    private bool ShouldIncludePowerUp(PowerUp p)
    {
        if (p == null)
            return false;

        // If it's an AddWeaponPowerUp, don't include it if player already has that weapon type in any slot
        if (p is AddWeaponPowerUp addWeaponPU && player != null)
        {
            var handler = player.GetComponent<PlayerWeaponHandler>();
            if (handler != null)
            {
                foreach (var slot in handler.weaponSlots)
                {
                    // Guard for null slot entries just in case
                    if (slot != null && slot.type == addWeaponPU.weaponType)
                    {
                        Debug.Log($"Excluding weapon power-up {addWeaponPU.weaponType} because player already has it in a slot.");
                        return false;
                    }
                }
            }
        }

        // Default: include
        return true;
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

        // Debug weaponStats values
        Debug.Log($"[ShowWeaponReplaceDialog] weaponStats: {(weaponStats != null ? weaponStats.ToString() : "null")}");
        Debug.Log($"[ShowWeaponReplaceDialog] weaponStats.maxClipAmmo: {(weaponStats != null ? weaponStats.maxClipAmmo : -1)}");
        Debug.Log($"[ShowWeaponReplaceDialog] weaponStats.maxAmmo: {(weaponStats != null ? weaponStats.maxAmmo : -1)}");

        for (int i = 0; i < handler.weaponSlots.Count; i++)
        {
            var weaponType = handler.weaponSlots[i].type;
            int slotIndex = i; // Capture for closure

            var cardObj = Instantiate(cardPrefab, cardParent);
            var card = cardObj.GetComponent<PowerUpCard>();
            card.SetupForWeaponReplace(weaponType, () =>
            {
                int clipAmmo = weaponStats != null ? weaponStats.maxClipAmmo : 100;
                int maxClipAmmo = weaponStats != null ? weaponStats.maxClipAmmo : 100;
                int reserveAmmo = weaponStats != null ? weaponStats.maxAmmo : 300;
                int maxReserveAmmo = reserveAmmo;

                Debug.Log($"[ShowWeaponReplaceDialog] Replacing slot {slotIndex}: clipAmmo={clipAmmo}, maxClipAmmo={maxClipAmmo}, reserveAmmo={reserveAmmo}, maxReserveAmmo={maxReserveAmmo}");

                handler.ReplaceWeaponInSlot(
                    slotIndex,
                    newWeapon,
                    weaponPrefab,
                    clipAmmo,
                    maxClipAmmo,
                    reserveAmmo,
                    maxReserveAmmo,
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

    // New: Show selectable cards for each weapon slot to apply ammo increase
    public void ShowAmmoSlotSelection(PlayerWeaponHandler handler, int percentIncrease)
    {
        if (handler == null)
            return;

        showingWeaponReplaceDialog = true;
        Debug.Log($"ShowAmmoSlotSelection called. +{percentIncrease}% to selected slot.");

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
            int slotIndex = i; // capture
            var slot = handler.weaponSlots[i];
            var cardObj = Instantiate(cardPrefab, cardParent);
            var card = cardObj.GetComponent<PowerUpCard>();
            card.SetupForAmmoSlot(
                slotIndex,
                slot != null ? slot.type : PlayerWeaponHandler.WeaponType.Pistol,
                slot != null ? slot.reserveAmmo : 0,
                slot != null ? slot.maxReserveAmmo : 0,
                percentIncrease,
                () =>
                {
                    Debug.Log($"[ShowAmmoSlotSelection] Applying +{percentIncrease}% to slot {slotIndex}");
                    handler.IncreaseMaxReserveForSlot(slotIndex, percentIncrease);
                    showingWeaponReplaceDialog = false;
                    StartCoroutine(HideAndContinue());
                }
            );
        }

        // Block input for a short time to prevent accidental selection
        cardInputBlockUntil = Time.unscaledTime + cardInputBlockTime;

        Debug.Log("ShowAmmoSlotSelection finished.");
    }
}