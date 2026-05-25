//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Keeps references to slices of an object when the object is sliced.
// Notes:
//
//=============================================================================

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicedPartsReference : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=

    [SerializeField] private List<CorGeo_SliceableMesh> sliceList = new List<CorGeo_SliceableMesh> ();
    private List<CorGeo_SliceableMesh> originalMesh = new List<CorGeo_SliceableMesh> ();

    //=-----------------=
    // Reference Variables
    //=-----------------=

    /// <summary>
    /// Returns the list of sliced mesh parts, unless there are none in which case return the original mesh.
    /// </summary>
    public List<CorGeo_SliceableMesh> GetMeshes { get {
            if (sliceList.Count > 0)
            {
                return sliceList;
            }
            return originalMesh;
        } }

    //public List<Mesh_Slicable> SlicedMeshesOnly => sliceList;
    //public List<Mesh_Slicable> originalMeshOnly => originalMesh;

    //=-----------------=
    // Mono Functions
    //=-----------------=

    //=-----------------=
    // Internal Functions
    //=-----------------=

    //=-----------------=
    // External Functions
    //=-----------------=

    public void Setup (CorGeo_SliceableMesh slicable)
    {
        originalMesh.Add (slicable);
    }

    public void DoReset ()
    {
        sliceList.Clear ();
    }

    public void AddSlice (CorGeo_SliceableMesh slice)
    {
        sliceList.Add (slice);
    }

    public void SetLayer (string layerName)
    {
        int i = LayerMask.NameToLayer (layerName);
        foreach (var mesh in  originalMesh)
        {
            if (mesh != null)
            {
                mesh.gameObject.layer = i;
            }
        }
        foreach (var mesh in GetMeshes)
        {
            if (mesh != null)
            {
                mesh.gameObject.layer = i;
            }
        }
    }

}