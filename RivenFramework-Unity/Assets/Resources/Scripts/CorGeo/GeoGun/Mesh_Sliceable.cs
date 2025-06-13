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
// Source: https://www.youtube.com/watch?v=VwGiwDLQ40A
//
//====================================================================================================================//

using System;
using UnityEngine;
using BzKovSoft.ObjectSlicer;
using DG.Tweening.Core.Easing;

/// <summary>
/// Added to a mesh to allow it to be sliced
/// </summary>
/// 
[RequireComponent (typeof (BzSliceableObject))]
public class Mesh_Sliceable : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        EnsureNonConvexWhenCloned();
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Slices the mesh if it intersects with either of the rift planes
    /// This should only ever be called on clones of a mesh to avoid destroying the original!
    /// </summary>
    private async void AttemptSlice(GameObject _originalMesh)
    {
        // This will stay false if the attempt to slice fails
        // (aka the objects is not intersecting with a rift cut plane)
        bool slicedByPlane = false;
        var slicer = GetComponent<BzSliceableObject>();
        var sliceData = GetComponent<IBzMeshSlicer>();
        var riftManager = FindObjectOfType<GI_RiftManager>();

        // Attempt to slice across the rift's 'A Plane'
        if (slicedByPlaneA)
        {
            
        }

        // Even if A plane fails, still attempt to slice across the rift's 'B Plane'
        if (slicedByPlaneB)
        {
            
        }

        // If both slices fail, the clone should be destroyed, and the original needs to be set to the correct space container
        if (slicedByPlaneA is false && slicedByPlaneB is false)
        {
            
        }
        
        // Attempt to slice across the rift's 'A Plane'
        var resultOfAPlaneSlice = await slicer.SliceAsync(riftManager.planeA, sliceData);
        if (resultOfAPlaneSlice.sliced)
        {
            _originalMesh.SetActive(false);
            slicedByPlane = true;
            
            foreach (var obj in resultOfAPlaneSlice.resultObjects)
            {
                // Get if this is the positive or negative side of the plane
                if (obj.side)
                {
                }
            }
        }
            
            
        
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
        }
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
        Mesh_Sliceable meshClone = Instantiate(this, transform.position, transform.rotation);

        // Slice the clone ONLY!!! Never slice the original objects!!
        meshClone.AttemptSlice(gameObject);


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
