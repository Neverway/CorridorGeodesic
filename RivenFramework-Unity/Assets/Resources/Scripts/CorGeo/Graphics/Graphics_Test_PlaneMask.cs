//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics_Test_PlaneMask : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=

    private Renderer renderer;
    private List<Material> materials = new List<Material> ();

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        renderer = GetComponent<Renderer> ();

        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
        {
            materials.Add (new Material (renderer.sharedMaterials[i]));
        }

        renderer.sharedMaterials = materials.ToArray ();

        foreach (Material mat in renderer.sharedMaterials)
        {
            mat.SetFloat ("_UseSlice", 1);
            mat.SetVector ("_SliceNormalOne", Vector3.right);
            mat.SetVector ("_SliceNormalTwo", -Vector3.right);

            mat.SetVector ("_SliceCenterOne", new Vector3(-1, 0, 0));
            mat.SetVector ("_SliceCenterTwo", new Vector3 (1, 0, 0)); ;
        }
    }

    private void Update()
    {
    
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}