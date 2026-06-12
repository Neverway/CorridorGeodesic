//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
// 
// Notes
// DDA is a line drawing algorithm, it stands for Digital Differential Analyzer
// It's essentially just a really fancy way to say
// "Simple function that gives me a passable approximation of a line translated to a grid of pixels"
// It is one of the simple methods to get translate a ray to a grid position,
// and it has some issues and quirks due to rounding,
// but I think the rounding errors actually end up help to keep it from over culling?
// This page explains how it works pretty well:
// https://www.geeksforgeeks.org/computer-graphics/dda-line-generation-algorithm-computer-graphics/
// ~Liz
// 
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class VoxOcclusionCuller : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Header("Culling Settings")]
    [Tooltip("The camera to use for occlusion checks (usually the main camera)")]
    public Camera cullingCamera;
    
    [Tooltip("How many props to check per frame (lower = better performance, higher = more responsive culling)")]
    public int propsCheckedPerFrame = 50;
    
    [Tooltip("How often to update occlusion (in seconds). Lower = more responsive but more expensive")]
    public float updateInterval = 0.1f;
    
    [Tooltip("Maximum distance to check for occlusion. Props beyond this are always rendered")]
    public float maxCullingDistance = 100f;
    
    [Header("Debug")]
    [Tooltip("Show debug rays for occlusion checks")]
    public bool showDebugRays = false;
    
    [Tooltip("Show statistics in console")]
    public bool showStatistics = false;
    

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<Vox_CullableActor> registeredActors = new List<Vox_CullableActor>();
    private int currentCheckIndex = 0;
    private float timeSinceLastUpdate = 0f;
    private int totalActors = 0;
    private int culledActors = 0;
    private int visibleActors = 0;
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public RiftContext riftContext;
    // A reference to the corgeo actor component on the camera so that we can tell what space the camera is in when starting a Vcast
    private CorGeo_Actor cameraActor; 


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        // Default to main camera if not set
        if (cullingCamera == null)
        {
            var localPlayer = GameInstance.Get<GI_PawnManager>().localPlayerCharacter;
            if (localPlayer) cullingCamera = localPlayer.GetComponentInChildren<Camera>();
            if (cullingCamera) cameraActor = cullingCamera.GetComponentInParent<CorGeo_Actor>();
        }

        // Get a reference to the rift context (It's like a crappy linker to connect to the rift manager)
        riftContext = GameInstance.Get<RiftContext>();
        
        // Find all actors in the scene
        RegisterAllActors();
        
        Debug.Log($"VoxelOcclusionCuller initialized with {registeredActors.Count} actors");
    }

    private void Update()
    {
        if (!cullingCamera)
        {
            var localPlayer = GameInstance.Get<GI_PawnManager>().localPlayerCharacter;
            if (localPlayer) cullingCamera = localPlayer.GetComponentInChildren<Camera>();
        }
        if (!cullingCamera || !VoxWorldManager.Instance) return;
        
        
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateOcclusion();
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void RegisterAllActors()
    {
        registeredActors.Clear();
        foreach (Vox_CullableActor _actor in FindObjectsOfType<Vox_CullableActor>())
        {
            RegisterActor(_actor);
        }

        totalActors = registeredActors.Count;
    }

    public void RegisterActor(Vox_CullableActor _actor)
    {
        if (!registeredActors.Contains(_actor))
        {
            registeredActors.Add(_actor);
            totalActors = registeredActors.Count;
        }
    }

    public void UnregisterActor(Vox_CullableActor _actor)
    {
        registeredActors.Remove(_actor);
        totalActors = registeredActors.Count;
    }

    private void UpdateOcclusion()
    {
        if (registeredActors.Count == 0) return;

        int propsCheckedThisFrame = 0;
        culledActors = 0;
        visibleActors = 0;

        while (propsCheckedThisFrame < propsCheckedPerFrame && registeredActors.Count > 0)
        {
            if (currentCheckIndex >= registeredActors.Count) currentCheckIndex = 0;

            Vox_CullableActor actor = registeredActors[currentCheckIndex];

            if (actor is null)
            {
                registeredActors.RemoveAt(currentCheckIndex);
                continue;
            }

            bool isOccluded = IsOccluded(actor);

            foreach (var renderer in actor.cullableRenderers)
            {
                if (renderer) renderer.enabled = !isOccluded;
            }

            if (isOccluded) culledActors++;
            else visibleActors++;

            currentCheckIndex++;
            propsCheckedThisFrame++;
        }

        if (showStatistics && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Occlusion Stats] Total: {totalActors}, Visible: {visibleActors}, Culled: {culledActors}");
        }
    }

    private bool IsOccluded(Vox_CullableActor _actor)
    {
        Vector3 cameraPosition = cullingCamera.transform.position;

        foreach (Renderer renderer in _actor.cullableRenderers)
        {
            if (!renderer) continue;

            Bounds bounds = renderer.bounds;

            float distance = Vector3.Distance(cameraPosition, bounds.center);
            if (distance > maxCullingDistance) return false;

            Vector3[] samplePoints = GetBoundsSamplePoints(bounds, cameraPosition);

            foreach (Vector3 samplePoint in samplePoints)
            {
                Vector3 direction = (samplePoint - cameraPosition).normalized;
                float sampleDistance = Vector3.Distance(cameraPosition, samplePoint);

                bool hitVoxel = VoxelRaycast(cameraPosition, direction, sampleDistance, out float hitDistance);

                // A visible point on the renderer was found, so don't cull it
                if (!hitVoxel || hitDistance >= sampleDistance - 0.1f)
                {
                    if (showDebugRays)
                    {
                        Debug.DrawLine(cameraPosition, samplePoint, Color.green, updateInterval);
                    }

                    return false;
                }

                if (showDebugRays)
                {
                    Debug.DrawRay(cameraPosition, direction * hitDistance, Color.red, updateInterval);
                    Debug.DrawRay(cameraPosition+direction * hitDistance, direction * (sampleDistance - hitDistance), Color.yellow, updateInterval);
                }
            }
        }

        // Every sample point was occluded, Cull! That! Prooooooooop!
        return true;
    }

    private Vector3[] GetBoundsSamplePoints(Bounds _bounds, Vector3 _cameraPosition)
    {
        Vector3 center  = _bounds.center;
        Vector3 extents = _bounds.extents;
        Vector3 toCamera = (_cameraPosition - center).normalized;
 
        List<Vector3> points = new List<Vector3>(5) { center };
 
        float absX = Mathf.Abs(toCamera.x);
        float absY = Mathf.Abs(toCamera.y);
        float absZ = Mathf.Abs(toCamera.z);
 
        if (absX > absY && absX > absZ)
        {
            float x = toCamera.x > 0 ? extents.x : -extents.x;
            points.Add(center + new Vector3(x, extents.y, extents.z));
            points.Add(center + new Vector3(x, extents.y, -extents.z));
            points.Add(center + new Vector3(x, -extents.y, extents.z));
            points.Add(center + new Vector3(x, -extents.y, -extents.z));
        }
        else if (absY > absZ)
        {
            float y = toCamera.y > 0 ? extents.y : -extents.y;
            points.Add(center + new Vector3(extents.x, y, extents.z));
            points.Add(center + new Vector3(extents.x, y, -extents.z));
            points.Add(center + new Vector3(-extents.x, y, extents.z));
            points.Add(center + new Vector3(-extents.x, y, -extents.z));
        }
        else
        {
            float z = toCamera.z > 0 ? extents.z : -extents.z;
            points.Add(center + new Vector3(extents.x, extents.y, z));
            points.Add(center + new Vector3(extents.x, -extents.y, z));
            points.Add(center + new Vector3(-extents.x, extents.y, z));
            points.Add(center + new Vector3(-extents.x, -extents.y, z));
        }
 
        return points.ToArray();

    }

    private bool VoxelRaycast(Vector3 _origin, Vector3 _direction, float _maxDistance, out float _hitDistance)
    {
        _hitDistance = _maxDistance;

        if (riftContext == null || !riftContext.IsRiftActive)
        {
            return DDA(_origin, _direction, _maxDistance, 0f, out _hitDistance);
        }

        // Determine the space the ray starts in
        RiftSpace cameraSpace = (cameraActor != null) ? cameraActor.riftSpace : RiftSpace.A;

        // Find where the ray intersects each rift plane
        float tA = RayPlaneIntersect(_origin, _direction, riftContext.PlaneA);
        float tB = RayPlaneIntersect(_origin, _direction, riftContext.PlaneB);

        // Clamp plane intersections to the ray length and discard any that aren't in that range
        if (tA > _maxDistance) tA = -1f;
        if (tB > _maxDistance) tB = -1f;

        // Split the Vcast ray into parts based on each plane intersection
        List<float> boundaries = new List<float>() { 0, _maxDistance };
        if (tA > 0f) boundaries.Add(tA);
        if (tB > 0f) boundaries.Add(tB);
        boundaries.Sort();

        // Process each Vcast line segment
        for (int i = 0; i < boundaries.Count - 1; i++)
        {
            float segStart = boundaries[i];
            float segEnd = boundaries[i + 1];
            if (segEnd <= segStart) continue;
            
            float segMid = (segStart + segEnd) * 0.5f;
            Vector3 segMidWorld = _origin + _direction * segMid;
            RiftSpace segmentSpace = GetSpaceAtPoint(segMidWorld);

            Vector3 segOriginWorld = _origin + _direction * segStart;
            Vector3 segOriginVox;
            Vector3 segDirVox;
            float segLenVox;

            switch (segmentSpace)
            {
                case RiftSpace.A:
                    segOriginVox = segOriginWorld;
                    segDirVox = _direction;
                    segLenVox = segEnd - segStart;
                    break;
                case RiftSpace.NULLSpace:
                    segOriginVox = InverseScalePoint(segOriginWorld, riftContext.NSpaceScalePivot, riftContext.NSpaceScale);
                    segDirVox = _direction / riftContext.NSpaceScale;
                    segLenVox = (segEnd - segStart) / riftContext.NSpaceScale;
                    break;
                case RiftSpace.B:
                    segOriginVox = segOriginWorld - riftContext.BSpaceShift;
                    segDirVox = _direction;
                    segLenVox = segEnd - segStart;
                    break;
                default:
                    continue;
            }

            float segHit;
            if (DDA(segOriginVox, segDirVox.normalized, segLenVox, 0f, out segHit))
            {
                float worldHit = (segmentSpace == RiftSpace.NULLSpace) ? segStart + segHit * riftContext.NSpaceScale : segStart + segHit;
                _hitDistance = worldHit;
                return true;
            }
        }

        return false;
    }

    private bool DDA(Vector3 _rayOriginWorld, Vector3 _direction, float _maxDistance, float _tOffset, out float _hitDistance)
    {
        _hitDistance = _maxDistance;

        float scale = VoxWorldManager.Instance.voxelScale;
        
        // Convert to voxel grid coords
        Vector3 pos = _rayOriginWorld / scale;
        Vector3Int voxel = new Vector3Int(
            Mathf.FloorToInt(pos.x),
            Mathf.FloorToInt(pos.y),
            Mathf.FloorToInt(pos.z));

        Vector3Int step = new Vector3Int(
            _direction.x >= 0f ? 1 : -1,
            _direction.y >= 0f ? 1 : -1,
            _direction.z >= 0f ? 1 : -1);

        float dx = _direction.x == 0f ? 0.0001f : _direction.x;
        float dy = _direction.y == 0f ? 0.0001f : _direction.y;
        float dz = _direction.z == 0f ? 0.0001f : _direction.z;

        Vector3 deltaDist = new Vector3(
            Mathf.Abs(1f / dx),
            Mathf.Abs(1f / dy),
            Mathf.Abs(1f / dz));
        
        Vector3 sideDist = new Vector3(
            step.x > 0 ? (voxel.x + 1 - pos.x) * deltaDist.x : (pos.x - voxel.x) * deltaDist.x,
            step.y > 0 ? (voxel.y + 1 - pos.y) * deltaDist.y : (pos.y - voxel.y) * deltaDist.y,
            step.z > 0 ? (voxel.z + 1 - pos.z) * deltaDist.z : (pos.z - voxel.z) * deltaDist.z);

        float maxDistVoxels = _maxDistance / scale;
        float traveledVoxels = 0;
        int maxIterations = Mathf.CeilToInt(maxDistVoxels * 2f) + 1;

        for (int i = 0; i < maxIterations && traveledVoxels < maxDistVoxels; i++)
        {
            // We always have to advance first so that we don't get false results on the first voxel
            if (sideDist.x < sideDist.y)
            {
                if (sideDist.x < sideDist.z)
                {
                    traveledVoxels = sideDist.x;
                    sideDist.x += deltaDist.x;
                    voxel.x += step.x;
                }
                else
                {
                    traveledVoxels = sideDist.z;
                    sideDist.z += deltaDist.z;
                    voxel.z += step.z;
                }
            }
            else
            {
                if (sideDist.y < sideDist.z)
                {
                    traveledVoxels = sideDist.y;
                    sideDist.y += deltaDist.y;
                    voxel.y += step.y;
                }
                else
                {
                    traveledVoxels = sideDist.z;
                    sideDist.z += deltaDist.z;
                    voxel.z += step.z;
                }
            }
            
            // After teh first iteration we can start checking the voxels
            if (IsVoxelSolid(voxel))
            {
                _hitDistance = traveledVoxels * scale;
                return true;
            }
        }

        return false;
    }

    private bool IsVoxelSolid(Vector3Int _voxelPosition)
    {
        if (VoxWorldManager.Instance.useProgressiveChunkGeneration)
        {
            Vector3Int chunkIndex = new Vector3Int(
                Mathf.FloorToInt((float)(_voxelPosition.x - VoxWorldManager.Instance.minPosition.x) / VoxWorldManager.Instance.chunkSize.x),
                Mathf.FloorToInt((float)(_voxelPosition.y - VoxWorldManager.Instance.minPosition.y) / VoxWorldManager.Instance.chunkSize.y),
                Mathf.FloorToInt((float)(_voxelPosition.z - VoxWorldManager.Instance.minPosition.z) / VoxWorldManager.Instance.chunkSize.z));

            VoxContainer chunk = VoxWorldManager.Instance.GetChunk(chunkIndex);
            if (chunk)
                return chunk[new Vector3(_voxelPosition.x, _voxelPosition.y, _voxelPosition.z)].isSolid;
            return false;
        }
        else
        {
            VoxContainer container = VoxWorldManager.Instance.GetMainContainer();
            if (container)
                return container[new Vector3(_voxelPosition.x, _voxelPosition.y, _voxelPosition.z)].isSolid;
            return false;
        }
    }

    private static float RayPlaneIntersect(Vector3 _origin, Vector3 _direction, Plane _plane)
    {
        float denom = Vector3.Dot(_direction, _plane.normal);
        if (Mathf.Abs(denom) < 1e-6f) return -1f;

        float t = -(Vector3.Dot(_plane.normal, _origin) + _plane.distance) / denom;
        return t >= 0f ? t : -1f;
    }

    private static Vector3 InverseScalePoint(Vector3 _point, Vector3 _pivot, float _scale)
    {
        if (_scale <= 0f) _scale = 0.0001f;
        return _pivot + (_point - _pivot) / _scale;
    }

    private static void Swap(ref float _a, ref float _b)
    {
        // This is apparently known as a deconstruction method,
        // it's just a simple way to say, this sets values actually = these values
        (_a, _b) = (_b, _a);
    }

    private RiftSpace GetSpaceAtPoint(Vector3 _point)
    {
        if (riftContext.PlaneA.GetSide(_point)) return RiftSpace.A;
        if (riftContext.PlaneB.GetSide(_point)) return RiftSpace.B;
        return RiftSpace.NULLSpace;
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    public void ForceUpdate()
    {
        currentCheckIndex = 0;

        foreach (Vox_CullableActor _actor in registeredActors)
        {
            if (_actor == null) continue;
            bool isOccluded = IsOccluded(_actor);
            Renderer renderer = _actor.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = !isOccluded;
        }
    }

    #endregion
}