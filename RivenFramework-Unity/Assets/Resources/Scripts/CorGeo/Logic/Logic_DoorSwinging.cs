//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Events;

public class Logic_DoorSwinging : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicInput<bool> interactedSideA = new(false);
    public LogicInput<bool> interactedSideB = new(false);
    public DoorState currentDoorState = DoorState.Closed;
    public DoorLockState currentDoorLockState = DoorLockState.bothSidesLocked;

    public float openAngle = 90f;
    public float springForce = 150f;
    public float springDamper = 10f;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private float targetAngle = 0;
    private bool isTraveling = false;
    private float travelTimer = 0f;
    private float jamCheckDelay = 0.1f;
    


    //=-----------------=
    // Reference Variables
    //=-----------------=
    public Rigidbody doorRigidbody;
    public HingeJoint hingeJoint;
    public GameObject doorHandle;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        ConfigureSpring(targetAngle);
        
        if (interactedSideA.HasLogicOutputSource) interactedSideA.CallOnSourceChanged(() => { Toggle(0); });
        if (interactedSideB.HasLogicOutputSource) interactedSideB.CallOnSourceChanged(() => { Toggle(1); });
    }

    private void FixedUpdate()
    {
        if (!isTraveling) return;

        travelTimer += Time.fixedDeltaTime;
        if (travelTimer < jamCheckDelay) return;

        float angularSpeed = doorRigidbody.angularVelocity.magnitude * Mathf.Rad2Deg;
        float angleRemaining = Mathf.Abs(hingeJoint.angle - targetAngle);

        if (angularSpeed < 2f)
        {
            if (angleRemaining > 3f)
            {
                currentDoorState = DoorState.Jammed;
                travelTimer = jamCheckDelay;
            }
            else
            {
                isTraveling = false;
                currentDoorState = (targetAngle == 0) ? DoorState.Closed : (targetAngle > 0) ? DoorState.OpenedToSideA : DoorState.OpenedToSideB;
                doorRigidbody.isKinematic = true;
            }
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void Toggle(int side)
    {
        if (!interactedSideA.Get() && !interactedSideB.Get()) return;
        
        if (currentDoorLockState == DoorLockState.bothSidesLocked)
        {
            AnimateHandleLocked();
            return;
        }

        bool sideIsLocked = (side == 0 && currentDoorLockState == DoorLockState.sideALocked ||
                             side == 1 && currentDoorLockState == DoorLockState.sideBLocked);

        if (currentDoorState == DoorState.Closed)
        {
            if (sideIsLocked)
            {
                AnimateHandleLocked();
                return;
            }
            AnimateHandleSuccess();
            OpenTowards(side);
        }
        else if (currentDoorState == DoorState.Jammed)
        {
            AnimateHandleSuccess();
            OpenTowards(targetAngle > 0 ? 0 : 1);
        }
        else
        {
            AnimateHandleSuccess();
            Close();
        }
    }

    private void OpenTowards(int side)
    {
        BeginTravel();
        targetAngle = (side == 0) ? openAngle : -openAngle;
        currentDoorState = side == 0 ? DoorState.OpenedToSideA : DoorState.OpenedToSideB;
        ConfigureSpring(targetAngle);
    }

    private void Close()
    {
        BeginTravel();
        targetAngle = 0f;
        currentDoorState = DoorState.Closed;
        ConfigureSpring(targetAngle);
    }

    private void BeginTravel()
    {
        doorRigidbody.isKinematic = false;
        isTraveling = true;
        travelTimer = 0f;
    }

    private void ConfigureSpring(float targetAngle)
    {
        hingeJoint.useSpring = true;
        JointSpring spring = hingeJoint.spring;
        spring.spring = springForce;
        spring.damper = springDamper;
        spring.targetPosition = targetAngle;
        hingeJoint.spring = spring;
    }

    private void AnimateHandleSuccess()
    {
        if (!doorHandle) return;
        doorHandle.transform.DOLocalRotate(new Vector3(0, 0, 30f), 0.15f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad).OnComplete(() =>
            doorHandle.transform.DOLocalRotate(new Vector3(0, 0, -30f), 0.2f, RotateMode.LocalAxisAdd).SetEase(Ease.InQuad));
    }

    private void AnimateHandleLocked()
    {
        if (!doorHandle) return;
        doorHandle.transform.DOKill();
        Vector3 restRotation = doorHandle.transform.localEulerAngles;
        doorHandle.transform
            .DOShakeRotation(0.4f, new Vector3(0, 0, 12f), vibrato: 15, randomness: 0).SetEase(Ease.OutQuad).OnComplete(() =>
                doorHandle.transform.DOLocalRotate(restRotation, 0.1f).SetEase(Ease.OutQuad));
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}

public enum DoorState
{
    OpenedToSideA,
    Closed,
    OpenedToSideB,
    Jammed,
}

public enum DoorLockState
{
    // Interacting with either side results in just nothing
    bothSidesLocked,
    // Interacting with side a when the door is closed will do nothing, but will close the door in any other state
    sideALocked,
    // Interacting with side a when the door is closed will do nothing, but will close the door in any other state
    sideBLocked,
    // Interacting with either side will open or close the door
    bothSidesUnlocked,
}