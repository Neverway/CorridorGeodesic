
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(GI_LevelSelect))]
public class GI_LevelSelectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GI_LevelSelect levelSelect = (GI_LevelSelect)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);
        DrawDefaultInspector();
        
        List<string> missingLevels = GetMissingLevelPaths(levelSelect);
        
        if (missingLevels.Count > 0)
        {
            EditorGUILayout.HelpBox($"{missingLevels.Count} level(s) have not been added to the build settings!", MessageType.Warning);

            if (GUILayout.Button("Add All Levels To Build Settings", GUILayout.Height(30)))
            {
                AddLevelsToBuildSettings(levelSelect);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("All levels are in the build settings", MessageType.Info);
        }
    }
    
    private List<string> GetMissingLevelPaths(GI_LevelSelect levelSelect)
    {
        List<string> missing = new List<string>();
        if (levelSelect.levels == null) return missing;

        HashSet<string> buildScenePaths = new HashSet<string>(EditorBuildSettings.scenes.Select(s => s.path));

        foreach (var level in levelSelect.levels)
        {
            if (level.targetScene == null) continue;

            string path = AssetDatabase.GetAssetPath(level.targetScene);
            if (!string.IsNullOrEmpty(path) && !buildScenePaths.Contains(path))
            {
                missing.Add(path);
            }
        }

        return missing;
    }

    private void AddLevelsToBuildSettings(GI_LevelSelect levelSelect)
    {
        if (levelSelect.levels == null) return;

        List<string> listedPaths = new List<string>();
        foreach (var level in levelSelect.levels)
        {
            if (level.targetScene == null) continue;

            string path = AssetDatabase.GetAssetPath(level.targetScene);
            if (!string.IsNullOrEmpty(path) && !listedPaths.Contains(path))
            {
                listedPaths.Add(path);
            }
        }

        if (listedPaths.Count == 0) return;

        HashSet<string> listedSet = new HashSet<string>(listedPaths);

        List<EditorBuildSettingsScene> keptScenes = EditorBuildSettings.scenes.Where(s => !listedSet.Contains(s.path)).ToList();

        foreach (string path in listedPaths)
        {
            keptScenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = keptScenes.ToArray();

        Debug.Log($"[GI_LevelSelect] Added/reordered {listedPaths.Count} level(s) in build settings");
    }
}
#endif