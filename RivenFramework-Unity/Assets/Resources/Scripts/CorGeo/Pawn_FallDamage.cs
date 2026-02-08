using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When a pawn is no longer grounded, check their y position
/// When they are grounded again, if their velocity was over the threshold, and the y distance is lower than the threshold
/// Deal damage based on the multiplication of height
/// </summary>
public class Pawn_FallDamage : MonoBehaviour
{
    [Tooltip("The pawn must be moving downwards faster than this for the damage to count")]
    [SerializeField] private float velocityThreshold = 5;
    [Tooltip("The minimum height the pawn must fall before receiving damage")]
    [SerializeField] private float fallDistanceThreshold = 16;
    [Tooltip("How much damage to apply")]
    [SerializeField] private float damageAmount = 10;
    [Tooltip("How much to multiply damage based on how many times over the distance threshold the pawn fell")]
    [SerializeField] private float damageDistanceMultiplier = 1;
    
    [Tooltip("Tracker for if the pawn is grounded")]
    private bool isPawnGrounded;
    [Tooltip("Tracker for if the pawn is in the process of falling")]
    private bool pawnIsFalling;
    [Tooltip("The Y position the pawn was at when they stopped touching ground")]
    [SerializeField] private float startingGroundHeight;
    [Tooltip("The Y position the pawn is at now that they've touch ground again")]
    [SerializeField] private float endingGroundHeight;

    private FPPawn linkedPawn;

    private void Start()
    {
        linkedPawn = GetComponent<FPPawn>();
    }

    // Update is called once per frame
    void Update()
    {
        isPawnGrounded = IsOnGround(linkedPawn);

        // Pawn has left the ground, start tracking their fall!
        if (!isPawnGrounded && !pawnIsFalling)
        {
            StartFallingEvent();
        }
        // Pawn has hit the ground, calculate fall damage!
        else if (isPawnGrounded && pawnIsFalling)
        {
            StopFallingEvent();
        }
    }

    private void StartFallingEvent()
    {
        pawnIsFalling = true;
        startingGroundHeight = linkedPawn.transform.position.y;
    }

    private void StopFallingEvent()
    {
        pawnIsFalling = false;
        endingGroundHeight = linkedPawn.transform.position.y;
        print("Stop fall");

        // Pawn wasn't moving fast enough for fall damage
        if (linkedPawn.physicsbody.velocity.y > velocityThreshold)
        {
            print($"{linkedPawn.physicsbody.velocity.y} vel exit");
            return;
        }
        
        // Pawn didn't fall far enough for fall damage
        if (Mathf.Abs(startingGroundHeight - endingGroundHeight) < fallDistanceThreshold)
        {
            print("dis exit");
            return;
        }

        var totalFallDistanceMultiplier = Mathf.Abs(startingGroundHeight - endingGroundHeight) / fallDistanceThreshold;
        print($"FALL SYCCC {damageAmount*(totalFallDistanceMultiplier*damageDistanceMultiplier)}");
        
        linkedPawn.ModifyHealth(-damageAmount*(totalFallDistanceMultiplier*damageDistanceMultiplier));
    }
    
    public bool IsOnGround(FPPawn _pawn)
    {
        // Move the ground check position upwards if the pawn is crouching to account for their change in height
        Vector3 crouchingOffset = new Vector3(0,0,0);
        
        return Physics.CheckSphere(_pawn.transform.position - ((FPPawnStats)_pawn.currentStats).groundCheckOffset + crouchingOffset, ((FPPawnStats)_pawn.currentStats).groundCheckRadius, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore);
    }
}
