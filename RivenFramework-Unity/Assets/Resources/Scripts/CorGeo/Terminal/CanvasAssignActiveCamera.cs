using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class CanvasAssignActiveCamera : MonoBehaviour
{
    private bool hasCompleted;
    void Update()
    {
        if (hasCompleted) return;
        var pawnManager = GameInstance.Get<GI_PawnManager>();
        if (!pawnManager) return;
        if (!pawnManager.localPlayerCharacter) return;
        var localPlayerCharacter = pawnManager.localPlayerCharacter.GetComponent<FPPawn>();
        GetComponent<Canvas>().worldCamera = localPlayerCharacter.GetComponentInChildren<Camera>(true);
        hasCompleted = true;
    }
}
