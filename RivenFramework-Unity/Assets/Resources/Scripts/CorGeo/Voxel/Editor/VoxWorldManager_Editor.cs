// Written by Liz M.
// Editor script for VoxWorldManager to allow pre-baking voxels in the editor

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using Unity.Collections;
using Unity.Jobs;

[CustomEditor(typeof(VoxWorldManager))]
public class VoxWorldManagerEditor : Editor
{
    private VoxWorldManager manager;
    private bool isBaking = false;
    private float bakingProgress = 0f;
    private string bakingStatus = "";
    
    private BoxBoundsHandle _boundsHandle = new BoxBoundsHandle();
    
    void OnEnable()
    {
        manager = (VoxWorldManager)target;
    }
    
    private void OnSceneGUI()
    {
        using (new Handles.DrawingScope(new Color(0.3f, 0.8f, 1f, 0.9f), manager.transform.localToWorldMatrix))
        {
            _boundsHandle.SetColor(new Color(0.3f, 0.8f, 1f, 0.9f));
 
            Vector3 min = (Vector3)manager.minPosition * manager.voxelScale;
            Vector3 max = (Vector3)manager.maxPosition * manager.voxelScale;
 
            _boundsHandle.center = (min + max) * 0.5f;
            _boundsHandle.size   = max - min;
 
            EditorGUI.BeginChangeCheck();
            _boundsHandle.DrawHandle();
 
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(manager, "Resize Vox Bounds");
 
                Vector3 newMin = _boundsHandle.center - _boundsHandle.size * 0.5f;
                Vector3 newMax = _boundsHandle.center + _boundsHandle.size * 0.5f;
 
                manager.minPosition = new Vector3Int(
                    Mathf.RoundToInt(newMin.x / manager.voxelScale),
                    Mathf.RoundToInt(newMin.y / manager.voxelScale),
                    Mathf.RoundToInt(newMin.z / manager.voxelScale));
 
                manager.maxPosition = new Vector3Int(
                    Mathf.RoundToInt(newMax.x / manager.voxelScale),
                    Mathf.RoundToInt(newMax.y / manager.voxelScale),
                    Mathf.RoundToInt(newMax.z / manager.voxelScale));
 
                manager.minPosition = Vector3Int.Min(manager.minPosition, manager.maxPosition - Vector3Int.one);
                manager.maxPosition = Vector3Int.Max(manager.maxPosition, manager.minPosition + Vector3Int.one);
 
                EditorUtility.SetDirty(manager);
            }
        }
    }

    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Voxel Baking", EditorStyles.boldLabel);
        
        using (new EditorGUI.DisabledScope(isBaking))
        {
            if (GUILayout.Button("Bake Voxels in Editor", GUILayout.Height(30)))
            {
                BakeVoxels();
            }
        }
        
        if (GUILayout.Button("Clear All Voxels", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear Voxels", "Are you sure you want to clear all baked voxels?", "Yes", "Cancel"))
            {
                ClearVoxels();
            }
        }
        
        if (isBaking)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Baking Status:", bakingStatus);
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), bakingProgress, $"{Mathf.RoundToInt(bakingProgress * 100)}%");
        }
        
        EditorGUILayout.Space(10);
        ShowChunkInfo();
    }
    
    void ShowChunkInfo()
    {
        EditorGUILayout.LabelField("Current Voxel Info", EditorStyles.boldLabel);
        
        Transform chunksParent = manager.transform.Find("VoxelChunks");
        int chunkCount = 0;
        int totalVoxels = 0;
        
        if (chunksParent != null)
        {
            chunkCount = chunksParent.childCount;
            
            for (int i = 0; i < chunksParent.childCount; i++)
            {
                VoxContainer container = chunksParent.GetChild(i).GetComponent<VoxContainer>();
                if (container != null && container.containerData != null)
                {
                    totalVoxels += container.containerData.Count;
                }
            }
        }
        
        EditorGUILayout.LabelField($"Chunks: {chunkCount}");
        EditorGUILayout.LabelField($"Total Voxels: {totalVoxels}");
        
        if (chunkCount > 0)
        {
            EditorGUILayout.HelpBox("Voxels have been baked to the scene. They will persist when you save.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("No voxels baked yet. Click 'Bake Voxels in Editor' to generate.", MessageType.Warning);
        }
    }
    
    static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    string GetSceneVoxelFolder()
    {
        string scenePath = manager.gameObject.scene.path;

        if (string.IsNullOrEmpty(scenePath))
        {
            return "Assets/GeneratedVoxelData/UnsavedScene/Voxel";
        }

        string sceneDir = System.IO.Path.GetDirectoryName(scenePath).Replace("\\", "/");
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        return $"{sceneDir}/{sceneName}/Voxel";
    }

    void BakeVoxels()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorCoroutineUtility.StartCoroutine(BakeVoxelsCoroutine(), this);
        }
        else
        {
            EditorUtility.DisplayDialog("Cannot Bake", "Cannot bake voxels while in Play Mode. Exit Play Mode first.", "OK");
        }
    }

    IEnumerator BakeVoxelsCoroutine()
    {
        isBaking = true;
        bakingProgress = 0f;

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        ClearVoxels();

        string sceneVoxelFolder = GetSceneVoxelFolder();
        string meshFolder = $"{sceneVoxelFolder}/Meshes";
        string prefabFolder = $"{sceneVoxelFolder}/Prefabs";
        EnsureFolder(meshFolder);
        EnsureFolder(prefabFolder);

        Transform chunksParentTransform = manager.transform.Find("VoxelChunks");
        GameObject chunksParent;

        if (chunksParentTransform == null)
        {
            chunksParent = new GameObject("VoxelChunks");
            chunksParent.transform.SetParent(manager.transform);
            chunksParent.transform.localPosition = Vector3.zero;
        }
        else
        {
            chunksParent = chunksParentTransform.gameObject;
        }

        bakingStatus = "Finding colliders...";
        yield return null;

        Collider[] allColliders = Object.FindObjectsOfType<Collider>();
        List<Collider> validColliders = new List<Collider>();

        foreach (Collider col in allColliders)
        {
            if (((1 << col.gameObject.layer) & manager.voxelDetectionLayers) != 0)
            {
                validColliders.Add(col);
            }
        }

        Debug.Log($"Found {validColliders.Count} valid colliders to voxelize");

        if (validColliders.Count == 0)
        {
            EditorUtility.DisplayDialog("No Colliders Found", "No valid colliders found! Check your layer settings.", "OK");
            isBaking = false;
            yield break;
        }

        Vector3Int numChunks = new Vector3Int(
            Mathf.CeilToInt((float)(manager.maxPosition.x - manager.minPosition.x) / manager.chunkSize.x),
            Mathf.CeilToInt((float)(manager.maxPosition.y - manager.minPosition.y) / manager.chunkSize.y),
            Mathf.CeilToInt((float)(manager.maxPosition.z - manager.minPosition.z) / manager.chunkSize.z)
        );

        int totalChunks = numChunks.x * numChunks.y * numChunks.z;
        int chunksCompleted = 0;
        int totalVoxelsDetected = 0;

        Debug.Log($"Baking {totalChunks} chunks ({numChunks.x}x{numChunks.y}x{numChunks.z})");

        Vector3 halfExtents = Vector3.one * manager.voxelScale * (manager.overlapCheckSize / 2f);
        Collider[] colliderBuffer = new Collider[10];
        Vector3 origin = manager.transform.position;

        for (int cx = 0; cx < numChunks.x; cx++)
        {
            for (int cy = 0; cy < numChunks.y; cy++)
            {
                for (int cz = 0; cz < numChunks.z; cz++)
                {
                    bakingStatus = $"Baking chunk {chunksCompleted + 1}/{totalChunks}";
                    bakingProgress = (float)chunksCompleted / totalChunks;

                    Vector3Int chunkMin = manager.minPosition + new Vector3Int(
                        cx * manager.chunkSize.x,
                        cy * manager.chunkSize.y,
                        cz * manager.chunkSize.z
                    );

                    Vector3Int chunkMax = new Vector3Int(
                        Mathf.Min(chunkMin.x + manager.chunkSize.x, manager.maxPosition.x),
                        Mathf.Min(chunkMin.y + manager.chunkSize.y, manager.maxPosition.y),
                        Mathf.Min(chunkMin.z + manager.chunkSize.z, manager.maxPosition.z)
                    );

                    GameObject chunkObject = new GameObject($"Chunk_{cx}_{cy}_{cz}");
                    chunkObject.transform.SetParent(chunksParent.transform);
                    chunkObject.transform.localPosition = Vector3.zero;
                    VoxContainer chunk = chunkObject.AddComponent<VoxContainer>();

                    chunk.doNotSkipGeneratingAirBlocks = manager.doNotSkipGeneratingAirBlocks;
                    chunk.doNotSkipGeneratingBackfaces = manager.doNotSkipGeneratingBackfaces;
                    chunk.Initialize(manager.worldMaterial, manager.voxelScale);

                    int chunkVoxelsDetected = 0;

                    Bounds chunkBounds = new Bounds(
                        new Vector3(
                            (chunkMin.x + chunkMax.x) * 0.5f * manager.voxelScale + origin.x,
                            (chunkMin.y + chunkMax.y) * 0.5f * manager.voxelScale + origin.y,
                            (chunkMin.z + chunkMax.z) * 0.5f * manager.voxelScale + origin.z
                        ),
                        new Vector3(
                            (chunkMax.x - chunkMin.x) * manager.voxelScale,
                            (chunkMax.y - chunkMin.y) * manager.voxelScale,
                            (chunkMax.z - chunkMin.z) * manager.voxelScale
                        )
                    );
                    bool anyColliderTouchesChunk = false;
                    Vector3Int unionMin = chunkMax;
                    Vector3Int unionMax = chunkMin;

                    foreach (Collider col in validColliders)
                    {
                        if (col == null || col.GetComponent<VoxContainer>() != null) continue;
                        if (!col.bounds.Intersects(chunkBounds)) continue;

                        anyColliderTouchesChunk = true;

                        Vector3Int boundsMin = new Vector3Int(
                            Mathf.Max(chunkMin.x, Mathf.FloorToInt((col.bounds.min.x - origin.x) / manager.voxelScale)),
                            Mathf.Max(chunkMin.y, Mathf.FloorToInt((col.bounds.min.y - origin.y) / manager.voxelScale)),
                            Mathf.Max(chunkMin.z, Mathf.FloorToInt((col.bounds.min.z - origin.z) / manager.voxelScale))
                        );

                        Vector3Int boundsMax = new Vector3Int(
                            Mathf.Min(chunkMax.x, Mathf.CeilToInt((col.bounds.max.x - origin.x) / manager.voxelScale)),
                            Mathf.Min(chunkMax.y, Mathf.CeilToInt((col.bounds.max.y - origin.y) / manager.voxelScale)),
                            Mathf.Min(chunkMax.z, Mathf.CeilToInt((col.bounds.max.z - origin.z) / manager.voxelScale))
                        );

                        unionMin = Vector3Int.Min(unionMin, boundsMin);
                        unionMax = Vector3Int.Max(unionMax, boundsMax);
                    }

                    if (anyColliderTouchesChunk)
                    {
                        int sizeX = unionMax.x - unionMin.x;
                        int sizeY = unionMax.y - unionMin.y;
                        int sizeZ = unionMax.z - unionMin.z;
                        int voxelCount = sizeX * sizeY * sizeZ;

                        if (voxelCount > 0)
                        {
                            var commands = new NativeArray<OverlapBoxCommand>(voxelCount, Allocator.TempJob);
                            var results = new NativeArray<ColliderHit>(voxelCount, Allocator.TempJob);

                            QueryParameters queryParams = new QueryParameters(manager.voxelDetectionLayers, false, QueryTriggerInteraction.UseGlobal);

                            int cmdIndex = 0;
                            for (int x = unionMin.x; x < unionMax.x; x++)
                            {
                                for (int z = unionMin.z; z < unionMax.z; z++)
                                {
                                    for (int y = unionMin.y; y < unionMax.y; y++)
                                    {
                                        Vector3 voxelPosition = new Vector3(x, y, z);
                                        Vector3 checkPosition = (voxelPosition * manager.voxelScale) + Vector3.one * (manager.voxelScale * 0.5f) + origin;

                                        commands[cmdIndex] = new OverlapBoxCommand(checkPosition, halfExtents, Quaternion.identity, queryParams);
                                        cmdIndex++;
                                    }
                                }
                            }

                            JobHandle handle = OverlapBoxCommand.ScheduleBatch(commands, results, 32, 1, default);
                            handle.Complete();

                            cmdIndex = 0;
                            for (int x = unionMin.x; x < unionMax.x; x++)
                            {
                                for (int z = unionMin.z; z < unionMax.z; z++)
                                {
                                    for (int y = unionMin.y; y < unionMax.y; y++)
                                    {
                                        if (results[cmdIndex].collider != null)
                                        {
                                            chunk[new Vector3(x, y, z)] = new Voxel() { ID = 1 };
                                            chunkVoxelsDetected++;
                                        }

                                        cmdIndex++;
                                    }
                                }
                            }

                            commands.Dispose();
                            results.Dispose();
                        }
                    }

                    if (chunkVoxelsDetected > 0)
                    {
                        chunk.GenerateMesh();
                        chunk.UploadMesh();
                        totalVoxelsDetected += chunkVoxelsDetected;

                        chunk.SerializeVoxelData();

                        Mesh bakedMesh = Object.Instantiate(chunk.GeneratedMesh);
                        bakedMesh.name = chunkObject.name;
                        string meshAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{meshFolder}/{chunkObject.name}.asset");
                        AssetDatabase.CreateAsset(bakedMesh, meshAssetPath);
                        chunk.AssignExternalMesh(bakedMesh);

                        string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{prefabFolder}/{chunkObject.name}.prefab");
                        PrefabUtility.SaveAsPrefabAssetAndConnect(chunkObject, prefabPath, InteractionMode.AutomatedAction);

                        EditorUtility.SetDirty(chunkObject);
                    }
                    else
                    {
                        Object.DestroyImmediate(chunkObject);
                    }

                    chunksCompleted++;

                    if (chunksCompleted % 5 == 0)
                    {
                        yield return null;
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
 
        stopwatch.Stop();
        
        bakingProgress = 1f;
        bakingStatus = "Complete!";
        
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(chunksParent);
        
        Debug.Log($"Voxel baking complete! Total: {totalVoxelsDetected} voxels in {chunksCompleted} chunks");
        Debug.Log($"Time taken: {stopwatch.ElapsedMilliseconds}ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
        
        EditorUtility.DisplayDialog("Baking Complete!", $"Successfully baked {totalVoxelsDetected} voxels across {chunksCompleted} chunks in {stopwatch.Elapsed.TotalSeconds:F2} seconds.\n\nDon't forget to save your scene!", "OK");
        
        isBaking = false;
        
        Repaint();
    }


    void ClearVoxels()
    {
        Transform chunksParent = manager.transform.Find("VoxelChunks");
        if (chunksParent != null)
        {
            for (int i = chunksParent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(chunksParent.GetChild(i).gameObject);
            }
            
            Debug.Log("Cleared all voxel chunks");
        }

        string sceneVoxelFolder = GetSceneVoxelFolder();

        bool deletedAny = false;
        if (AssetDatabase.IsValidFolder(sceneVoxelFolder))
        {
            deletedAny = AssetDatabase.DeleteAsset(sceneVoxelFolder);
        }

        if (deletedAny)
        {
            AssetDatabase.Refresh();
            Debug.Log("Deleted generated voxel mesh/prefab assets for this scene");
        }
        
        EditorUtility.SetDirty(manager);
        Repaint();
    }
}

public static class EditorCoroutineUtility
{
    private class EditorCoroutine
    {
        private IEnumerator routine;
        
        public EditorCoroutine(IEnumerator routine)
        {
            this.routine = routine;
        }
        
        public void Start()
        {
            EditorApplication.update += Update;
        }
        
        public void Stop()
        {
            EditorApplication.update -= Update;
        }
        
        private void Update()
        {
            if (!routine.MoveNext())
            {
                Stop();
            }
        }
    }
    
    public static void StartCoroutine(IEnumerator routine, Object owner)
    {
        EditorCoroutine coroutine = new EditorCoroutine(routine);
        coroutine.Start();
    }
}
#endif