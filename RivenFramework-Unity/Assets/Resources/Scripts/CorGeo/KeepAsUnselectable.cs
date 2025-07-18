using UnityEngine;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class KeepAsUnselectable : MonoBehaviour
{
#if UNITY_EDITOR
    public bool isSelectable = false;
    public bool includeChildren = true;

    void Update()
    {
        bool isCurrentlySelectable = false;
        if (includeChildren)
            isCurrentlySelectable = !SceneVisibilityManager.instance.IsPickingDisabledOnAllDescendants(gameObject);
        else
            isCurrentlySelectable = !SceneVisibilityManager.instance.IsPickingDisabled(gameObject);

        if (isSelectable ^ isCurrentlySelectable)
            SceneVisibilityManager.instance.EnablePicking(gameObject, includeChildren);
        else
            SceneVisibilityManager.instance.DisablePicking(gameObject, includeChildren);

    }
#endif
}
