using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerGrenadeHandler : MonoBehaviour
{
    [Serializable]
    public class GrenadeSlot
    {
        public GrenadeData type;
        public int count;
    }

    [Header("Grenade Slots (2)")]
    public GrenadeSlot[] slots = new GrenadeSlot[2] { new GrenadeSlot(), new GrenadeSlot() };

    [Header("Slot Capacity")]
    public int maxPerSlot = 100;


    [Header("Throw / Grenade Prefab (2D)")]
    [Tooltip("Prefab must have a Rigidbody2D and a Grenade behaviour to handle fuse/explosion.")]
    public GameObject grenadePrefab;
    public float throwForce = 8f;
    public float upwardBias = 0.5f; // small arc bias

    [Header("Debug")]
    public bool debugLogs = true;

    // Selected slot index: 0 or 1. -1 = no selection (no grenades)
    private int selectedSlot = -1;

    private void Start()
    {
        if (slots == null || slots.Length != 2)
            slots = new GrenadeSlot[2] { new GrenadeSlot(), new GrenadeSlot() };

        // Ensure slots elements are not null
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null) slots[i] = new GrenadeSlot();

        // initialize selectedSlot to a sensible default
        InitializeSelectedSlot();
        UpdateUI();
    }

    private void InitializeSelectedSlot()
    {
        bool s0 = slots.Length > 0 && slots[0].count > 0;
        bool s1 = slots.Length > 1 && slots[1].count > 0;

        if (s0 && !s1) selectedSlot = 0;
        else if (s1 && !s0) selectedSlot = 1;
        else if (s0 && s1) selectedSlot = 0; // default to slot 0 when both present
        else selectedSlot = -1;
    }

    private void Update()
    {
        bool gPressed = false;

        if (Keyboard.current != null)
        {
            gPressed = Keyboard.current.gKey.wasPressedThisFrame;
        }

        if (!gPressed && UnityEngine.Input.GetKeyDown(KeyCode.G))
        {
            gPressed = true;
        }

        if (gPressed)
        {
            bool shiftHeld = false;
            if (Keyboard.current != null)
            {
                // Use correct Input System properties for modifier keys
                var left = Keyboard.current.leftShiftKey;
                var right = Keyboard.current.rightShiftKey;
                shiftHeld = (left != null && left.isPressed) || (right != null && right.isPressed);
            }
            else
            {
                shiftHeld = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
            }

            if (shiftHeld)
            {
                ToggleSelectedSlot();
            }
            else
            {
                if (debugLogs)
                {
                    string s0 = slots[0].type != null ? $"{slots[0].type.displayName}:{slots[0].count}" : $"empty:{slots[0].count}";
                    string s1 = slots[1].type != null ? $"{slots[1].type.displayName}:{slots[1].count}" : $"empty:{slots[1].count}";
                    Debug.Log($"G pressed (throw) - slots: [{s0}], [{s1}], prefabAssigned: {grenadePrefab != null}, selectedSlot: {selectedSlot}");
                }
                TryThrowGrenade();
            }
        }
    }

    private void ToggleSelectedSlot()
    {
        bool s0 = slots.Length > 0 && slots[0].count > 0;
        bool s1 = slots.Length > 1 && slots[1].count > 0;

        if (!s0 && !s1)
        {
            selectedSlot = -1;
            if (debugLogs) Debug.Log("ToggleSelectedSlot: no grenades to select.");
            UpdateUI();
            return;
        }

        // If both have grenades, toggle between them
        if (s0 && s1)
        {
            selectedSlot = (selectedSlot == 0) ? 1 : 0;
            if (debugLogs) Debug.Log($"Toggled selectedSlot -> {selectedSlot}");
            UpdateUI();
            return;
        }

        // If only one slot has grenades, pick that slot
        if (s0)
            selectedSlot = 0;
        else
            selectedSlot = 1;

        if (debugLogs) Debug.Log($"SelectedSlot set to available slot {selectedSlot}");
        UpdateUI();
    }

    // Backwards compatibility: simple AddOrReplaceGrenades (gives untyped grenades)
    public void AddOrReplaceGrenades(int amount)
    {
        if (amount <= 0) return;

        // Put into first slot that isn't full (no type)
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].type == null && slots[i].count < maxPerSlot)
            {
                slots[i].count = Mathf.Min(maxPerSlot, slots[i].count + amount);
                InitializeSelectedSlot();
                UpdateUI();
                return;
            }
        }

        // If all occupied, replace slot 0 with untyped count
        slots[0].type = null;
        slots[0].count = Mathf.Min(maxPerSlot, amount);
        InitializeSelectedSlot();
        UpdateUI();
    }

    // New: add grenades of a specific type (GrenadeData)
    public void AddTypedGrenades(GrenadeData type, int amount)
    {
        if (type == null || amount <= 0) return;

        // 1) If a slot already has this type, add to it
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].type == type)
            {
                slots[i].count = Mathf.Min(maxPerSlot, slots[i].count + amount);
                InitializeSelectedSlot();
                UpdateUI();
                if (debugLogs) Debug.Log($"Added {amount}x {type.displayName} to existing slot {i}. Now {slots[i].count}");
                return;
            }
        }

        // 2) Find an empty slot and assign
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].count == 0 || slots[i].type == null)
            {
                slots[i].type = type;
                slots[i].count = Mathf.Min(maxPerSlot, amount);
                InitializeSelectedSlot();
                UpdateUI();
                if (debugLogs) Debug.Log($"Assigned {amount}x {type.displayName} to empty slot {i}.");
                return;
            }
        }

        // 3) Both slots occupied with other types -> replace slot 0
        slots[0].type = type;
        slots[0].count = Mathf.Min(maxPerSlot, amount);
        InitializeSelectedSlot();
        UpdateUI();
        if (debugLogs) Debug.Log($"Replaced slot 0 with {amount}x {type.displayName} (capped to {slots[0].count}).");
    }

    // Try to consume one grenade; returns the GrenadeData of the consumed grenade (or null if untyped), or null if none available
    public GrenadeData TryConsumeGrenade()
    {
        // Prefer selected slot if valid and has grenades
        if (selectedSlot >= 0 && selectedSlot < slots.Length && slots[selectedSlot].count > 0)
        {
            int i = selectedSlot;
            slots[i].count--;
            GrenadeData consumedType = slots[i].type;
            if (slots[i].count == 0)
            {
                slots[i].type = null;
                // auto-select other slot if it has grenades, otherwise clear selection
                bool otherHas = (i == 0) ? (slots[1].count > 0) : (slots[0].count > 0);
                selectedSlot = otherHas ? (i == 0 ? 1 : 0) : -1;
            }
            UpdateUI();
            if (debugLogs) Debug.Log($"Consumed one grenade from selected slot {i} -> remaining {slots[i].count} of {(consumedType != null ? consumedType.displayName : "untyped")}");
            return consumedType;
        }

        // Fallback: first non-empty slot (maintain original behavior)
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].count > 0)
            {
                slots[i].count--;
                GrenadeData consumedType = slots[i].type;
                if (slots[i].count == 0)
                {
                    slots[i].type = null;
                }
                // update selectedSlot sensibly
                InitializeSelectedSlot();
                UpdateUI();
                if (debugLogs) Debug.Log($"Consumed one grenade from fallback slot {i} -> remaining {slots[i].count} of {(consumedType != null ? consumedType.displayName : "untyped")}");
                return consumedType;
            }
        }

        if (debugLogs) Debug.Log("No grenades available to consume.");
        return null;
    }

    private void UpdateUI()
    {
        // UI icon & selection logic
        bool slot0Has = slots.Length > 0 && slots[0].count > 0;
        bool slot1Has = slots.Length > 1 && slots[1].count > 0;

        // Determine visible selection:
        int visibleSelection = 0;
        if (slot0Has || slot1Has)
        {
            // If player manually selected a slot and it has grenades, respect it
            if (selectedSlot >= 0 && selectedSlot < slots.Length && slots[selectedSlot].count > 0)
            {
                visibleSelection = selectedSlot + 1;
            }
            else
            {
                // If only one slot has grenades, show that one
                if (slot0Has && !slot1Has) visibleSelection = 1;
                else if (slot1Has && !slot0Has) visibleSelection = 2;
                else visibleSelection = 0; // both present but no selection
            }
        }
        else
        {
            visibleSelection = 0;
        }

        // Try to extract optional Sprite "icon" from GrenadeData via reflection (works whether or not field/property exists)
        Sprite icon0 = GetIconSprite(slots.Length > 0 ? slots[0].type : null);
        Sprite icon1 = GetIconSprite(slots.Length > 1 ? slots[1].type : null);

        // Update WeaponUI if present
        var weaponUI = FindFirstObjectByType<WeaponUI>();
        if (weaponUI != null)
        {
            weaponUI.UpdateGrenadeDisplay(icon0, slot0Has ? slots[0].count : 0, icon1, slot1Has ? slots[1].count : 0, visibleSelection);
        }
    }

    // Reflection helper: attempt to read a Sprite named "icon" from GrenadeData (field or property)
    private Sprite GetIconSprite(GrenadeData data)
    {
        if (data == null) return null;

        Type t = data.GetType();
        // field
        var f = t.GetField("icon", BindingFlags.Public | BindingFlags.Instance);
        if (f != null && typeof(Sprite).IsAssignableFrom(f.FieldType))
        {
            return f.GetValue(data) as Sprite;
        }

        // property
        var p = t.GetProperty("icon", BindingFlags.Public | BindingFlags.Instance);
        if (p != null && typeof(Sprite).IsAssignableFrom(p.PropertyType))
        {
            return p.GetValue(data) as Sprite;
        }

        return null;
    }

    private void TryThrowGrenade()
    {
        if (grenadePrefab == null)
        {
            Debug.LogWarning("PlayerGrenadeHandler: grenadePrefab is not assigned.");
            return;
        }

        // Attempt to consume a grenade and get its type
        GrenadeData chosenType = TryConsumeGrenade();
        if (chosenType == null && (slots[0].count == 0 && slots[1].count == 0))
        {
            // No grenades at all
            return;
        }

        // spawn position (keep z from transform to avoid depth change)
        Vector3 spawnPos3 = transform.position + Vector3.up * 0.6f;
        Vector2 spawnPos = new Vector2(spawnPos3.x, spawnPos3.y);

        if (debugLogs) Debug.Log($"Spawning grenade at {spawnPos}");
        GameObject grenadeObj = Instantiate(grenadePrefab, spawnPos3, Quaternion.identity);
        if (grenadeObj == null)
        {
            Debug.LogError("Instantiate returned null grenadeObj.");
            return;
        }
        if (debugLogs) Debug.Log($"Grenade spawned: {grenadeObj.name}");

        // If the grenade prefab has a Grenade component, assign its grenadeData to the consumed type
        var grenadeComp = grenadeObj.GetComponent<Grenade>();
        if (grenadeComp != null)
        {
            if (chosenType != null)
                grenadeComp.grenadeData = chosenType;
        }

        Rigidbody2D rb = grenadeObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Compute mouse world position correctly for 2D (use camera distance to world plane)
            Vector2 mouseWorldPos2;
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                float camZ = -Camera.main.transform.position.z; // distance from camera to world z=0 plane
                Vector3 mp3 = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camZ));
                mouseWorldPos2 = new Vector2(mp3.x, mp3.y);
            }
            else
            {
                Vector3 mp3 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
                mouseWorldPos2 = new Vector2(mp3.x, mp3.y);
            }

            // Direction from spawn to mouse (2D)
            Vector2 dir = (mouseWorldPos2 - spawnPos);
            if (dir.sqrMagnitude < 0.001f)
            {
                // If mouse is too close to spawn, default to player's forward/up
                dir = Vector2.up;
            }
            dir = dir.normalized;

            // Apply upward bias from GrenadeData if present, otherwise use handler's upwardBias
            float useUpBias = chosenType != null ? chosenType.upwardBias : upwardBias;
            dir = (dir + Vector2.up * useUpBias).normalized;

            // Use the grenade-specific throwForce if available, otherwise fallback to PlayerGrenadeHandler.throwForce
            float appliedForce = (chosenType != null ? chosenType.throwForce : throwForce);

            // Ensure Rigidbody2D is dynamic and simulated, then set velocity directly for deterministic throw behaviour
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.linearVelocity = dir * appliedForce;

            // Optional: add a small random angular spin to make throw look organic
            rb.angularVelocity = UnityEngine.Random.Range(-360f, 360f);

            if (debugLogs) Debug.Log($"Set grenade velocity to {rb.linearVelocity} (dir {dir}, appliedForce {appliedForce}, rb mass {rb.mass})");
            Debug.DrawRay(spawnPos3, new Vector3(dir.x, dir.y, 0f) * 1.5f, Color.green, 2f);
        }
        else
        {
            Debug.LogWarning("PlayerGrenadeHandler: grenadePrefab missing Rigidbody2D.");
        }
    }
}