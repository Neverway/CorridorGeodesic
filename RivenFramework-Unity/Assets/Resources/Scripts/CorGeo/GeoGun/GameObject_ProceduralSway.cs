//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Add some procedural bobbing and swaying to things like weapons for when a pawn looks around
// Notes: This is based off the tutorial by BuffaTwo
// Source: https://youtu.be/DR4fTllQnXg
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObject_ProceduralSway : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] private bool lockXRot;
    [SerializeField] private bool lockYRot;
    [SerializeField] private float xRotMultiplier=1;
    [SerializeField] private float yRotMultiplier=1;
    
    [SerializeField] private bool sway = true;
    [SerializeField] private bool bob = true;
    [SerializeField] private float smoothingPosition = 10f;
    [SerializeField] private float smoothingRotation = 12f;
    [SerializeField] private float smoothingRotation2 = 12f;
    [Header("Sway")]
    [SerializeField] private float swayPositionStep = 0.01f;
    [SerializeField] private float swayRotationStep = 1f;
    [SerializeField] private float maxSwayDistance = 0.06f;
    [SerializeField] private float maxSwayRotation = 5f;
    
    [Header("Sway Threshold")]
    [SerializeField] private float rotationThreshold = 0.1f;
    
    [Header("Bob")]
    [SerializeField] private float bobIntensity = 0.01f;
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobSmoothing = 5f;
    [SerializeField] private float movementThreshold = 0.01f;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private Vector2 moveInput;
    private Vector2 lastLook;
    private Vector2 currentLook;
    private Vector2 lookInput;
    private Vector3 swayPosition;
    private Vector3 swayRotation;
    private bool hasInitialized; // A delay value to make sure that things like ropes that may be attached have time to get their position references

    private float currentXRot;
    private float currentYRot;
    private float lastXRot;
    private float lastYRot;
    
    private Vector3 lastBodyPosition;
    private float bobTimer;
    private Vector3 bobOffset;



    //=-----------------=
    // Reference Variables
    //=-----------------=
    //[SerializeField] private Rigidbody positionReference;
    //[SerializeField] private Pawn pawn;
    //[SerializeField] private PlayerController_FirstPerson_CorGeo pawnController;
    [SerializeField] private Transform xRotationRef, yRotationRef, bodyRef;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        if (bodyRef != null) lastBodyPosition = bodyRef.position;
    }

    private void Update()
    {
        GetInput();
        Sway();
        Bob();
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void GetInput()
    {
        float rawXRot = xRotationRef.localEulerAngles.x;
        float rawYRot = yRotationRef.localEulerAngles.y;
        
        // Fix -180 180 wrap arounds
        float deltaX = Mathf.DeltaAngle(lastXRot, rawXRot);
        float deltaY = Mathf.DeltaAngle(lastYRot, rawYRot);

        // Filter small jitters
        float threshold = 0.1f;
        if (Mathf.Abs(deltaX) < threshold) deltaX = 0;
        if (Mathf.Abs(deltaY) < threshold) deltaY = 0;

        
        currentLook = new Vector2(deltaY, deltaX);

        // Smooth to prevent snap
        lookInput = Vector2.Lerp(lookInput, currentLook, Time.deltaTime * smoothingRotation2);

        lastXRot = rawXRot;
        lastYRot = rawYRot;
        lastLook = currentLook;
    }

    private float NormalizeAngle(float angle)
    {
        return (angle > 180f) ? angle - 360f : angle;
    }
    
    private void Sway()
    {
        if (!sway)
        {
            swayPosition = Vector3.zero;
            return;
        }
        
        
        // Sway position
        Vector3 invertLook = lookInput * -swayPositionStep;
        // Apply lock
        if (lockXRot) invertLook.x = 0;
        if (lockYRot) invertLook.y = 0;
        //
        invertLook.x = Mathf.Clamp(invertLook.x, -maxSwayDistance, maxSwayDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxSwayDistance, maxSwayDistance);
        swayPosition = invertLook;
        
        // Sway Rotation
        invertLook = lookInput * -swayRotationStep;
        // Apply multiplier
        invertLook.x *= xRotMultiplier;
        invertLook.y *= yRotMultiplier;
        //
        // Apply lock again
        if (lockXRot) invertLook.x = 0;
        if (lockYRot) invertLook.y = 0;
        //
        invertLook.x = Mathf.Clamp(invertLook.x, -maxSwayRotation, maxSwayRotation);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxSwayRotation, maxSwayRotation);
        swayRotation = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        
        // Apply movement & rotation
        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPosition, Time.deltaTime * smoothingPosition);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayRotation), Time.deltaTime*smoothingRotation);
    }

    private void Bob()
    {
        if (!bob || bodyRef == null)
            return;

        // Movement delta
        Vector3 bodyDelta = bodyRef.position - lastBodyPosition;
        float movementSpeed = bodyDelta.magnitude / Time.deltaTime;

        // If movement is significant, advance bobbing
        if (movementSpeed > movementThreshold)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobAmount = Mathf.Sin(bobTimer) * bobIntensity;

            // Up-down bob on local Y
            bobOffset = new Vector3(0, bobAmount, 0);
        }
        else
        {
            // Reset timer slowly when standing still to avoid jitter
            bobTimer = Mathf.Lerp(bobTimer, 0, Time.deltaTime * bobSmoothing);
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * bobSmoothing);
        }

        // Apply bob offset additively on top of swayPosition
        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPosition + bobOffset, Time.deltaTime * smoothingPosition);

        // Update for next frame
        lastBodyPosition = bodyRef.position;
    }



    //=-----------------=
    // External Functions
    //=-----------------=
}
