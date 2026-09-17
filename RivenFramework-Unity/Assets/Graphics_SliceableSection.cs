//===================== (Neverway 2024) Written by Connorses =====================
//
// Purpose: One section of a graphic that uses the Graphics_ThreePartSliceable script.
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics_SliceableSection : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=

    [SerializeField] private Renderer rend;
    [SerializeField] private Animator animator;
    private RiftManager riftManager;
    public SliceSpace space;
    private List<Material> materials = new List<Material> ();
    private bool useSlice;
    private Transform originalParent;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        originalParent = transform.parent;

        riftManager = FindAnyObjectByType<RiftManager>();

        for (int i = 0; i < rend.sharedMaterials.Length; i++)
        {
            materials.Add (new Material (rend.sharedMaterials[i]));
        }

        rend.sharedMaterials = materials.ToArray ();
    }

    private void Update()
    {
        if (useSlice && rend.sharedMaterials.Length > 0)
        {
            SetMaterialPlanes ();
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    public void SetMaterialPlanes ()
    {

        Vector3 normalB = RiftManager.cutPlaneA.normal;
        Vector3 normalA = RiftManager.cutPlaneB.normal;
        Vector3 planePosA = riftManager.geometryHandler.visualPlaneA.transform.position;
        Vector3 planePosB = riftManager.geometryHandler.visualPlaneB.transform.position;
        Debug.Log ("normalA " + normalA);
        Debug.Log ("normalB " + normalB);

        Debug.Log ("Setting " + name);
        switch (space)
        {

            case SliceSpace.Plane1:

                foreach (Material mat in rend.materials)
                {
                    mat.SetVector ("_SliceNormalOne", normalA * -1);
                    mat.SetVector ("_SliceNormalTwo", normalB);

                    mat.SetVector ("_SliceCenterOne", planePosA);
                    mat.SetVector ("_SliceCenterTwo", planePosB);
                }
                break;
            case SliceSpace.Plane2:
                foreach (Material mat in rend.materials)
                {
                    mat.SetVector ("_SliceNormalOne", normalA);
                    mat.SetVector ("_SliceNormalTwo", normalB * -1);

                    mat.SetVector ("_SliceCenterOne", planePosA);
                    mat.SetVector ("_SliceCenterTwo", planePosB);
                }
                break;
            case SliceSpace.Null:
                foreach (Material mat in rend.materials)
                {
                    mat.SetVector ("_SliceNormalOne", normalA);
                    mat.SetVector ("_SliceNormalTwo", normalB);

                    mat.SetVector ("_SliceCenterOne", planePosA);
                    mat.SetVector ("_SliceCenterTwo", planePosB);
                }
                break;
        }
    }
    public void StartSlicing ()
    {
        /*Debug.Log ("StartSlicing()");
        Plane[] planes = new Plane[2];
        planes[0] = RiftManager.cutPlaneA;
        planes[1] = RiftManager.cutPlaneB;
        if (GeometryUtility.TestPlanesAABB (planes, rend.bounds))
            Slice ();
    }
    private void Slice ()
    {*/

        useSlice = true;
        if (rend.sharedMaterials.Length == 0)
            return;

        Debug.Log ("Slice()");
        foreach (Material mat in rend.sharedMaterials)
        {
            mat.SetFloat ("_UseSlice", 1);
        }
    }
    public void StopSlicing ()
    {
        useSlice = false;
        if (rend.sharedMaterials.Length == 0)
            return;

        foreach (Material mat in rend.sharedMaterials)
        {
            mat.SetFloat ("_UseSlice", 0);
        }
    }

    public void SetBool (string _name, bool _isPowered)
    {
        animator.SetBool (_name, _isPowered);
    }

    public void ResetParent ()
    {
        transform.SetParent (originalParent);
    }
}