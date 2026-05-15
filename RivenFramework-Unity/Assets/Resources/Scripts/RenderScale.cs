using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class RenderScale : MonoBehaviour
{
    public bool enabled = true;
    [Range(0.1f, 1f)] public float scale = 0.5f;
    public FilterMode filterMode = FilterMode.Bilinear;

    Camera _cam;
    Camera _blitCam;
    RenderTexture _rt;

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        CreateBlitCamera();
        UpdateRT();
    }

    void OnDisable() => Cleanup();

    void LateUpdate()
    {
        if (!enabled) return;
        int w = Mathf.Max(1, Mathf.RoundToInt(Screen.width  * scale));
        int h = Mathf.Max(1, Mathf.RoundToInt(Screen.height * scale));
        if (_rt == null || _rt.width != w || _rt.height != h) UpdateRT();

        _blitCam.enabled = enabled;
        _cam.targetTexture = enabled ? _rt : null;
    }

    void CreateBlitCamera()
    {
        var go = new GameObject("__BlitCam__") { hideFlags = HideFlags.HideAndDontSave };
        _blitCam = go.AddComponent<Camera>();
        _blitCam.clearFlags = CameraClearFlags.Nothing;
        _blitCam.cullingMask = 0;
        _blitCam.depth = _cam.depth + 1;
        _blitCam.allowHDR = false;
        _blitCam.allowMSAA = false;
        _blitCam.useOcclusionCulling = false;
        go.AddComponent<BlitToScreen>().Init(_rt, filterMode);
    }

    void UpdateRT()
    {
        if (_rt != null) { _rt.Release(); DestroyImmediate(_rt); }
        int w = Mathf.Max(1, Mathf.RoundToInt(Screen.width  * scale));
        int h = Mathf.Max(1, Mathf.RoundToInt(Screen.height * scale));
        _rt = new RenderTexture(w, h, 24,
            _cam.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32);
        _rt.filterMode = filterMode;
        _cam.targetTexture = _rt;
        if (_blitCam != null)
            _blitCam.GetComponent<BlitToScreen>().Init(_rt, filterMode);
    }

    void Cleanup()
    {
        if (_cam != null) _cam.targetTexture = null;
        if (_blitCam != null) DestroyImmediate(_blitCam.gameObject);
        if (_rt != null) { _rt.Release(); DestroyImmediate(_rt); _rt = null; }
    }
}

public class BlitToScreen : MonoBehaviour
{
    RenderTexture _rt;
    FilterMode _filterMode;

    public void Init(RenderTexture rt, FilterMode filterMode)
    {
        _rt = rt;
        _filterMode = filterMode;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_rt != null) Graphics.Blit(_rt, dest);
        else Graphics.Blit(src, dest);
    }
}