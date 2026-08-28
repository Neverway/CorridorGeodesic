//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicDebugIndicator : Logic
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> input = new(false);
    public LogicOutput<bool> output = new(false);
    public GameObject gameObjectOn, gameObjectOff;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Update()
    {
        if (input)
        {
            output.Set(true);
            gameObjectOn.SetActive(true);
            gameObjectOff.SetActive(false);
        }
        else
        {
            output.Set(false);
            gameObjectOn.SetActive(false);
            gameObjectOff.SetActive(true);
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
