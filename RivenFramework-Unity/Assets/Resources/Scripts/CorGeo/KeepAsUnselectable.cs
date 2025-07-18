using UnityEngine;

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
        if (isSelectable)
            SceneVisibilityManager.instance.EnablePicking(gameObject, includeChildren);
        else
            SceneVisibilityManager.instance.DisablePicking(gameObject, includeChildren);
    }
#endif
}
