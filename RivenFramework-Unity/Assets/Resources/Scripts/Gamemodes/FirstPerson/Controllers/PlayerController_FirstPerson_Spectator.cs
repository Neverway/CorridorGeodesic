//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using Neverway.Framework.ApplicationManagement;
using UnityEngine;
using Neverway.Framework.PawnManagement;

[CreateAssetMenu(
    fileName="PlayerController_FirstPerson_Spectator", 
    menuName="Gamemodes/FirstPerson/Controllers/PlayerController_FirstPerson_Spectator")]
public class PlayerController_FirstPerson_Spectator : PawnController
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private float yRotation;
    private float xRotation;
    private Vector3 moveDirection;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private InputActions.FirstPersonActions firstPersonActions;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public override void PawnAwake(Pawn _pawn)
    {
        // Setup inputs
        firstPersonActions = new InputActions().FirstPerson;
        firstPersonActions.Enable();
    }
    
    public override void PawnUpdate(Pawn _pawn)
    {
        UpdateRotation(_pawn);
        UpdateMovement(_pawn);
    }

    public override void PawnFixedUpdate(Pawn _pawn)
    {
        MovePawn(_pawn);
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void UpdateRotation(Pawn _pawn)
    {
        var applicationSettings = FindObjectOfType<ApplicationSettings>();
        // Separate multipliers for mouse and joystick
        float mouseMultiplier = 0.01f;
        float joystickMultiplier = 0.2f;

        // Determine the input method (mouse or joystick)
        bool isUsingMouse = false;
        if (firstPersonActions.LookAxis.IsInProgress())
        {
            if (firstPersonActions.LookAxis.activeControl.device.name == "Mouse")
            {
                isUsingMouse = true;
            }
        }

        // Apply the appropriate multiplier
        var multiplier = isUsingMouse ? mouseMultiplier : joystickMultiplier;
        yRotation += firstPersonActions.LookAxis.ReadValue<Vector2>().x * (20 * applicationSettings.currentSettingsData.horizontalLookSpeed) * multiplier;
        xRotation -= firstPersonActions.LookAxis.ReadValue<Vector2>().y * (20 * applicationSettings.currentSettingsData.verticalLookSpeed) * multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        _pawn.transform.rotation = Quaternion.Euler(0, yRotation, 0);
        _pawn.viewCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    private void UpdateMovement(Pawn _pawn)
    {
        moveDirection = _pawn.transform.forward * firstPersonActions.Move.ReadValue<Vector2>().y + _pawn.transform.right * firstPersonActions.Move.ReadValue<Vector2>().x;
    }

    private void MovePawn(Pawn _pawn)
    {
        _pawn.transform.Translate(moveDirection * (((CharacterStats_FirstPerson)_pawn.currentStats).movementSpeed * Time.deltaTime));
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
