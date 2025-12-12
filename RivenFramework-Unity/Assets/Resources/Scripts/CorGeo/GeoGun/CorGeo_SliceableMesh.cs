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
using RivenFramework;

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
    private GI_RiftManager riftManager;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        slicer = GetComponent<BzSliceableObject>();
        riftManager = GameInstance.Get<GI_RiftManager>();
        if (!slicer.defaultSliceMaterial)
        {
            slicer.defaultSliceMaterial = riftManager.nullSpaceMaterial;
        }
        EnsureNonConvexWhenCloned();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Slices the mesh if it intersects with either of the rift planes, then sorts the pieces into the correct space
    /// This should only ever be called on clones of a mesh to avoid destroying the original!
    /// </summary>
    private async void AttemptSlice(GameObject _originalMesh)
    {
        // PERFORMED ON CLONES
        if (!isClone)
        {
            print("Attempted to slice an original mesh, this is strange....");
            return;
        }
        
        
        // This will stay false if the attempt to slice fails
        // (aka the objects is not intersecting with a rift cut plane)
        isSlicedByPlane = false;
        
        // Clones get AttemptSlice called before start or awake, so we have to force get components here
        slicer = GetComponent<BzSliceableObject>();
        sliceData = GetComponent<IBzMeshSlicer>();
        riftManager = GameInstance.Get<GI_RiftManager>();
        
        // Mark the start of a slice operation (this will get set false when we are done (unless there is a critical failure (which happens a lot (sry)))) ~Liz
        isSliceInProgress = true;

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
                            */

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
                    */

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

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Begins the process of duplicating the mesh and attempting to slice it
    /// </summary>
    public void ApplyCuts()
    {
        // Exit if this is a clone
        if (isClone) return;
        
        // Make a clone of this mesh to be cut
        CorGeo_SliceableMesh corGeoSliceableMeshClone = Instantiate(this, transform.position, transform.rotation);
        corGeoSliceableMeshClone.isClone = true;
        corGeoSliceableMeshClone.name = $"[CUT] {name}";

        // Slice the clone ONLY!!! Never slice the original objects!!
        corGeoSliceableMeshClone.AttemptSlice(gameObject);


        /*
        if (gameObject.name.StartsWith("[CUT]"))
        {
            //Debug.LogError("TRYING TO CUT " + gameObject.name + " BUT ITS ALREADY CUT: Maybe make sure read/write is enabled for this model?");
            return;
        }

        if (partsReference != null)
        {
            partsReference.DoReset();
        }
        Mesh_Slicable sliceThis = Instantiate (this, transform.position, transform.rotation);
        sliceThis.gameObject.name = $"[CUT] {name}";
        isCut = false;
        Alt_Item_Geodesic_Utility_GeoGun.originalSliceableObjects.Add(this);
        homePosition = transform.position;
        homeScale = transform.localScale;
        homeParent = transform.parent;
        homeRotation = transform.rotation;
        sliceThis.SliceClone (gameObject);*/
    }

    public void AssignMeshToSpaceLists()
    {
        if (!riftManager) riftManager = GameInstance.Get<GI_RiftManager>();
        
        MeshFilter meshFilter = GetComponent<MeshFilter> ();
        
        if (meshFilter && meshFilter.mesh.vertices.Length > 0)
        {
            var vert = meshFilter.mesh.vertices[0];
            Vector3 testPoint = new Vector3 (vert.x, vert.y, vert.z);
            Vector3 worldPoint = transform.TransformPoint (testPoint);
            // Object is in A Space
            if (GI_RiftManager.planeA.GetDistanceToPoint (worldPoint) < 0)
            {
                // Set the original to correct space
                riftManager.spaceAMeshes.Add(gameObject);
                space = Space.A;
                assignedCount++;
            }
            // Object is in B Space
            else if (GI_RiftManager.planeB.GetDistanceToPoint (worldPoint) < 0)
            {
                // Set the original to correct space
                riftManager.spaceBMeshes.Add(gameObject);
                space = Space.B;
                assignedCount++;
            }
            // Object is in NULL Space
            else
            {
                // Set the original to correct space
                riftManager.spaceNullMeshes.Add(gameObject);
                space = Space.NULL;
                assignedCount++;
            }
        }
        else
        {
            print($"Wtf? {gameObject.name} didn't have mesh filter");
        }
    }

    #endregion
}
