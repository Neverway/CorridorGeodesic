using Sabresaurus.SabreCSG;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CSGForceRebuildOnUndo : MonoBehaviour
{
    public CSGModel csgModel;
    public bool buildInBackgroundThread = false;
#if UNITY_EDITOR
    private void OnEnable()
    {
        csgModel = GetComponent<CSGModel>();
        Undo.undoRedoPerformed += OnUndoOrRedo;
    }
    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoOrRedo;
    }

    private void OnUndoOrRedo()
    {
        csgModel.Build(true, buildInBackgroundThread);
    }
#endif
}
