//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class FPPawn : Pawn
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [HideInInspector] public List<Pawn> visiblePawns = new List<Pawn>();
    [HideInInspector] public List<Pawn> visibleHostiles = new List<Pawn>();
    [HideInInspector] public List<Pawn> visibleAllies = new List<Pawn>();


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    public FPPawnStats FPDefaultStats;
    public FPPawnStats FPCurrentStats => (FPPawnStats)currentStats;
    public FPPawnActions action;
    [HideInInspector] public Rigidbody physicsbody;
    [HideInInspector] public GameObject viewPoint;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        // Get references
        physicsbody = GetComponent<Rigidbody>();
        viewPoint = transform.Find("ViewPoint").gameObject;
        
        defaultStats = FPDefaultStats;     
        currentStats = defaultStats;
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}