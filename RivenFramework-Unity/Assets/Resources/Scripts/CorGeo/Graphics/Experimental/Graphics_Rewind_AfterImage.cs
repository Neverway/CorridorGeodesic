using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics_Rewind_AfterImage : MonoBehaviour
{
    [Header("Timing")]
    public float spawnInterval = 0.05f;

    [Header("Pool")]
    public int maxAfterImages = 10;

    [Header("Appearance")]
    public Material afterImageMaterial;
    public Color startColor = new Color(0.4f, 0.8f, 1f, 1f);
    public Color endColor = new Color(0.4f, 0.8f, 1f, 0f);
    public float startEmissionIntensity = 2f;
    public float endEmissionIntensity = 2f;
    public float ghostScaleMultiplier = 1f;

    [Header("Control")]
    public bool isEmitting = false;

    private Renderer[] sourceRenderers;
    private float timer;

    private class GhostImage
    {
        public GameObject root;
        public List<Material> materials = new List<Material>();
    }

    private readonly List<GhostImage> activeGhosts = new List<GhostImage>();

    void Start()
    {
        sourceRenderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (!isEmitting) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnAfterImage();
        }
    }

    void SpawnAfterImage()
    {
        GhostImage ghost = new GhostImage();
        ghost.root = new GameObject("AfterImage");
        ghost.root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        foreach (Renderer sourceRenderer in sourceRenderers)
        {
            if (sourceRenderer == null || !sourceRenderer.enabled) continue;

            Mesh bakedMesh;

            if (sourceRenderer is SkinnedMeshRenderer skinnedRenderer)
            {
                bakedMesh = new Mesh();
                skinnedRenderer.BakeMesh(bakedMesh);
            }
            else if (sourceRenderer is MeshRenderer)
            {
                MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null) continue;
                bakedMesh = sourceFilter.sharedMesh;
            }
            else
            {
                continue;
            }

            Transform sourceTransform = sourceRenderer.transform;

            GameObject part = new GameObject(sourceRenderer.name + "_Ghost");
            part.transform.SetParent(ghost.root.transform, false);
            part.transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
            part.transform.localScale = sourceTransform.lossyScale * ghostScaleMultiplier;

            MeshFilter mf = part.AddComponent<MeshFilter>();
            mf.mesh = bakedMesh;

            MeshRenderer mr = part.AddComponent<MeshRenderer>();

            int submeshCount = bakedMesh.subMeshCount;
            Material[] ghostMats = new Material[submeshCount];
            for (int i = 0; i < submeshCount; i++)
            {
                Material mat = new Material(afterImageMaterial);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                ghostMats[i] = mat;
                ghost.materials.Add(mat);
            }
            mr.sharedMaterials = ghostMats;
        }

        activeGhosts.Insert(0, ghost);

        if (activeGhosts.Count > maxAfterImages)
        {
            GhostImage oldest = activeGhosts[activeGhosts.Count - 1];
            activeGhosts.RemoveAt(activeGhosts.Count - 1);
            foreach (Material mat in oldest.materials) Destroy(mat);
            Destroy(oldest.root);
        }

        RecolorQueue();
    }

    void RecolorQueue()
    {
        int count = activeGhosts.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float t = (maxAfterImages > 1) ? (float)i / (maxAfterImages - 1) : 0f;
            Color c = Color.Lerp(startColor, endColor, t);
            Color emission = c * Mathf.Lerp(startEmissionIntensity, endEmissionIntensity, t);

            foreach (Material mat in activeGhosts[i].materials)
            {
                mat.SetColor("_EmissionColor", emission);
            }
        }
    }
}
