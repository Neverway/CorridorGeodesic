//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TextEventEditorWindow : EditorWindow
{
    /*-----[ State ]--------------------------------------------------------------------------------------------------*/
    private Func_TextEvent targetComponent;
    private SerializedObject serializedTarget;
    private SerializedProperty textEventProp;

    private Vector2 scrollPos;
    private int selectedFrame = -1;
    private bool framesListFolded = false;

    /*-----[ Static Opener ]------------------------------------------------------------------------------------------*/
    [MenuItem("Neverway/Text Event Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<TextEventEditorWindow>("Text Event Editor");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }

    public static void OpenWithTarget(Func_TextEvent target)
    {
        var window = GetWindow<TextEventEditorWindow>("Text Event Editor");
        window.minSize = new Vector2(500, 400);
        window.SetTarget(target);
        window.Show();
    }

    /*-----[ Unity Callbacks ]----------------------------------------------------------------------------------------*/
    private void OnSelectionChange()
    {
        // Auto-bind when user selects a GameObject with Func_TextEvent
        if (Selection.activeGameObject == null) return;
        var found = Selection.activeGameObject.GetComponent<Func_TextEvent>();
        if (found != null) SetTarget(found);
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (targetComponent == null)
        {
            EditorGUILayout.HelpBox("Select a GameObject with Func_TextEvent, or use Neverway > Text Event Editor and pick a target below.", MessageType.Info);
            DrawTargetPicker();
            return;
        }

        serializedTarget.Update();

        EditorGUILayout.Space(4);
        DrawTargetPicker();
        EditorGUILayout.Space(6);

        var textEvent = targetComponent.textEvent;
        if (textEvent == null)
        {
            EditorGUILayout.HelpBox("The TextEvent on this component is null.", MessageType.Warning);
            serializedTarget.ApplyModifiedProperties();
            return;
        }

        // ---- Two-column layout ----
        EditorGUILayout.BeginHorizontal();
        DrawFrameList(textEvent);
        DrawFrameDetail(textEvent);
        EditorGUILayout.EndHorizontal();

        serializedTarget.ApplyModifiedProperties();
    }

    /*-----[ Toolbar ]------------------------------------------------------------------------------------------------*/
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Text Event Editor", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();

        if (targetComponent != null)
        {
            if (GUILayout.Button("Add Frame", EditorStyles.toolbarButton))
                AddFrame();

            if (GUILayout.Button("Clear All", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Clear Frames", "Remove all frames from this TextEvent?", "Yes", "Cancel"))
                    ClearAllFrames();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /*-----[ Target Picker ]------------------------------------------------------------------------------------------*/
    private void DrawTargetPicker()
    {
        EditorGUI.BeginChangeCheck();
        var newTarget = (Func_TextEvent)EditorGUILayout.ObjectField(
            "Target Component", targetComponent, typeof(Func_TextEvent), true);
        if (EditorGUI.EndChangeCheck()) SetTarget(newTarget);
    }

    /*-----[ Frame List (left panel) ]--------------------------------------------------------------------------------*/
    private void DrawFrameList(TextEvent textEvent)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(180));
        EditorGUILayout.LabelField("Frames", EditorStyles.boldLabel);

        if (textEvent.frames == null) textEvent.frames = new System.Collections.Generic.List<TextFrames>();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
        for (int i = 0; i < textEvent.frames.Count; i++)
        {
            var frame = textEvent.frames[i];
            bool isSelected = (i == selectedFrame);

            var style = isSelected ? GetSelectedFrameStyle() : GetNormalFrameStyle();
            string label = string.IsNullOrEmpty(frame.name)
                ? $"Frame {i + 1}"
                : $"{i + 1}. {frame.name}";

            string preview = frame.chatContent ?? "";
            if (preview.Length > 24) preview = preview.Substring(0, 24) + "…";
            string fullLabel = label + (preview.Length > 0 ? $"\n  <i>{preview}</i>" : "");

            if (GUILayout.Button(new GUIContent(fullLabel), style, GUILayout.Height(38)))
            {
                GUI.FocusControl(null);
                selectedFrame = i;
            }

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = i > 0;
            if (GUILayout.Button("▲", GUILayout.Width(26))) { MoveFrame(i, i - 1); break; }
            GUI.enabled = i < textEvent.frames.Count - 1;
            if (GUILayout.Button("▼", GUILayout.Width(26))) { MoveFrame(i, i + 1); break; }
            GUI.enabled = true;
            GUIStyle deleteStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = new Color(0.9f, 0.3f, 0.3f) } };
            if (GUILayout.Button("✕", deleteStyle, GUILayout.Width(26)))
            {
                if (EditorUtility.DisplayDialog("Delete Frame", $"Delete frame {i + 1}?", "Delete", "Cancel"))
                { DeleteFrame(i); break; }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ Add Frame"))
            AddFrame();

        EditorGUILayout.EndVertical();
    }

    /*-----[ Frame Detail (right panel) ]-----------------------------------------------------------------------------*/
    private void DrawFrameDetail(TextEvent textEvent)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));

        if (selectedFrame < 0 || selectedFrame >= textEvent.frames.Count)
        {
            EditorGUILayout.HelpBox("Select a frame on the left to edit it.", MessageType.None);
            EditorGUILayout.EndVertical();
            return;
        }

        var frame = textEvent.frames[selectedFrame];
        Undo.RecordObject(targetComponent, "Edit TextFrame");

        EditorGUILayout.LabelField($"Frame {selectedFrame + 1}", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // ---- Basic info ----
        frame.name = EditorGUILayout.TextField("Speaker Name", frame.name);
        frame.portrait = (Sprite)EditorGUILayout.ObjectField("Portrait", frame.portrait, typeof(Sprite), false);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Chat Content");
        frame.chatContent = EditorGUILayout.TextArea(frame.chatContent ?? "", GUILayout.MinHeight(80));

        // ---- Voice ----
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Voice", EditorStyles.boldLabel);
        frame.chatterVoice = (Char_ChatterVoice)EditorGUILayout.ObjectField(
            "Chatter Voice Override", frame.chatterVoice, typeof(Char_ChatterVoice), false);

        // ---- Display Settings ----
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
        frame.displayMode = (TextboxDisplayMode)EditorGUILayout.EnumPopup("Display Mode",  frame.displayMode);
        frame.SpeechStyle = (SpeechStyle)EditorGUILayout.EnumPopup("Speech Style", frame.SpeechStyle);
        frame.speechEmissionPoint = (Transform)EditorGUILayout.ObjectField("Speech Emission Point", frame.speechEmissionPoint, typeof(Transform), true);
        
        // ---- Flags ----
        EditorGUILayout.Space(4);
        frame.preventTextSkipping  = EditorGUILayout.Toggle("Prevent Skipping",     frame.preventTextSkipping);
        frame.preventTextContinuing = EditorGUILayout.Toggle("Prevent Continuing",  frame.preventTextContinuing);
        frame.autoProgressOnComplete = EditorGUILayout.Toggle("Auto Progress",      frame.autoProgressOnComplete);

        // ---- OnFrameCompleted event ----
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("On Frame Completed", EditorStyles.boldLabel);
        var framesProp = serializedTarget.FindProperty("textEvent.frames");
        if (framesProp != null && selectedFrame < framesProp.arraySize)
        {
            var frameProp = framesProp.GetArrayElementAtIndex(selectedFrame);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Markup Cheatsheet", EditorStyles.miniLabel);
        EditorGUILayout.SelectableLabel(
            "{col=key} yellow  {col=stat} orange  {col=err} red  {col=} reset\n{spd=0.05} fast  {spd=} normal\n<b>bold</b>  <i>italic</i>  <color=#hex>text</color>",
            EditorStyles.helpBox, GUILayout.Height(46));

        EditorGUILayout.EndVertical();
    }

    /*-----[ Frame Operations ]---------------------------------------------------------------------------------------*/
    private void AddFrame()
    {
        Undo.RecordObject(targetComponent, "Add TextFrame");
        targetComponent.textEvent.frames.Add(new TextFrames(""));
        selectedFrame = targetComponent.textEvent.frames.Count - 1;
        EditorUtility.SetDirty(targetComponent);
    }

    private void DeleteFrame(int index)
    {
        Undo.RecordObject(targetComponent, "Delete TextFrame");
        targetComponent.textEvent.frames.RemoveAt(index);
        if (selectedFrame >= targetComponent.textEvent.frames.Count)
            selectedFrame = targetComponent.textEvent.frames.Count - 1;
        EditorUtility.SetDirty(targetComponent);
    }

    private void MoveFrame(int from, int to)
    {
        Undo.RecordObject(targetComponent, "Reorder TextFrame");
        var frames = targetComponent.textEvent.frames;
        var temp = frames[from];
        frames[from] = frames[to];
        frames[to] = temp;
        selectedFrame = to;
        EditorUtility.SetDirty(targetComponent);
    }

    private void ClearAllFrames()
    {
        Undo.RecordObject(targetComponent, "Clear TextFrames");
        targetComponent.textEvent.frames.Clear();
        selectedFrame = -1;
        EditorUtility.SetDirty(targetComponent);
    }

    /*-----[ Helpers ]------------------------------------------------------------------------------------------------*/
    private void SetTarget(Func_TextEvent target)
    {
        targetComponent = target;
        selectedFrame = -1;
        if (target != null)
        {
            serializedTarget = new SerializedObject(target);
            textEventProp = serializedTarget.FindProperty(nameof(Func_TextEvent.textEvent));
            if (target.textEvent == null) target.textEvent = new TextEvent();
        }
        Repaint();
    }

    private static GUIStyle _selectedFrameStyle;
    private static GUIStyle GetSelectedFrameStyle()
    {
        if (_selectedFrameStyle == null)
        {
            _selectedFrameStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                normal = { background = MakeTex(2, 2, new Color(0.25f, 0.48f, 0.9f, 0.85f)) }
            };
        }
        return _selectedFrameStyle;
    }

    private static GUIStyle _normalFrameStyle;
    private static GUIStyle GetNormalFrameStyle()
    {
        if (_normalFrameStyle == null)
        {
            _normalFrameStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };
        }
        return _normalFrameStyle;
    }

    private static Texture2D MakeTex(int w, int h, Color col)
    {
        var pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        var tex = new Texture2D(w, h);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
#endif