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

    private bool sliceStarted = false;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        riftManager = FindObjectOfType<RiftManager> ();
        sectionA.space = SliceSpace.Plane1;
        sectionB.space = SliceSpace.Plane2;
        sectionNull.space = SliceSpace.Null;
        sectionB.gameObject.SetActive (false);
        sectionNull.gameObject.SetActive (false);
    }
    private void OnEnable ()
    {
        RiftManager_StateHandler.OnStateChanged += OnStateChanged;
    }
    private void OnDisable ()
    {
        RiftManager_StateHandler.OnStateChanged -= OnStateChanged;
    }
    private void OnStateChanged ()
    {
        // Whoops, we need this reference, but it's not here!
        if (riftManager is null) riftManager = FindObjectOfType<RiftManager> ();
        // Still didn't find it? Okay, stop everything else
        if (riftManager is null) return;
        var state = riftManager.stateHandler.currentState.GetType ();

        if (riftManager.stateHandler.IsState<RiftState_None> ())
        {
            StopSlicing ();
        }
        else
        {
            //If the rift is real
            StartSlicing ();
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=

    /// <summary>
    /// Enable the Graphics_SliceableSection objects and assign them to separate spaces.
    /// </summary>
    public void StartSlicing()
    {
        if (sliceStarted) return;
        sliceStarted = true;
        //SectionA should always be enabled so it renders when there's no slice happening.
        sectionB.gameObject.SetActive(true);
        sectionNull.gameObject.SetActive(true);

        sectionA.StartSlicing ();
        sectionB.StartSlicing ();
        sectionNull.StartSlicing ();

        sectionA.transform.SetParent(riftManager.spaceController.spaceContainerA.transform, true);
        sectionB.transform.SetParent (riftManager.spaceController.spaceContainerB.transform, true);
        sectionNull.transform.SetParent(riftManager.spaceController.spaceContainerNull.transform, true);
    }

    /// <summary>
    /// Disable all the Graphics_SliceableSection objects.
    /// </summary>
    public void StopSlicing()
    {

        sectionB.gameObject.SetActive (false);
        sectionNull.gameObject.SetActive (false);

        if (sliceStarted == false) return;
        sliceStarted = false;

        sectionA.StopSlicing ();
        sectionB.StopSlicing ();
        sectionNull.StopSlicing ();
    }
    public void SetBool (string _name, bool _isPowered)
    {
        sectionA.SetBool (_name, _isPowered);
        sectionB.SetBool (_name, _isPowered);
        sectionNull.SetBool (_name, _isPowered);
    }

}