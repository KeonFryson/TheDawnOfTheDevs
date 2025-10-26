using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TreeSpawnerEditor : EditorWindow
{
    private GameObject[] treePrefabs = new GameObject[0];
    private Transform centerTransform;
    private Vector3 customCenter = Vector3.zero;
    private float spawnRadius = 10f;
    private int spawnCount = 50;
    private float minDistance = 1f;
    private float minScale = 0.9f;
    private float maxScale = 1.3f;
    private bool randomYRotation = true;
    private bool alignToGround = true;
    private LayerMask groundLayer = ~0;
    private int seed = 0;
    private bool useSeed = false;
    private bool previewArea = true;
    private Transform parentTransform;
    private string containerName = "Spawned_Trees";
    private List<GameObject> lastSpawned = new List<GameObject>();
    private int maxAttemptsPerTree = 30;
    private bool use2D = true; // place on XY plane for 2D projects
    private bool placeOnCircumference = false; // NEW: place along the circle edge evenly

    [MenuItem("Tools/Tree Spawner")]
    public static void ShowWindow()
    {
        GetWindow<TreeSpawnerEditor>("Tree Spawner");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tree Prefabs (drag one or more)", EditorStyles.boldLabel);
        int newLen = Mathf.Max(0, EditorGUILayout.IntField("Count", treePrefabs.Length));
        if (newLen != treePrefabs.Length)
        {
            var tmp = new GameObject[newLen];
            for (int i = 0; i < Mathf.Min(newLen, treePrefabs.Length); i++) tmp[i] = treePrefabs[i];
            treePrefabs = tmp;
        }
        for (int i = 0; i < treePrefabs.Length; i++)
            treePrefabs[i] = (GameObject)EditorGUILayout.ObjectField(treePrefabs[i], typeof(GameObject), false);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Spawn Area", EditorStyles.boldLabel);
        centerTransform = (Transform)EditorGUILayout.ObjectField("Center Transform (optional)", centerTransform, typeof(Transform), true);
        customCenter = EditorGUILayout.Vector3Field("Custom Center (if no Transform)", customCenter);
        spawnRadius = EditorGUILayout.FloatField("Radius", spawnRadius);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
        spawnCount = EditorGUILayout.IntField("Spawn Count", spawnCount);
        minDistance = EditorGUILayout.FloatField("Min Distance Between Trees", minDistance);
        maxAttemptsPerTree = EditorGUILayout.IntField("Max Attempts Per Tree", maxAttemptsPerTree);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
        use2D = EditorGUILayout.Toggle("2D (XY plane)", use2D);
        placeOnCircumference = EditorGUILayout.Toggle("Place On Circumference (even)", placeOnCircumference);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale / Rotation", EditorStyles.boldLabel);
        minScale = EditorGUILayout.FloatField("Min Scale", minScale);
        maxScale = EditorGUILayout.FloatField("Max Scale", maxScale);
        randomYRotation = EditorGUILayout.Toggle("Random Rotation", randomYRotation);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grounding / Layers", EditorStyles.boldLabel);
        alignToGround = EditorGUILayout.Toggle("Align to Ground (raycast down)", alignToGround);
        groundLayer = EditorGUILayout.LayerField("Ground Layer", groundLayer);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Parent / Container", EditorStyles.boldLabel);
        parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform (optional)", parentTransform, typeof(Transform), true);
        containerName = EditorGUILayout.TextField("Container Name", containerName);

        EditorGUILayout.Space();
        useSeed = EditorGUILayout.Toggle("Use Seed", useSeed);
        if (useSeed)
        {
            seed = EditorGUILayout.IntField("Seed", seed);
        }

        EditorGUILayout.Space();
        previewArea = EditorGUILayout.Toggle("Preview Area in Scene", previewArea);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Trees"))
        {
            SpawnTrees();
        }
        if (GUILayout.Button("Clear Last Spawn"))
        {
            ClearLastSpawn();
        }
        if (GUILayout.Button("Clear All Containers"))
        {
            ClearAllContainers();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sv)
    {
        if (!previewArea) return;

        Vector3 center = centerTransform != null ? centerTransform.position : customCenter;
        Vector3 normal = use2D ? Vector3.forward : Vector3.up;
        Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        Handles.DrawSolidDisc(center, normal, spawnRadius);
        Handles.color = Color.green;
        Handles.DrawWireDisc(center, normal, spawnRadius);

        // If placing on circumference, draw markers for even positions
        if (placeOnCircumference && spawnCount > 0)
        {
            Handles.color = Color.cyan;
            for (int i = 0; i < spawnCount; i++)
            {
                float theta = (2f * Mathf.PI / spawnCount) * i;
                Vector3 p;
                if (use2D)
                    p = center + new Vector3(Mathf.Cos(theta) * spawnRadius, Mathf.Sin(theta) * spawnRadius, 0f);
                else
                    p = center + new Vector3(Mathf.Cos(theta) * spawnRadius, 0f, Mathf.Sin(theta) * spawnRadius);
                Handles.DrawSolidDisc(p, normal, Mathf.Max(0.05f, spawnRadius * 0.02f));
            }
        }
    }

    private void SpawnTrees()
    {
        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            EditorUtility.DisplayDialog("Tree Spawner", "No tree prefabs assigned.", "OK");
            return;
        }

        Vector3 center = centerTransform != null ? centerTransform.position : customCenter;

        System.Random rng = useSeed ? new System.Random(seed) : new System.Random();

        // Create container
        GameObject container;
        if (parentTransform != null)
        {
            container = new GameObject(containerName);
            Undo.RegisterCreatedObjectUndo(container, "Create Tree Container");
            container.transform.SetParent(parentTransform, true);
        }
        else
        {
            container = new GameObject(containerName);
            Undo.RegisterCreatedObjectUndo(container, "Create Tree Container");
        }

        lastSpawned.Clear();

        if (placeOnCircumference)
        {
            // Evenly place spawnCount objects on the circle circumference
            for (int i = 0; i < spawnCount; i++)
            {
                float theta = (2f * Mathf.PI / spawnCount) * i;
                Vector3 pos;
                if (use2D)
                    pos = center + new Vector3(Mathf.Cos(theta) * spawnRadius, Mathf.Sin(theta) * spawnRadius, center.z);
                else
                    pos = center + new Vector3(Mathf.Cos(theta) * spawnRadius, 0f, Mathf.Sin(theta) * spawnRadius);

                // Ground align if requested
                if (alignToGround)
                    pos = AlignToGround(pos, use2D, center);

                SpawnSinglePrefabAt(pos, rng);
            }
        }
        else
        {
            // Random-inside-circle behaviour (existing)
            int spawned = 0;
            int attempts = 0;
            while (spawned < spawnCount && attempts < spawnCount * maxAttemptsPerTree)
            {
                attempts++;
                float r = spawnRadius * Mathf.Sqrt((float)rng.NextDouble());
                float theta = (float)rng.NextDouble() * Mathf.PI * 2f;
                Vector3 pos;
                if (use2D)
                    pos = center + new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), center.z);
                else
                    pos = center + new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));

                if (alignToGround)
                    pos = AlignToGround(pos, use2D, center);
                else
                    pos.y = center.y;

                // check min distance to existing spawned
                bool tooClose = false;
                foreach (var go in lastSpawned)
                {
                    if ((go.transform.position - pos).sqrMagnitude < minDistance * minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                SpawnSinglePrefabAt(pos, rng);
                spawned++;
            }
        }

        if (lastSpawned.Count == 0)
        {
            DestroyImmediate(container);
            EditorUtility.DisplayDialog("Tree Spawner", "No trees spawned (check layers/area/min distance).", "OK");
            return;
        }

        Selection.activeGameObject = container;
        EditorUtility.DisplayDialog("Tree Spawner", $"Spawned {lastSpawned.Count} trees.", "OK");
    }

    private Vector3 AlignToGround(Vector3 pos, bool use2D, Vector3 centerFallback)
    {
        if (use2D)
        {
            Vector2 rayStart = new Vector2(pos.x, pos.y + 50f);
            RaycastHit2D hit2D = Physics2D.Raycast(rayStart, Vector2.down, 200f, groundLayer);
            if (hit2D.collider != null)
            {
                pos.y = hit2D.point.y;
            }
            else
            {
                pos.y = centerFallback.y;
            }
        }
        else
        {
            RaycastHit hit;
            Vector3 rayStart = pos + Vector3.up * 50f;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f, 1 << groundLayer))
            {
                pos.y = hit.point.y;
            }
            else
            {
                Terrain t = Terrain.activeTerrain;
                if (t != null)
                {
                    float terrainY = t.SampleHeight(pos) + t.GetPosition().y;
                    pos.y = terrainY;
                }
                else
                {
                    pos.y = centerFallback.y;
                }
            }
        }
        return pos;
    }

    private void SpawnSinglePrefabAt(Vector3 pos, System.Random rng)
    {
        // pick prefab
        GameObject prefab = treePrefabs[rng.Next(0, treePrefabs.Length)];
        if (prefab == null) return;

        // instantiate as prefab instance (preserve prefab connection)
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
        {
            instance = (GameObject)Object.Instantiate(prefab);
            instance.name = prefab.name;
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Tree");
        }
        else
        {
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Tree");
        }

        // Parent to container (if any)
        var container = GameObject.Find(containerName);
        if (container != null)
            instance.transform.SetParent(container.transform, true);

        instance.transform.position = pos;
        float s = Mathf.Lerp(minScale, maxScale, (float)rng.NextDouble());
        instance.transform.localScale = instance.transform.localScale * s;

        if (randomYRotation)
        {
            if (use2D)
                instance.transform.rotation = Quaternion.Euler(0f, 0f, (float)rng.NextDouble() * 360f);
            else
                instance.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
        }

        lastSpawned.Add(instance);
    }

    private void ClearLastSpawn()
    {
        if (lastSpawned == null || lastSpawned.Count == 0)
        {
            EditorUtility.DisplayDialog("Tree Spawner", "No last spawn to clear.", "OK");
            return;
        }

        for (int i = lastSpawned.Count - 1; i >= 0; i--)
        {
            if (lastSpawned[i] != null)
                Undo.DestroyObjectImmediate(lastSpawned[i]);
        }
        lastSpawned.Clear();
    }

    private void ClearAllContainers()
    {
        GameObject[] all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int removed = 0;
        foreach (var go in all)
        {
            if (go == null) continue;
            if (go.name == containerName)
            {
                Undo.DestroyObjectImmediate(go);
                removed++;
            }
        }
        EditorUtility.DisplayDialog("Tree Spawner", $"Removed {removed} containers named '{containerName}'.", "OK");
    }
}