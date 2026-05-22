//===================== (Neverway 2024) Written by Connorses ================
//
// Purpose: Manages the 3 Graphics_SliceableSection objects that make up a sliceable object that needs to animate.
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics_ThreePartSliceable : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=

    [SerializeField] Graphics_SliceableSection sectionA;
    [SerializeField] Graphics_SliceableSection sectionB;
    [SerializeField] Graphics_SliceableSection sectionNull;

    private RiftManager riftManager;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public IEnumerator Start()
    {
        yield return new WaitUntil (() => Graphics_ThreePartSliceableManager.Instance != null);

        Graphics_ThreePartSliceableManager.Instance.AddToList (this);

        riftManager = FindObjectOfType<RiftManager> ();
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
    public void StartSlicing()
    {
        sectionA.StartSlicing ();
        sectionB.StartSlicing ();
        sectionNull.StartSlicing ();

        sectionA.transform.SetParent(riftManager.spaceController.spaceContainerA.transform);
        sectionB.transform.SetParent (riftManager.spaceController.spaceContainerB.transform);
        sectionNull.transform.SetParent(riftManager.spaceController.spaceContainerNull.transform);
    }
    public void StopSlicing()
    {
        sectionA.StopSlicing ();
        sectionB.StopSlicing ();
        sectionNull.StopSlicing ();

        sectionA.ResetParent ();
        sectionB.ResetParent ();
        sectionNull.ResetParent ();
    }
    public void SetBool (string _name, bool _isPowered)
    {
        sectionA.SetBool (_name, _isPowered);
        sectionB.SetBool (_name, _isPowered);
        sectionNull.SetBool (_name, _isPowered);
    }

}