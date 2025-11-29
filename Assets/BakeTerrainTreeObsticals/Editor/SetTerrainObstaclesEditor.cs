using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using System.Collections.Generic;

public class SetTerrainObstaclesEditor : EditorWindow
{
    [MenuItem("Tools/Set Terrain Tree Obstacles")]
    public static void ShowWindow()
    {
        GetWindow<SetTerrainObstaclesEditor>("Set Tree Terrain Obstacles");
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Tree Terrain Obstacles", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Bake Tree Obstacles", GUILayout.Height(40)))
        {
            BakeObstacles();
        }
    }

    private void BakeObstacles()
    {
        // 1. Check Terrain
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("ERROR: No Active Terrain found! Click on your Terrain in the scene to make it active.");
            return;
        }

        TerrainData data = terrain.terrainData;
        if (data.treeInstances.Length == 0)
        {
            Debug.LogError("ERROR: Terrain found, but it has 0 painted trees. Are you using standard GameObjects instead of Painted Terrain Trees?");
            return;
        }

        // 2. Prepare Parent Object
        string parentName = "Tree_Obstacles";
        GameObject parentObj = GameObject.Find(parentName);
        if (parentObj != null) DestroyImmediate(parentObj);
        parentObj = new GameObject(parentName);

        int bakedCount = 0;
        int skippedCount = 0;

        // 3. Loop through trees
        foreach (TreeInstance tree in data.treeInstances)
        {
            // Get the prefab
            GameObject prefab = data.treePrototypes[tree.prototypeIndex].prefab;

            // CHECK: Does prefab have a collider?
            CapsuleCollider prefabCollider = prefab.GetComponent<CapsuleCollider>();

            if (prefabCollider == null)
            {
                // Log strictly once to avoid spamming 1000 errors
                if (skippedCount == 0)
                    Debug.LogError($"ERROR: Tree Prefab '{prefab.name}' does NOT have a Capsule Collider! Please open the prefab and add one.");

                skippedCount++;
                continue;
            }

            // Calculate Position
            Vector3 position = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            // Create Obstacle
            GameObject obstacle = new GameObject("TreeObstacle");
            obstacle.transform.position = position;
            obstacle.transform.parent = parentObj.transform;

            NavMeshObstacle navObstacle = obstacle.AddComponent<NavMeshObstacle>();
            navObstacle.shape = NavMeshObstacleShape.Capsule;

            float scale = tree.widthScale;
            navObstacle.center = prefabCollider.center;
            navObstacle.radius = prefabCollider.radius * scale;
            navObstacle.height = prefabCollider.height * tree.heightScale;

            navObstacle.carving = true;
            navObstacle.carveOnlyStationary = true;

            bakedCount++;
        }

        // 4. Final Report
        if (bakedCount > 0)
        {
            Debug.Log($"<color=green>SUCCESS: Baked {bakedCount} trees.</color> Now click 'Bake' on your NavMeshSurface.");
        }

        if (skippedCount > 0)
        {
            Debug.LogError($"FAILED: Skipped {skippedCount} trees because their Prefabs were missing Capsule Colliders.");
        }
    }
}