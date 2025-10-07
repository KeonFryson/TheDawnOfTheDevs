using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class PowerUpAssetBatchCreator : EditorWindow
{
    private Type[] powerUpTypes;
    private int selectedTypeIndex = 0;
    private string folderPath = "Assets/Pefab/Cards/";
    private string iconFolderPath = "Assets/Sprites/PowerUpIcons/";

    // Default values for tiers
    private int minorValue = 1;
    private int majorValue = 3;
    private int ultimateValue = 7;

    [MenuItem("Tools/Create PowerUp Tiered Assets")]
    public static void ShowWindow()
    {
        GetWindow<PowerUpAssetBatchCreator>("PowerUp Asset Batch Creator");
    }

    private void OnEnable()
    {
        powerUpTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(PowerUp)) && !t.IsAbstract)
            .ToArray();
    }

    private void OnGUI()
    {
        GUILayout.Label("Select PowerUp Type", EditorStyles.boldLabel);

        if (powerUpTypes == null || powerUpTypes.Length == 0)
        {
            EditorGUILayout.HelpBox("No PowerUp types found.", MessageType.Warning);
            return;
        }

        string[] typeNames = powerUpTypes.Select(t => t.Name).ToArray();
        selectedTypeIndex = EditorGUILayout.Popup("PowerUp Type", selectedTypeIndex, typeNames);

        GUILayout.Space(10);
        folderPath = EditorGUILayout.TextField("Save Folder", folderPath);
        iconFolderPath = EditorGUILayout.TextField("Icon Folder", iconFolderPath);

        GUILayout.Space(10);
        GUILayout.Label("Tier Values (for common fields like damage/health):");
        minorValue = EditorGUILayout.IntField("Minor Value", minorValue);
        majorValue = EditorGUILayout.IntField("Major Value", majorValue);
        ultimateValue = EditorGUILayout.IntField("Ultimate Value", ultimateValue);

        GUILayout.Space(20);
        if (GUILayout.Button("Create Tiered Assets"))
        {
            CreateTieredAssets();
        }
    }

    private void CreateTieredAssets()
    {
        Type selectedType = powerUpTypes[selectedTypeIndex];

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        CreateAsset(selectedType, "Minor", PowerUpTier.Minor, minorValue);
        CreateAsset(selectedType, "Major", PowerUpTier.Major, majorValue);
        CreateAsset(selectedType, "Ultimate", PowerUpTier.Ultimate, ultimateValue);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("PowerUp Assets", "Tiered assets created!", "OK");
    }

    private void CreateAsset(Type type, string tierName, PowerUpTier tier, int value)
    {
        var asset = ScriptableObject.CreateInstance(type);

        // Set common fields
        string assetBaseName = $"{tierName}{type.Name.Replace("PowerUp", "")}";
        type.GetField("powerUpName").SetValue(asset, $"{tierName} {type.Name.Replace("PowerUp", "")}");
        type.GetField("description").SetValue(asset, $"+{value} {type.Name.Replace("PowerUp", "")}");
        type.GetField("tier").SetValue(asset, tier);

        // Try to set a value field (damageIncrease, healthIncrease, etc.)
        var valueField = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(f => f.Name.EndsWith("Increase", StringComparison.OrdinalIgnoreCase));
        if (valueField != null)
        {
            valueField.SetValue(asset, value);
        }

        // Try to assign an icon
        Sprite iconSprite = LoadIconSprite(assetBaseName);
        if (iconSprite != null)
        {
            type.GetField("icon").SetValue(asset, iconSprite);
        }

        string assetPath = Path.Combine(folderPath, assetBaseName + ".asset");
        AssetDatabase.CreateAsset(asset, assetPath);
    }

    private Sprite LoadIconSprite(string assetBaseName)
    {
        string[] guids = AssetDatabase.FindAssets(assetBaseName + " t:Sprite", new[] { iconFolderPath });
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }
}