//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (CorGeo_SliceableMesh))]
public class SliceableSectionController : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=

    private CorGeo_SliceableMesh sliceableMesh;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        sliceableMesh = GetComponent<CorGeo_SliceableMesh>();
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=

    public void SetCollisionLayers (int _layer)
    {
        foreach (CorGeo_SliceableMesh mesh in sliceableMesh.GetAllMeshes ())
        {
            mesh.gameObject.layer = _layer;
        }
    }
    public void SetCollisionLayers (string _layer)
    {
        foreach (CorGeo_SliceableMesh mesh in sliceableMesh.GetAllMeshes ())
        {
            mesh.gameObject.layer = LayerMask.NameToLayer (_layer);
        }
    }

}