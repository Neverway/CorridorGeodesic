using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
public class MeshSubdivider : MonoBehaviour
{
    [Range(1, 4)]
    [Tooltip("Number of subdivision passes. Each pass multiplies triangle count by 4.")]
    public int subdivisionPasses = 1;

    [Tooltip("Recalculates smooth normals after subdivision. Recommended for vertex lighting.")]
    public bool smoothNormals = true;

#if UNITY_EDITOR
    [Tooltip("Folder to save the subdivided mesh asset into (relative to Assets/).")]
    public string savePath = "Meshes/Subdivided";

    [ContextMenu("Subdivide and Save Mesh")]
    public void SubdivideAndSave()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("[MeshSubdivider] No MeshFilter or sharedMesh found.");
            return;
        }

        Mesh result = CopyMesh(mf.sharedMesh);

        for (int i = 0; i < subdivisionPasses; i++)
        {
            result = SubdivideOnce(result);
            Debug.Log($"[MeshSubdivider] Pass {i + 1} complete — " +
                      $"{result.vertexCount} verts, {result.triangles.Length / 3} tris.");
        }

        Finalise(result);

        string fullDir = $"Assets/{savePath}";
        if (!System.IO.Directory.Exists(fullDir))
            System.IO.Directory.CreateDirectory(fullDir);

        string assetPath = $"{fullDir}/{mf.sharedMesh.name}_sub{subdivisionPasses}.asset";
        AssetDatabase.CreateAsset(result, assetPath);
        AssetDatabase.SaveAssets();

        Undo.RecordObject(mf, "Subdivide Mesh");
        mf.sharedMesh = result;

        Debug.Log($"[MeshSubdivider] Saved to {assetPath} and assigned to MeshFilter.");
    }

    [ContextMenu("Preview (In-Memory, Not Saved)")]
    public void PreviewSubdivision()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Mesh result = CopyMesh(mf.sharedMesh);
        for (int i = 0; i < subdivisionPasses; i++)
            result = SubdivideOnce(result);

        Finalise(result);

        Undo.RecordObject(mf, "Preview Subdivision");
        mf.mesh = result;

        Debug.Log($"[MeshSubdivider] Preview assigned — " +
                  $"{result.vertexCount} verts, {result.triangles.Length / 3} tris. " +
                  $"Use 'Subdivide and Save' to make it permanent.");
    }

    [ContextMenu("Log Triangle / Vertex Count")]
    public void LogCounts()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;
        Mesh m = mf.sharedMesh;
        Debug.Log($"[MeshSubdivider] Current mesh — {m.vertexCount} verts, {m.triangles.Length / 3} tris.");
    }
