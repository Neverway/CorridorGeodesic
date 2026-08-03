//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInstance = RivenFramework.GameInstance;

public class Func_DisplayKeyhint : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> displayKeyhint = new(false);
    public float duration = 3f;
    public string keyhintText = "Lemon Milk";
    
    [Header("Dynamic Assignment")]
    public string targetActionMap;
    public string targetAction;

    [Header("Manuel Assignment")] 
    public bool useManuelAssignment = false;
    public Sprite keyhintImage;
    

    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Start()
    {
        if (displayKeyhint.HasLogicOutputSource is false) return;
        displayKeyhint.CallOnSourceChanged(TryDisplayKeyhint);
    }

    public void TryDisplayKeyhint()
    {
        if (displayKeyhint.Get()) DisplayKeyHint();
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    public void DisplayKeyHint()
    {
        if (useManuelAssignment)
        {
            GameInstance.Get<GameplayNotificationManager>().DisplayKeyHint(duration, keyhintText, keyhintImage);
        }
        else
        {
            GameInstance.Get<GameplayNotificationManager>().DisplayKeyHint(duration, keyhintText, targetActionMap, targetAction);
        }
    }
}