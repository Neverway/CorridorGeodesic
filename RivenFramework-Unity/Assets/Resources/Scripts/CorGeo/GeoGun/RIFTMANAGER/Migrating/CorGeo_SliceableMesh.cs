//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M. & Connorses
//
// Contributors
//  Errynei
//
// Notes: This code was "Super Expertly Adapted" from the source code created by
//          @DitzelGames on YouTube. (See source)
//          Also thanks to Connorses for helping fix the bridgeMeshGaps function
//          and for putting up with my crazed rambling about polygons. ~Liz
//
// Notes (Rework 1): The code was rewritten by Connorses to use the BzMeshslicer instead of the custom system ~Liz
//
// Notes (Rework 2): I have rewritten the code to work with the ground up rebuild of the project. It currently does not 
//          Handel any of the logic for supporting sliceable trigger volumes as I don't fully understand that system
//          yet. ~Liz
//      
// Source: https://www.youtube.com/watch?v=VwGiwDLQ40A
//
//====================================================================================================================//

using System.Collections.Generic;
using UnityEngine;
using BzKovSoft.ObjectSlicer;
using System;
using System.Collections;
using System.Threading.Tasks;
using RivenFramework;
using Sabresaurus.SabreCSG;
using ErryLib.MonoTasks;

/// <summary>
/// Added to meshes to allow them to be sliced by the geogun
/// </summary>
[RequireComponent (typeof (BzSliceableObject))]
public class CorGeo_SliceableMesh : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("If enabled, lasers are reflected off this mesh")]
    public bool isReflective;
    
    [Tooltip("Used to identify when a slice operation is being performed")]
    public bool isSliceInProgress;
    [Tooltip("Use to identify when a mesh has been sliced by a plane")]
    public bool isSlicedByPlane;
    [Tooltip("Used to identify cut chunks that will be removed when undoing a cut")]
    public bool isClone;
    [Tooltip("Used during space assignment for the space controller dictionary")]
    public RiftSpace riftSpace;
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("The BZSlicer script that actually cuts the mesh")]
    private BzSliceableObject slicer;
    [Tooltip("The data that the BZSlicer returns when cutting the mesh")]
    private IBzMeshSlicer sliceData;
    [Tooltip("Reference to the riftManager so the cut meshes can sort themselves into the manager's correct space lists")]
    private RiftManager riftManager;
    [Tooltip("Reference to the mesh renderer so the intersection util can quickly check its overlaps with rift planes")]
    [HideInInspector] public MeshRenderer meshRenderer;
    [Tooltip("The history of how this object has been cut, used for undoing cuts when a rift is destroyed")]
    private Stack<UndoSliceState> sliceHistory = new();
    
    public struct SliceResultChunks
    {
        public bool isSliced;
        public CorGeo_SliceableMesh positiveChunk;
        public CorGeo_SliceableMesh negativeChunk;

        public void FinalizeResult()
        {
            if (isSliced)
            {
                positiveChunk.FinishSlice();
                negativeChunk.FinishSlice();
            }
            else
            {
                negativeChunk.FinishSlice();
            }
        }
    }
    
    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        slicer = GetComponent<BzSliceableObject>();
        riftManager = GameInstance.Get<RiftManager>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (!slicer.defaultSliceMaterial)
        {
            slicer.defaultSliceMaterial = riftManager.nullSpaceMaterial;
        }
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Save the state of the mesh prior to being cut
    /// </summary>
    private void SaveUndoSnapshot()
    {
        UndoSliceState state = new UndoSliceState();

        var meshFilter = GetComponent<MeshFilter>();
        state.originalMesh = Instantiate(meshFilter.sharedMesh);
        
        var meshRenderer = GetComponent<MeshRenderer>();
        state.materials = meshRenderer.materials;

        var colliders = GetComponents<MeshCollider>();
        print($"Found {colliders.Length} MeshColliders");
        state.colliders = new UndoSliceState.ColliderData[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            print($"Registered collider {colliders[i]}");
            state.colliders[i] = new UndoSliceState.ColliderData
            {
                mesh = colliders[i].sharedMesh != null ? Instantiate(colliders[i].sharedMesh) : null,
                convex = colliders[i].convex,
                isTrigger = colliders[i].isTrigger,
            };
        }
        
        state.transformData.position = transform.position;
        state.transformData.rotation = transform.rotation;
        state.transformData.scale = transform.localScale;
        
        sliceHistory.Push(state);
    }

    /// <summary>
    /// Attempts to cut this mesh, and any of its subsequent slice chunks, across the rift planes
    /// </summary>
    private async void AttemptSliceRiftPlanes()
    {
        // Mark the start of a slice operation (this will get set false when we are done (unless there is a critical failure (which happens a lot (sry)))) ~Liz
        isSliceInProgress = true;
        // Mark everything as a clone (this will be set to false for the original that we keep later)
        //isClone = true;
        // Store a reference to the original object that holds the slice history
        var originalObject = this;
        
        // DO THE SLICEY THING!!!
        var sliceResultOfAPlane = await AttemptSlice(RiftManager.cutPlaneA);
        await For.NextFrame; // THIS IS VERY IMPORTANT TO AVOID ASYNC SLICE COLLISIONS (THANK YOU ERRYNEIIIIIII)
        var sliceResultOfBPlane = await sliceResultOfAPlane.negativeChunk.AttemptSlice(RiftManager.cutPlaneB);

        // Find the actual original object
        CorGeo_SliceableMesh nonClone = null;
        
        
        // Check A slice results
        if (sliceResultOfAPlane.isSliced)
        {
            // Check if the positive chunk is the original
            if (sliceResultOfAPlane.positiveChunk == originalObject)
            {
                nonClone = sliceResultOfAPlane.positiveChunk;
            }
            // Otherwise check the negative side results
            else if (sliceResultOfBPlane.isSliced)
            {
                // One of these must be the original
                if (sliceResultOfBPlane.positiveChunk == originalObject)
                    nonClone = sliceResultOfBPlane.positiveChunk;
                else if (sliceResultOfBPlane.negativeChunk == originalObject)
                    nonClone = sliceResultOfBPlane.negativeChunk;
            }
            else
            {
                // B plane didn't slice, so negative chunk from A is the original
                if (sliceResultOfAPlane.negativeChunk == originalObject)
                    nonClone = sliceResultOfAPlane.negativeChunk;
            }
        }
        // A plane missed, Check B slice results
        else if (sliceResultOfBPlane.isSliced)
        {
            if (sliceResultOfBPlane.positiveChunk == originalObject)
                nonClone = sliceResultOfBPlane.positiveChunk;
            else if (sliceResultOfBPlane.negativeChunk == originalObject)
                nonClone = sliceResultOfBPlane.negativeChunk;
        }
        // Nothing was sliced, so the original was not changed
        else
        {
            nonClone = originalObject;
        }
        
        
        
        
        // Mark everything else as clones
        if (sliceResultOfAPlane.isSliced)
        {
            sliceResultOfAPlane.positiveChunk.isClone = (sliceResultOfAPlane.positiveChunk != nonClone);
            sliceResultOfAPlane.negativeChunk.isClone = (sliceResultOfAPlane.negativeChunk != nonClone);
        }
        if (sliceResultOfBPlane.isSliced)
        {
            sliceResultOfBPlane.positiveChunk.isClone = (sliceResultOfBPlane.positiveChunk != nonClone);
            sliceResultOfBPlane.negativeChunk.isClone = (sliceResultOfBPlane.negativeChunk != nonClone);
        }
        
        // Remove the non-clone from cut meshes list
        if (nonClone != null && riftManager.geometryHandler.cutMeshes.Contains(nonClone.gameObject))
        {
            riftManager.geometryHandler.cutMeshes.Remove(nonClone.gameObject);
        }
        
        // Ensure convex and mark the slice operation as completed for all chunks
        sliceResultOfAPlane.FinalizeResult();
        sliceResultOfBPlane.FinalizeResult();
    }
     
    /// <summary>
    /// Attempts to cut this mesh across a single plane
    /// </summary>
    /// <param name="_cutPlane">The plane to attempt to cut across</param>
    /// <returns>Returns result of the mesh slice</returns>
    public async Task<SliceResultChunks> AttemptSlice(Plane _cutPlane)
    {
        // Clones get AttemptSlice called before start or awake, so we have to force get components here
        //slicer = GetComponent<BzSliceableObject>();
        //sliceData = GetComponent<IBzMeshSlicer>();
        //riftManager = GameInstance.Get<RiftManager>();

        slicer.asynchronously = true;
        
        // Attempt to slice Plane
        var sliceResult = await slicer.SliceAsync(_cutPlane, slicer);

        return ProcessSliceResult(sliceResult);
    }
    
    private SliceResultChunks ProcessSliceResult(BzSliceTryResult sliceResult)
    {
        SliceResultChunks result = new ();
        
        result.isSliced = sliceResult.sliced;

        // Slice was a miss, just do some bull shit
        if (!result.isSliced)
        {
            result.negativeChunk = this;
            return result;
        }
        
        // Find the chunk that hasn't been cut yet
        foreach (var cutChunk in sliceResult.resultObjects)
        {
            riftManager.geometryHandler.cutMeshes.Add(cutChunk.gameObject);
            
            var chunkSliceable = cutChunk.gameObject.GetComponent<CorGeo_SliceableMesh>();
            
            // .side returns true if the chunk is on the positive side of the plane (which in this case means that we haven't cut it yet)
            if (cutChunk.side)
            {
                result.positiveChunk = chunkSliceable;
            }
            else
            {
                result.negativeChunk = chunkSliceable;
            }
        }

        return result;
    }

    /// <summary>
    /// Mark the mesh as being done with its slice, this is fired once on each mesh chunk
    /// </summary>
    private void FinishSlice()
    {
        // Ensures this function is only fired once per mesh chunk
        if (!isSliceInProgress) return;
        
        isSliceInProgress = false;
        AssignMeshToSpaceLists();
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Saves the starting mesh state and attempts to cut the mesh across the rift planes
    /// </summary>
    public void ApplyCuts()
    {
        if (isSliceInProgress) return;
        isSlicedByPlane = true;
        SaveUndoSnapshot();
        AttemptSliceRiftPlanes();
    }

    /// <summary>
    /// Restores the saved mesh state and destroys cut mesh chunks
    /// </summary>
    public void UndoCuts()
    {
        print($"Called undo on object {gameObject.name}");
        if (sliceHistory.Count == 0)
        {
            print($"Slice history is empty on object {gameObject.name}");
            return;
        }
        isSlicedByPlane = false;

        UndoSliceState state = sliceHistory.Pop();
        
        GetComponent<MeshFilter>().sharedMesh = state.originalMesh;
        
        GetComponent<MeshRenderer>().sharedMaterials = state.materials;
      
        // Restore collider data (Curse you mesh-collider the platypus!)
        // Start with destroying all existing colliders (I can't trust them! >:{ )
        var existingColliders = GetComponents<MeshCollider>();
        foreach (var existingCollider in existingColliders)
        {
            Destroy(existingCollider);
        }
        // Now actually reconstruct the original collider from the save data
        for (int i = 0; i < state.colliders.Length; i++)
        {
            var colliderData = state.colliders[i];
            var newCollider = gameObject.AddComponent<MeshCollider>();
            newCollider.sharedMesh = colliderData.mesh;
            newCollider.convex = colliderData.convex;
            newCollider.isTrigger = colliderData.isTrigger;
        }
        
        
        
        transform.position = state.transformData.position;
        transform.rotation = state.transformData.rotation;
        transform.localScale = state.transformData.scale;

        foreach (var clone in riftManager.geometryHandler.cutMeshes)
        {
            print($"Destroying clone '{clone.gameObject.name}'");
            Destroy(clone);
        }
        riftManager.geometryHandler.cutMeshes.Clear();
        
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Test the zeroth vertex of the mesh to determine which side of the rift planes it falls in, then sort it into the correct space
    /// </summary>
    public void AssignMeshToSpaceLists()
    {
        if (!riftManager) riftManager = GameInstance.Get<RiftManager>();
        
        // Clear itself from old lists
        riftManager.spaceController.spaceMeshes.Remove(gameObject);
        
        // Single vertex check
        MeshFilter meshFilter = GetComponent<MeshFilter> ();
        if (!meshFilter || meshFilter.sharedMesh == null || meshFilter.mesh.vertices.Length <= 0)
        {
            Debug.LogWarning($"{name} has no mesh filter or is an empty mesh");
            return;
        }
        var vert = meshFilter.mesh.vertices[0];
        Vector3 testPoint = new Vector3(vert.x, vert.y, vert.z);
        Vector3 worldPoint = transform.TransformPoint(testPoint);
        
        // Object is in A Space
        if (RiftManager.cutPlaneA.GetDistanceToPoint(worldPoint) > 0) { riftSpace = RiftSpace.A; }
        // Object is in B Space
        else if (RiftManager.cutPlaneB.GetDistanceToPoint(worldPoint) > 0) { riftSpace = RiftSpace.B; }
        // Object is in NULL Space
        else { riftSpace = RiftSpace.NULLSpace; }
        
        // Store in space meshes list
        riftManager.spaceController.spaceMeshes.Add(gameObject, riftSpace);
    }

    #endregion
}

[Todo("Need to add support for other collider types, currently only works with mesh colliders", TodoSeverity.Major, Owner = "liz")]
struct UndoSliceState
{
    public Mesh originalMesh;
    public Material[] materials;
    public ColliderData[] colliders;
    public MeshTransformData transformData;

    public struct MeshTransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public struct ColliderData
    {
        public Mesh mesh;
        public bool convex;
        public bool isTrigger;
    }
}