#endif

    static Mesh SubdivideOnce(Mesh src)
    {
        Vector3[] srcVerts   = src.vertices;
        Vector3[] srcNormals = src.normals;
        Vector2[] srcUV      = src.uv;
        Vector2[] srcUV2     = src.uv2;
        Color[]   srcColors  = src.colors;
        int[]     srcTris    = src.triangles;

        bool hasNormals = srcNormals != null && srcNormals.Length == srcVerts.Length;
        bool hasUV      = srcUV      != null && srcUV.Length      == srcVerts.Length;
        bool hasUV2     = srcUV2     != null && srcUV2.Length     == srcVerts.Length;
        bool hasColors  = srcColors  != null && srcColors.Length  == srcVerts.Length;

        int triCount = srcTris.Length / 3;

        var newVerts   = new List<Vector3>(srcVerts);
        var newNormals = hasNormals ? new List<Vector3>(srcNormals) : null;
        var newUV      = hasUV     ? new List<Vector2>(srcUV)      : null;
        var newUV2     = hasUV2    ? new List<Vector2>(srcUV2)     : null;
        var newColors  = hasColors ? new List<Color>(srcColors)    : null;
        var newTris    = new List<int>(triCount * 12);

        var midpointCache = new Dictionary<long, int>();

        for (int t = 0; t < triCount; t++)
        {
            int i0 = srcTris[t * 3 + 0];
            int i1 = srcTris[t * 3 + 1];
            int i2 = srcTris[t * 3 + 2];

            int m01 = GetOrCreateMidpoint(i0, i1,
                srcVerts, srcNormals, srcUV, srcUV2, srcColors,
                hasNormals, hasUV, hasUV2, hasColors,
                newVerts, newNormals, newUV, newUV2, newColors, midpointCache);

            int m12 = GetOrCreateMidpoint(i1, i2,
                srcVerts, srcNormals, srcUV, srcUV2, srcColors,
                hasNormals, hasUV, hasUV2, hasColors,
                newVerts, newNormals, newUV, newUV2, newColors, midpointCache);

            int m20 = GetOrCreateMidpoint(i2, i0,
                srcVerts, srcNormals, srcUV, srcUV2, srcColors,
                hasNormals, hasUV, hasUV2, hasColors,
                newVerts, newNormals, newUV, newUV2, newColors, midpointCache);

            newTris.Add(i0);  newTris.Add(m01); newTris.Add(m20);
            newTris.Add(m01); newTris.Add(i1);  newTris.Add(m12);
            newTris.Add(m20); newTris.Add(m12); newTris.Add(i2);
            newTris.Add(m01); newTris.Add(m12); newTris.Add(m20);
        }

        Mesh dst = new Mesh();
        dst.name = src.name;

        if (newVerts.Count > 65535)
            dst.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        dst.vertices  = newVerts.ToArray();
        dst.triangles = newTris.ToArray();

        if (hasNormals) dst.normals = newNormals.ToArray();
        if (hasUV)      dst.uv      = newUV.ToArray();
        if (hasUV2)     dst.uv2     = newUV2.ToArray();
        if (hasColors)  dst.colors  = newColors.ToArray();

        return dst;
    }

    static int GetOrCreateMidpoint(
        int a, int b,
        Vector3[] verts, Vector3[] normals, Vector2[] uv, Vector2[] uv2, Color[] colors,
        bool hasNormals, bool hasUV, bool hasUV2, bool hasColors,
        List<Vector3> newVerts, List<Vector3> newNormals,
        List<Vector2> newUV, List<Vector2> newUV2, List<Color> newColors,
        Dictionary<long, int> cache)
    {
        long key = a < b ? ((long)a << 32) | (uint)b
                         : ((long)b << 32) | (uint)a;

        if (cache.TryGetValue(key, out int existing))
            return existing;

        int index = newVerts.Count;
        newVerts.Add((verts[a] + verts[b]) * 0.5f);

        if (hasNormals) newNormals.Add(Vector3.Normalize((normals[a] + normals[b]) * 0.5f));
        if (hasUV)      newUV.Add((uv[a]   + uv[b])   * 0.5f);
        if (hasUV2)     newUV2.Add((uv2[a]  + uv2[b])  * 0.5f);
        if (hasColors)  newColors.Add(Color.Lerp(colors[a], colors[b], 0.5f));

        cache[key] = index;
        return index;
    }


    void Finalise(Mesh m)
    {
        if (smoothNormals)
            m.RecalculateNormals();

        m.RecalculateTangents();
        m.RecalculateBounds();
        m.Optimize();
    }

    static Mesh CopyMesh(Mesh src)
    {
        Mesh copy        = new Mesh();
        copy.name        = src.name;
        copy.indexFormat = src.indexFormat;
        copy.vertices    = src.vertices;
        copy.normals     = src.normals;
        copy.uv          = src.uv;
        copy.uv2         = src.uv2;
        copy.colors      = src.colors;
        copy.tangents    = src.tangents;
        copy.triangles   = src.triangles;
        copy.subMeshCount = src.subMeshCount;

        for (int i = 0; i < src.subMeshCount; i++)
            copy.SetSubMesh(i, src.GetSubMesh(i));

        return copy;
    }
}