//===================== (Neverway 2024) Written by _____ =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_AudioEventPlayer: MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] private bool playOnEnabled = false;


    //=-----------------=
    // Private Variables
    //=-----------------=
    [SerializeField] private EventReference eventReference;

    //=-----------------=
    // Reference Variables
    //=-----------------=
    [SerializeField] private GameObject attached;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void OnEnable()
    {
        if (playOnEnabled)
        {
            if (attached != null) RuntimeManager.PlayOneShotAttached(eventReference, attached);
            else Audio_FMODAudioManager.PlayOneShot(eventReference, transform.position);
        }
    }


    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    public void PlaySound()
    {
        if (attached != null) RuntimeManager.PlayOneShotAttached(eventReference, attached);
        else Audio_FMODAudioManager.PlayOneShot(eventReference, transform.position);
    }
}
