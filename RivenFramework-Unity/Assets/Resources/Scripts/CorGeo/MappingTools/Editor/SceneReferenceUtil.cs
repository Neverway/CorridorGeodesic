using System.IO;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;


public static class SceneReferenceUtil
{
    private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private const string SceneAssetFieldName = "sceneAsset";
    private const string SceneNameFieldName = "sceneName";

    public static string GetScenePath(SceneReference _level)
    {
        Object sceneAsset = GetSceneAsset(_level);
        if (sceneAsset == null) return null;

        string path = AssetDatabase.GetAssetPath(sceneAsset);
        return string.IsNullOrEmpty(path) ? null : path;
    }

    public static string GetDisplayName(SceneReference _level)
    {
        //if (_level == null) return "(empty)";

        string path = GetScenePath(_level);
        if (!string.IsNullOrEmpty(path))
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        string cachedName = GetSceneName(_level);
        return string.IsNullOrEmpty(cachedName) ? "(missing scene)" : cachedName;
    }

    public static Object GetSceneAsset(SceneReference _level)
    {
        //if (_level == null) return null;
        FieldInfo field = typeof(SceneReference).GetField(SceneAssetFieldName, FieldFlags);
        return field?.GetValue(_level) as Object;
    }

    public static string GetSceneName(SceneReference _level)
    {
        //if (_level == null) return null;
        FieldInfo field = typeof(SceneReference).GetField(SceneNameFieldName, FieldFlags);
        return field?.GetValue(_level) as string;
    }

    public static SceneReference CreateFromSceneAsset(Object _sceneAsset)
    {
        SceneReference reference = (SceneReference)System.Activator.CreateInstance(typeof(SceneReference), nonPublic: true);

        FieldInfo assetField = typeof(SceneReference).GetField(SceneAssetFieldName, FieldFlags);
        assetField?.SetValue(reference, _sceneAsset);

        FieldInfo nameField = typeof(SceneReference).GetField(SceneNameFieldName, FieldFlags);
        if (nameField != null && _sceneAsset != null)
        {
            nameField.SetValue(reference, _sceneAsset.name);
        }

        return reference;
    }
}