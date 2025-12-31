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
using System.Threading.Tasks;
using RivenFramework;
using Sabresaurus.SabreCSG;

/// <summary>
/// Added to meshes to allow them to be sliced by the geogun
/// </summary>
[RequireComponent (typeof (BzSliceableObject))]
public class CorGeo_SliceableMesh : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("Used by slice clones to identify when planes have cut them")]
    public bool isSlicedByPlane;
    [Tooltip("If enabled, lasers are reflected off this mesh")]
    public bool isReflective;

    public bool isClone;
    public bool isSliceInProgress;
    public int assignedCount;
    public Space space;
    public enum Space
    {
        none,
        A,
        B,
        NULL
    }

    public int timesSliced;
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("The BZSlicer script that actually cuts the mesh")]
    private BzSliceableObject slicer;
    [Tooltip("The data that the BZSlicer returns when cutting the mesh")]
    private IBzMeshSlicer sliceData;
    [Tooltip("Reference to the riftManager so the cut meshes can sort themselves into the manager's correct space lists")]
    private RiftManager riftManager;
    
    private Stack<UndoSliceState> sliceHistory = new();


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        slicer = GetComponent<BzSliceableObject>();
        riftManager = GameInstance.Get<RiftManager>();
        if (!slicer.defaultSliceMaterial)
        {
            slicer.defaultSliceMaterial = riftManager.nullSpaceMaterial;
        }
        EnsureNonConvexWhenCloned();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void SaveUndoSnapshot()
    {
        UndoSliceState state = new UndoSliceState();

        var meshFilter = GetComponent<MeshFilter>();
        state.originalMesh = Instantiate(meshFilter.sharedMesh);
        
        var meshRenderer = GetComponent<MeshRenderer>();
        state.materials = meshRenderer.materials;
        
        var colliders = GetComponents<MeshCollider>();
        state.colliders = new MeshCollider[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            state.colliders[i] = colliders[i];
        }
        
        state.transformData.position = transform.position;
        state.transformData.rotation = transform.rotation;
        state.transformData.scale = transform.localScale;
        
        sliceHistory.Push(state);
    }
    
    /// <summary>
    /// Slices the mesh if it intersects with either of the rift planes
    /// </summary>
    private async void AttemptSlice(GameObject _originalMesh)
    {
        // PERFORMED ON CLONES
        if (!isClone)
        {
            //print("Attempted to slice an original mesh, this is strange....");
            //return;
        }
        
        // This will stay false if the attempt to slice fails
        // (aka the objects is not intersecting with a rift cut plane)
        isSlicedByPlane = false;
        // Mark the start of a slice operation (this will get set false when we are done (unless there is a critical failure (which happens a lot (sry)))) ~Liz
        isSliceInProgress = true;
        
        // Clones get AttemptSlice called before start or awake, so we have to force get components here
        slicer = GetComponent<BzSliceableObject>();
        sliceData = GetComponent<IBzMeshSlicer>();
        riftManager = GameInstance.Get<RiftManager>();
        
        // --- PART ONE: SLICE PLANE A ---
        var sliceA = await slicer.SliceAsync(RiftManager.cutPlaneA, sliceData);
        if (sliceA.sliced)
        {
            HandleOriginal(_originalMesh);

            foreach (var sliceAResultObject in sliceA.resultObjects)
            {
                await ProcessSliceResult(sliceAResultObject, planeA: true);
            }

            FinishSlice(_originalMesh);
            return;
        }
        
        // --- PART TWO: SLICE PLANE B IF A FAILED ---
        var sliceB = await slicer.SliceAsync(GI_RiftManager.planeB, sliceData);
        if (sliceB.sliced)
        {
            HandleOriginal(_originalMesh);

            foreach (var sliceBResultObject in sliceB.resultObjects)
            {
                await ProcessSliceResult(sliceBResultObject, planeA: false);
            }

            FinishSlice(_originalMesh);
            return;
        }
        
        // --- PART THREE: DOUBLE SLICE FAILURE, CLEANUP ---
        Destroy(gameObject);
        isSliceInProgress = false;
        _originalMesh.GetComponent<CorGeo_SliceableMesh>().isSliceInProgress = false;


        /*
        // --- PART ONE ---
        // Attempt to slice across the rift's 'A Plane'
        var planeASliceResult = await slicer.SliceAsync(GI_RiftManager.planeA, sliceData);
        if (planeASliceResult.sliced)
        {
            // Hide the original and set the isSlicedByPlane to true
            riftManager.hiddenOriginalMeshes.Add(_originalMesh);
            isSlicedByPlane = true;

            // Since the slice across the 'A Plane' was successful, attempt to slice the new meshes across the rift's 'B Plane'
            foreach (var newACutMesh in planeASliceResult.resultObjects)
            {
                var newACutMeshObject = newACutMesh.gameObject;
                var newACutMeshSliceable = newACutMeshObject.GetComponent<CorGeo_SliceableMesh>();

                // Make sure the part knows that it's a cut mesh
                newACutMeshObject.gameObject.SetActive (false);
                riftManager.meshesToActivate.Add (newACutMeshObject);
                newACutMeshSliceable.isSlicedByPlane = true;
                newACutMeshSliceable.isSliceInProgress = true;

                // Only cut the new meshes on the positive side (the side that faces towards where the B plane should be)
                if (newACutMesh.side)
                {
                    // Slice the new objects on the positive side of the cut, this time with the B plane
                    var newACutMeshSlicer = newACutMeshObject.GetComponent<IBzMeshSlicer>();
                    var planeABSliceResult = await newACutMeshSlicer.SliceAsync(GI_RiftManager.planeB);
                    if (planeABSliceResult.sliced)
                    {
                        foreach (var newBCutMesh in planeABSliceResult.resultObjects)
                        {
                            var newBCutMeshObject = newBCutMesh.gameObject;
                            var newBCutMeshSliceable = newBCutMeshObject.GetComponent<CorGeo_SliceableMesh>();
                            var newBCutMeshSlicer = newBCutMeshObject.GetComponent<IBzMeshSlicer>();

                            // Make sure the part knows that it's a cut mesh
                            newBCutMeshObject.SetActive(false);
                            riftManager.meshesToActivate.Add (newBCutMeshObject);
                            newBCutMeshSliceable.isSlicedByPlane = true;
                            newBCutMeshSliceable.isSliceInProgress = true;

                            /* OLD CODE FOR 'PartsReference' STUFF (Which is used for allowing sliceable logic volumes)
                            // I have not fully re-created this in the new system yet as I don't fully understand it ~Liz
                            if (partsReference)
                            {
                                partsReference.AddSlice(objMesh2);
                                // objMesh2 is now called newBCutMeshSliceable
                            }


                            // Sort A meshes into rift manager list
                            // This... did not work. I don't know why.
                            // It does work when I move it down to the else statement below though! ~Liz
                            //CleanupExtraMeshColliders(newBCutMesh.gameObject);
                            //riftManager.spaceAMeshes.Add(newACutMesh.gameObject);
                            //newACutMesh.gameObject.name = $"{name} [A]";

                            // Sort NULL meshes into rift manager list
                            if (newBCutMesh.side)
                            {
                                //riftManager.spaceNullMeshes.Add(newBCutMesh.gameObject);
                                newBCutMeshObject.name = $"{name} [AB-NULL]";
                                newBCutMeshSliceable.isSliceInProgress = false;
                            }

                            // Finally sort B meshes into rift manager list
                            else
                            {
                                //riftManager.spaceBMeshes.Add(newBCutMesh.gameObject);
                                newBCutMeshObject.name = $"{name} [AB]";
                                newBCutMeshSliceable.isSliceInProgress = false;
                            }
                        }
                    }
                    // If B plane slice fails, the rest of the mesh was in null space
                    else
                    {
                        //obj2.gameObject.transform.SetParent(Alt_Item_Geodesic_Utility_GeoGun.planeBMeshes.transform);
                        //riftManager.spaceNullMeshes.Add(newACutMesh.gameObject);
                        newACutMeshObject.name = $"{name} [A-NULL]";
                        newACutMeshSliceable.isSliceInProgress = false;
                    }
                }
                // Sort A meshes into rift manager list
                else
                {
                    //riftManager.spaceAMeshes.Add(newACutMesh.gameObject);
                    newACutMeshObject.name = $"{name} [A]";
                }
            }
        }

        // --- PART TWO ---
        // Even if A plane fails, still attempt to slice across the rift's 'B Plane'
        else
        {
            var planeBSliceResult = await slicer.SliceAsync(GI_RiftManager.planeB, sliceData);
            if (planeBSliceResult.sliced)
            {
                // Add the original and set the isSlicedByPlane to true
                riftManager.hiddenOriginalMeshes.Add(_originalMesh);
                isSlicedByPlane = true;

                foreach (var newBCutMesh in planeBSliceResult.resultObjects)
                {
                    var newBCutMeshObject = newBCutMesh.gameObject;
                    var newBCutMeshSliceable = newBCutMeshObject.GetComponent<CorGeo_SliceableMesh>();

                    // Make sure the part knows that it's a cut mesh
                    newBCutMeshObject.SetActive(false);
                    riftManager.meshesToActivate.Add (newBCutMeshObject);
                    newBCutMeshSliceable.isSlicedByPlane = true;
                    newBCutMeshSliceable.isSliceInProgress = true;




                    /* OLD CODE FOR 'PartsReference' STUFF (Which is used for allowing sliceable logic volumes)
                    // I have not fully re-created this in the new system yet as I don't fully understand it ~Liz
                    if (partsReference)
                    {
                        partsReference.AddSlice(objMesh2);
                        // objMesh3 is now called newBCutMeshSliceable
                    }


                    // Sort A meshes into rift manager list
                    // This... did not work. I don't know why.
                    // It does work when I move it down to the else statement below though! ~Liz
                    //CleanupExtraMeshColliders(newBCutMesh.gameObject);
                    //riftManager.spaceAMeshes.Add(newACutMesh.gameObject);
                    //newACutMesh.gameObject.name = $"{name} [A]";

                    // Sort NULL meshes into rift manager list
                    if (newBCutMesh.side)
                    {
                        //riftManager.spaceNullMeshes.Add(newBCutMesh.gameObject);
                        newBCutMeshObject.name = $"{name} [B-NULL]";
                        newBCutMeshSliceable.isSliceInProgress = false;
                    }

                    // Finally sort B meshes into rift manager list
                    else
                    {
                        //riftManager.spaceBMeshes.Add(newBCutMesh.gameObject);
                        newBCutMeshObject.name = $"{name} [B]";
                        newBCutMeshSliceable.isSliceInProgress = false;
                    }
                }
            }
        }

        // Clean up any random non-cut meshes
        if (!isSlicedByPlane)
        {
            Destroy(gameObject);
        }

        isSliceInProgress = false;
        _originalMesh.GetComponent<CorGeo_SliceableMesh>().isSliceInProgress = false;
        */
    }
    
    /// <summary>
    /// Used to ensure that level meshes don't push objects out of them when they are cut
    /// </summary>
    private void EnsureNonConvexWhenCloned()
    {
        foreach (var meshCollider in gameObject.GetComponents<MeshCollider>())
        {
            if (!meshCollider.isTrigger)
            {
                meshCollider.convex = false;
            }
            meshCollider.sharedMesh = meshCollider.sharedMesh;
        }
    }
    
    // [--- Attempt Slice Helper Functions ---]
    private void HandleOriginal(GameObject _originalMesh)
    {
        if (!riftManager.geometryHandler.originalMeshesToHide.Contains(_originalMesh)) riftManager.geometryHandler.originalMeshesToHide.Add(_originalMesh);
        isSlicedByPlane = true;
    }
    
    

    private async Task ProcessSliceResult(BzSlicerTryResultObject resultObj, bool planeA)
    {
        GameObject obj = resultObj.gameObject;
        var s = obj.GetComponent<CorGeo_SliceableMesh>();

        obj.SetActive(false);
        riftManager.geometryHandler.cutMeshesToActivate.Add(obj);

        s.isClone = true;
        s.isSlicedByPlane = true;
        s.isSliceInProgress = true;

        // If this slice should continue to B-plane
        if (resultObj.side)
        {
            var slicer2 = obj.GetComponent<IBzMeshSlicer>();
            var sub = await slicer2.SliceAsync(planeA ? GI_RiftManager.planeB : GI_RiftManager.planeA);

            if (sub.sliced)
            {
                foreach (var subObj in sub.resultObjects)
                    await ProcessSecondarySlice(subObj, planeA);
            }
            else
            {
                obj.name = planeA ? $"{name} [A-NULL]" : $"{name} [B-NULL]";
                s.isSliceInProgress = false;
            }
        }
        else
        {
            obj.name = planeA ? $"{name} [A]" : $"{name} [B]";
            s.isSliceInProgress = false;
        }
    }

    private async Task ProcessSecondarySlice(BzSlicerTryResultObject resultObj, bool planeA)
    {
        GameObject obj = resultObj.gameObject;
        var s = obj.GetComponent<CorGeo_SliceableMesh>();

        obj.SetActive(false);
        riftManager.geometryHandler.cutMeshesToActivate.Add(obj);

        s.isClone = true;
        s.isSlicedByPlane = true;
        s.isSliceInProgress = true;

        if (resultObj.side)
            obj.name = planeA ? $"{name} [AB-NULL]" : $"{name} [BA-NULL]";
        else
            obj.name = planeA ? $"{name} [AB]" : $"{name} [BA]";

        s.isSliceInProgress = false;
    }

    private void FinishSlice(GameObject original)
    {
        isSliceInProgress = false;
        original.GetComponent<CorGeo_SliceableMesh>().isSliceInProgress = false;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Begins the process of duplicating the mesh and attempting to slice it
    /// </summary>
    public void ApplyCuts()
    {
        /*
        // Exit if this is a clone
        if (isClone) return;
        
        // Make a clone of this mesh to be cut
        CorGeo_SliceableMesh corGeoSliceableMeshClone = Instantiate(this, transform.position, transform.rotation);
        corGeoSliceableMeshClone.isClone = true;
        corGeoSliceableMeshClone.name = $"[CUT] {name}";

        // Slice the clone ONLY!!! Never slice the original objects!!
        corGeoSliceableMeshClone.AttemptSlice(gameObject);*/
        
        if (isSliceInProgress) return;
        SaveUndoSnapshot();
        AttemptSlice(gameObject);
    }

    public void UndoCuts()
    {
        if (sliceHistory.Count == 0) return;

        UndoSliceState state = sliceHistory.Pop();
        
        GetComponent<MeshFilter>().sharedMesh = state.originalMesh;
        
        GetComponent<MeshRenderer>().sharedMaterials = state.materials;

        var colliders = GetComponents<MeshCollider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].sharedMesh = state.colliders[i].sharedMesh;
            colliders[i].convex = false;
            colliders[i].isTrigger = state.colliders[i].isTrigger;
        }
        
        transform.position = state.transformData.position;
        transform.rotation = state.transformData.rotation;
        transform.localScale = state.transformData.scale;

        foreach (var clone in riftManager.geometryHandler.cutMeshesToActivate)
        {
            Destroy(clone);
        }
        riftManager.geometryHandler.cutMeshesToActivate.Clear();
        
        gameObject.SetActive(true);
    }

    public void AssignMeshToSpaceLists()
    {
        if (!riftManager) riftManager = GameInstance.Get<RiftManager>();
        
        // Clear itself from old lists
        riftManager.spaceController.spaceMeshesA.Remove(gameObject);
        riftManager.spaceController.spaceMeshesB.Remove(gameObject);
        riftManager.spaceController.spaceMeshesNull.Remove(gameObject);
        
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
        if (GI_RiftManager.planeA.GetDistanceToPoint(worldPoint) < 0)
        {
            // Set the original to correct space
            riftManager.spaceController.spaceMeshesA.Add(gameObject);
            space = Space.A;
        }
        // Object is in B Space
        else if (GI_RiftManager.planeB.GetDistanceToPoint(worldPoint) < 0)
        {
            // Set the original to correct space
            riftManager.spaceController.spaceMeshesB.Add(gameObject);
            space = Space.B;
        }
        // Object is in NULL Space
        else
        {
            // Set the original to correct space
            riftManager.spaceController.spaceMeshesNull.Add(gameObject);
            space = Space.NULL;
        }
        assignedCount++;
    }

    #endregion
}

struct UndoSliceState
{
    public Mesh originalMesh;
    public Material[] materials;
    public MeshCollider[] colliders;
    public MeshTransformData transformData;

    public struct MeshTransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}