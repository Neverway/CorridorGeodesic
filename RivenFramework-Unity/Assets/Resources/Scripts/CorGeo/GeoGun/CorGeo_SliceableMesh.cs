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
        riftManager = FindObjectOfType<GI_RiftManager>();
        if (!slicer.defaultSliceMaterial)
        {
            print("Wowza!");
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
        List<GameObject> objs = new List<GameObject>();
        // This will stay false if the attempt to slice fails
        // (aka the objects is not intersecting with a rift cut plane)
        isSlicedByPlane = false;
        slicer = GetComponent<BzSliceableObject>();
        sliceData = GetComponent<IBzMeshSlicer>();
        riftManager = FindObjectOfType<GI_RiftManager>();

        // --- PART ONE ---
        // Attempt to slice across the rift's 'A Plane'
        var resultOfPlaneASlice = await slicer.SliceAsync(riftManager.planeA, sliceData);
        if (resultOfPlaneASlice.sliced)
        {
            // Hide the original and set the isSlicedByPlane to true, so we can skip Part Three
            _originalMesh.SetActive(false);
            riftManager.hiddenOriginalMeshes.Add(_originalMesh);
            isSlicedByPlane = true;
            
            // Since the slice across the 'A Plane' was successful, attempt to slice the new meshes across the rift's 'B Plane'
            foreach (var newACutMesh in resultOfPlaneASlice.resultObjects)
            {
                // Make sure the part knows that it's a cut mesh
                CorGeo_SliceableMesh newACutCorGeoSliceableMesh = newACutMesh.gameObject.GetComponent<CorGeo_SliceableMesh>();
                newACutCorGeoSliceableMesh.isSlicedByPlane = true;
                
                // Only cut the new meshes on the positive side (the side that faces towards where the B plane should be)
                if (newACutMesh.side)
                {
                    // Slice the new objects on the positive side of the cut, this time with the B plane
                    IBzMeshSlicer subSliceData = newACutMesh.gameObject.GetComponent<IBzMeshSlicer>();
                    var resultOfSecondPlaneSlice = await subSliceData.SliceAsync(riftManager.planeB);
                    if (resultOfSecondPlaneSlice.sliced)
                    {
                        foreach (var newBCutMesh in resultOfSecondPlaneSlice.resultObjects)
                        {
                            // Make sure the part knows that it's a cut mesh
                            CorGeo_SliceableMesh newBCutCorGeoSliceableMesh = newBCutMesh.gameObject.GetComponent<CorGeo_SliceableMesh>();
                            newBCutCorGeoSliceableMesh.isSlicedByPlane = true;

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
                                riftManager.spaceNullMeshes.Add(newBCutMesh.gameObject);
                                newBCutMesh.gameObject.name = $"{name} [NULL]";
                            }

                            // Finally sort B meshes into rift manager list
                            else
                            {
                                riftManager.spaceBMeshes.Add(newBCutMesh.gameObject);
                                newBCutMesh.gameObject.name = $"{name} [B]";
                            }
                        }
                    }
                    // If B plane slice fails, the rest of the mesh was in null space
                    else
                    {
                        //obj2.gameObject.transform.SetParent(Alt_Item_Geodesic_Utility_GeoGun.planeBMeshes.transform);
                        riftManager.spaceNullMeshes.Add(newACutMesh.gameObject);
                        newACutMesh.gameObject.name = $"{name} [NULL2]";
                    }
                }
                // Sort A meshes into rift manager list
                else
                {
                    riftManager.spaceAMeshes.Add(newACutMesh.gameObject);
                    newACutMesh.gameObject.name = $"{name} [A]";
                }
            }
        }
        
        // --- PART TWO ---
        // Even if A plane fails, still attempt to slice across the rift's 'B Plane'
        else
        {
            var resultOfPlaneBSlice = await slicer.SliceAsync(riftManager.planeB, sliceData);
            if (resultOfPlaneBSlice.sliced)
            {
                // Hide the original and set the isSlicedByPlane to true, so we can skip Part Three
                _originalMesh.SetActive(false);
                riftManager.hiddenOriginalMeshes.Add(_originalMesh);
                isSlicedByPlane = true;

                foreach (var newBCutMesh in resultOfPlaneBSlice.resultObjects)
                {
                    // Make sure the part knows that it's a cut mesh
                    CorGeo_SliceableMesh newBCutCorGeoSliceableMesh = newBCutMesh.gameObject.GetComponent<CorGeo_SliceableMesh>();
                    newBCutCorGeoSliceableMesh.isSlicedByPlane = true;

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
                        riftManager.spaceNullMeshes.Add(newBCutMesh.gameObject);
                        newBCutMesh.gameObject.name = $"{name} [NULL]";
                    }

                    // Finally sort B meshes into rift manager list
                    else
                    {
                        riftManager.spaceBMeshes.Add(newBCutMesh.gameObject);
                        newBCutMesh.gameObject.name = $"{name} [B]";
                    }
                }
            }
        }
        
        // --- PART THREE ---
        // If both slices fail, the clone should be destroyed, and the original needs to be set to the correct space container
        if (isSlicedByPlane is false)
        {
            // If all the slices miss, we have to figure out where this mesh is.

            MeshFilter meshFilter = GetComponent<MeshFilter> ();
            if (meshFilter && meshFilter.mesh.vertices.Length > 0)
            {
                var vert = meshFilter.mesh.vertices[0];
                Vector3 testPoint = new Vector3 (vert.x, vert.y, vert.z);
                Vector3 worldPoint = transform.TransformPoint (testPoint);
                // Object is in A Space
                if (riftManager.planeA.GetDistanceToPoint (worldPoint) < 0)
                {
                    // Set the original to correct space
                    riftManager.spaceAMeshes.Add(_originalMesh);
                    // Destroy ourselves (The clone)
                    Destroy (gameObject);
                }
                // Object is in B Space
                else if (riftManager.planeB.GetDistanceToPoint (worldPoint) < 0)
                {
                    // Set the original to correct space
                    riftManager.spaceBMeshes.Add(_originalMesh);
                    // Destroy ourselves (The clone)
                    Destroy (gameObject);
                }
                // Object is in NULL Space
                else
                {
                    // Set the original to correct space
                    riftManager.spaceNullMeshes.Add(_originalMesh);
                    // Destroy ourselves (The clone)
                    Destroy (gameObject);
                }
            }
        }
            
            
        /*
        bool sliced = false;
        Collider coll = GetComponent<Collider> ();
        if (!coll) return;
        bool isTrigger = coll.isTrigger;
        sliceableObject= GetComponent<BzSliceableObject> ();
        //Slice the object
        var result = await sliceableObject.SliceAsync (Alt_Item_Geodesic_Utility_GeoGun.planeA, meshSlicer);
        if (result.sliced)
        {

            _original.SetActive (false);
            sliced = true;
            foreach (var obj in result.resultObjects)
            {
                Mesh_Slicable objMesh1 = GetComponent<Mesh_Slicable> ();
                objMesh1.isCut = true;
                foreach (Collider collider in obj.gameObject.GetComponents<Collider> ())
                {
                    collider.isTrigger = isTrigger;
                    MeshCollider meshColl = collider as MeshCollider;
                    if (meshColl != null) //todo is this a bug? i think i meant to only do this if the mesh was a trigger
                    {
                        meshColl.convex = true;
                    }
                }

                Alt_Item_Geodesic_Utility_GeoGun.slicedMeshes.Add (obj.gameObject);
                if (obj.side)
                {
                    //Slice the new object on the positive side of the cut, this time with the other plane
                    IBzMeshSlicer objSlicer = obj.gameObject.GetComponent<IBzMeshSlicer> ();
                    var result2 = await objSlicer.SliceAsync (Alt_Item_Geodesic_Utility_GeoGun.planeB);
                    if (result2.sliced)
                    {
                        sliced = true;
                        _original.SetActive (false);
                        //add the positive sides to the null list
                        foreach (var obj2 in result2.resultObjects)
                        {
                            foreach (Collider collider in obj2.gameObject.GetComponents<Collider> ())
                            {
                                collider.isTrigger = isTrigger;
                                MeshCollider meshColl = coll as MeshCollider;
                                if (meshColl != null)
                                {
                                    meshColl.convex = true;
                                }
                            }
                            Mesh_Slicable objMesh2 = obj2.gameObject.GetComponent<Mesh_Slicable> ();
                            objMesh2.isCut = true;
                            Alt_Item_Geodesic_Utility_GeoGun.slicedMeshes.Add (obj.gameObject);

                            if (partsReference)
                            {
                                partsReference.AddSlice (objMesh2);
                            }

                            if (obj2.side)
                            {
                                Alt_Item_Geodesic_Utility_GeoGun.nullSlices.Add (obj2.gameObject);
                            }
                            else
                            {
                                obj2.gameObject.transform.SetParent (Alt_Item_Geodesic_Utility_GeoGun.planeBMeshes.transform);
                            }
                        }
                    }
                    else
                    {
                        //if slice 2 failed, we still add this object
                        Alt_Item_Geodesic_Utility_GeoGun.nullSlices.Add (obj.gameObject);
                        if (partsReference)
                        {
                            partsReference.AddSlice (obj.gameObject.GetComponent<Mesh_Slicable> ());
                        }
                    }
                }
                else //if !obj.side
                {
                    if (partsReference)
                    {
                        partsReference.AddSlice (obj.gameObject.GetComponent<Mesh_Slicable> ());
                    }
                }
            }
        }

        else //if we didn't slice, we still try slicing with plane2
        {
            var result2 = await sliceableObject.SliceAsync (Alt_Item_Geodesic_Utility_GeoGun.planeB, meshSlicer);
            if (result2.sliced)
            {
                _original.SetActive (false);
                sliced = true;
                foreach (var obj3 in result2.resultObjects)
                {
                    foreach (Collider collider in obj3.gameObject.GetComponents<Collider> ())
                    {
                        collider.isTrigger = isTrigger;
                        MeshCollider meshColl = coll as MeshCollider;
                        if (meshColl != null)
                        {
                            meshColl.convex = true;
                        }
                    }
                    Mesh_Slicable objMesh3 = obj3.gameObject.GetComponent<Mesh_Slicable> ();
                    objMesh3.isCut = true;
                    Alt_Item_Geodesic_Utility_GeoGun.slicedMeshes.Add (obj3.gameObject);
                    if (partsReference)
                    {
                        partsReference.AddSlice (objMesh3);
                    }

                    if (obj3.side)
                    {
                        Alt_Item_Geodesic_Utility_GeoGun.nullSlices.Add (obj3.gameObject);
                    }
                    else
                    {
                        obj3.gameObject.transform.SetParent (Alt_Item_Geodesic_Utility_GeoGun.planeBMeshes.transform);
                    }
                }
            }
        }


        if (!sliced)
        {
            //If all the slices miss, we have to figure out where this mesh is.

            MeshFilter meshFilter = GetComponent<MeshFilter> ();
            if (meshFilter != null && meshFilter.mesh.vertices.Length > 0)
            {
                var vert = meshFilter.mesh.vertices[0];
                Vector3 testPoint = new Vector3 (vert.x, vert.y, vert.z);
                Vector3 worldPoint = transform.TransformPoint (testPoint);
                if (Alt_Item_Geodesic_Utility_GeoGun.planeA.GetDistanceToPoint (worldPoint) < 0)
                {
                    //Entire object was outside nullspace plane1
                    _original.gameObject.SetActive (true);
                    Destroy (gameObject);
                    return;
                }
                else
                {
                    if (Alt_Item_Geodesic_Utility_GeoGun.planeB.GetDistanceToPoint (worldPoint) < 0)
                    {
                        //entire object was outside nullspace plane2
                        _original.SetActive (true);
                        _original.transform.SetParent (Alt_Item_Geodesic_Utility_GeoGun.planeBMeshes.transform);
                        Destroy (gameObject);
                        return;
                    }
                    else
                    {
                        //Entire object was in the nullspace
                        Alt_Item_Geodesic_Utility_GeoGun.nullSlices.Add (_original);
                        Destroy (gameObject);
                        return;
                    }
                }
            }
        }*/
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
        // If this is a clone, exit
        // If we are intersecting a cut plane
            // create a clone
            // slice the clone
        // Else
            // Store our 'home' transform
            // Attach ourselves to the appropriate space
        
        // Exit if this is a clone
        if (gameObject.name.StartsWith("[CUT]")) return;
        
        // Make a clone of this mesh to be cut
        CorGeo_SliceableMesh corGeoSliceableMeshClone = Instantiate(this, transform.position, transform.rotation);

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

    #endregion
}
