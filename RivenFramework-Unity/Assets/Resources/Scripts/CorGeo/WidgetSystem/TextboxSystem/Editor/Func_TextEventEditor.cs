#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(Func_TextEvent))]
public class Func_TextEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(4);
        if (GUILayout.Button("Open Text Event Editor"))
            TextEventEditorWindow.OpenWithTarget((Func_TextEvent)target);
    }
}
#endif