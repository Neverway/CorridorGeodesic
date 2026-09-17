//===================== (Neverway 2024) Written by Connorses =====================
//
// Purpose: Calls the ThreePartSliceable scripts when the rift is active
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Graphics_ThreePartSliceableManager : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
	public static Graphics_ThreePartSliceableManager Instance;

    //=-----------------=
    // Private Variables
    //=-----------------=
    private List<Graphics_ThreePartSliceable> threePartSliceableList = new List<Graphics_ThreePartSliceable> ();
    private RiftManager riftManager;

    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable ()
    {
        RiftManager_StateHandler.OnStateChanged += OnStateChanged;
    }
    private void OnDisable ()
    {
        RiftManager_StateHandler.OnStateChanged -= OnStateChanged;
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void OnStateChanged ()
    {
        // Whoops, we need this reference, but it's not here!
        if (riftManager is null) riftManager = FindObjectOfType<RiftManager> ();
        // Still didn't find it? Okay, stop everything else
        if (riftManager is null) return;
        var state = riftManager.stateHandler.currentState.GetType ();
        if (state != typeof (RiftState_None))
        {
            //If the rift is real
            SliceObjects ();
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
    public void AddToList(Graphics_ThreePartSliceable obj)
    {
        threePartSliceableList.Add(obj);
    }
    public void ClearList()
    {
        threePartSliceableList.Clear();
    }
    public void SliceObjects()
    {
        threePartSliceableList.ForEach(s => s.StartSlicing());
    }
}