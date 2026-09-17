using System.Collections;
using System.Collections.Generic;
using Neverway.Framework.PawnManagement;
using RivenFramework;
using UnityEngine;
using GameInstance = RivenFramework.GameInstance;

/// <summary>
/// Smoothly moves the active pawn view camera to this location while powered
/// When power is removed, the camera returns back to its original location
/// </summary>
public class Logic_CameraTarget : MonoBehaviour
{
    private object playerPauseToken;
    public void ActivateCameraTarget()
    {
        // Get references to pawn
        var pawnManager = GameInstance.Get<GI_PawnManager>();
        var localPlayerCharacter = pawnManager.localPlayerCharacter.GetComponent<FPPawn>();
        
        // Pause the player
        localPlayerCharacter.Pause(out playerPauseToken);
        
        // Move the camera
        FPPawnActions action = (FPPawnActions)localPlayerCharacter.action;
        action.StartCameraSequence(localPlayerCharacter, gameObject.transform);
    }

    public void DeactivateCameraTarget()
    {
        // Get references to pawn
        var pawnManager = GameInstance.Get<GI_PawnManager>();
        var localPlayerCharacter = pawnManager.localPlayerCharacter.GetComponent<FPPawn>();
        
        // Unpause the player
        localPlayerCharacter.Unpause(playerPauseToken);
            
        // Return the camera
        FPPawnActions action = (FPPawnActions)localPlayerCharacter.action;
        action.EndCameraSequence(localPlayerCharacter);
    }
}
