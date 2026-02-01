using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxWorldManager : MonoBehaviour
{
    public Material worldMaterial;
    public VoxelColor[] worldColors;
    private VoxContainer voxelContainer;
    
    [Header("Voxelization Settings")]
    [Tooltip("Which layers should be detected as solid objects for voxelization")]
    public LayerMask voxelDetectionLayers = -1;
    [Tooltip("Minimum size threshold for overlap detection (smaller = more accurate but slower)")]
    public float overlapCheckSize = 0.4f;
    
    [Header("Debugging")]
    public Vector3Int minPosition = new Vector3Int(0,0,0);
    public Vector3Int maxPosition = new Vector3Int(32,32,32);
    [Tooltip("In a standard voxel system there is no need to generate a voxel for air, enable this if you want to generate air blocks")]
    public bool doNotSkipGeneratingAirBlocks = false;
    [Tooltip("In a standard voxel system you don't usually want to draw faces that can't be seen or are covering each other, enable this if you want to generate touching faces")]
    public bool doNotSkipGeneratingBackfaces = false;
    
    private static VoxWorldManager _instance;
    public static VoxWorldManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<VoxWorldManager>();
            return _instance;
        }
    }

    public WorldSettings worldSettings;
    public static WorldSettings WorldSettings;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_instance != null)
        {
            if (_instance != this) Destroy(this);
        }
        else
        {
            _instance = this;
        }

        WorldSettings = worldSettings;
        GameObject containerObject = new GameObject("Voxel Container");
        containerObject.transform.SetParent(transform);
        voxelContainer = containerObject.AddComponent<VoxContainer>();

        voxelContainer.doNotSkipGeneratingAirBlocks = doNotSkipGeneratingAirBlocks;
        voxelContainer.doNotSkipGeneratingBackfaces = doNotSkipGeneratingBackfaces;
        voxelContainer.Initialize(worldMaterial, Vector3.zero);

        GenerateTerrain();
        
        voxelContainer.GenerateMesh();
        voxelContainer.UploadMesh();
    }
    
    void GenerateTerrain()
    {        voxelContainer.ClearData();
        
        // Calculate the center point and half extents for the overlap check
        Vector3 halfExtents = Vector3.one * (overlapCheckSize / 2f);
        
        int voxelsDetected = 0;
        
        for (int x = minPosition.x; x < maxPosition.x; x++)
        {
            for (int z = minPosition.z; z < maxPosition.z; z++)
            {
                for (int y = minPosition.y; y < maxPosition.y; y++)
                {
                    Vector3 voxelPosition = new Vector3(x, y, z);
                    
                    // Add 0.5 to check the center of the voxel cube
                    Vector3 checkPosition = voxelPosition + Vector3.one * 0.5f;
                    
                    // Check if there's a collider at this position
                    Collider[] colliders = Physics.OverlapBox(checkPosition, halfExtents, Quaternion.identity, voxelDetectionLayers);
                    
                    if (colliders.Length > 0)
                    {
                        // Ignore the voxel container's own collider
                        bool foundValidCollider = false;
                        foreach (Collider col in colliders)
                        {
                            if (col.GetComponent<VoxContainer>() == null)
                            {
                                foundValidCollider = true;
                                break;
                            }
                        }
                        
                        if (foundValidCollider)
                        {
                            voxelContainer[voxelPosition] = new Voxel() { ID = 1 };
                            voxelsDetected++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Voxelization complete! Detected {voxelsDetected} solid voxels.");
    }

    [ContextMenu("RegenerateVoxels")]
    public void RegenerateVoxels()
    {
        GenerateTerrain();
        voxelContainer.GenerateMesh();
        voxelContainer.UploadMesh();
    }
}

[System.Serializable]
public class WorldSettings
{
    public int containerSize = 16;
    public int maxHeight = 128;
}