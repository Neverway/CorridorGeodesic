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
    public List<Pawn> visiblePawns = new List<Pawn>();
    public List<Pawn> visibleHostiles = new List<Pawn>();
    public List<Pawn> visibleAllies = new List<Pawn>();


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    public new FPPawnStats defaultStats;
    public new FPPawnStats currentStats;
    public new FPPawnActions action;
    public Rigidbody physicsbody;
    public GameObject viewPoint;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        // Get references
        physicsbody = GetComponent<Rigidbody>();
        viewPoint = transform.Find("ViewPoint").gameObject;
        currentStats = defaultStats;
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}