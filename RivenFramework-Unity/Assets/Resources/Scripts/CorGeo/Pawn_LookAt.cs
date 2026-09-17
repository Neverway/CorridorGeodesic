//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Attached to a bone on a rig to make a pawn look towards a target point
// Notes:  
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a target object rotate to look at another target
/// Prioritizes looking at an object that is moving and is closest to the rotating target
/// </summary>
public class Pawn_LookAt : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Header("Rotation Parameters")]
    [Tooltip("The distance from ourselves to check for targets")]
    [SerializeField] private float targetRange;
    [Tooltip("If enabled, the rotating target can rotate on this local rotation axis to try to look at its target")]
    [SerializeField] private bool enableXRotation, enableYRotation, enableZRotation;
    [Tooltip("Adjust these to make the rotating target use the proper direction to look at it's target")]
    [SerializeField] private Vector3 defaultLookRotationOffset;
    [Tooltip("How fast (degrees/sec) the rotating target turns to face its look target. Set to 0 to snap instantly.")]
    [SerializeField] private float rotationSpeed = 360f;
    [Tooltip("How fast (units/sec) a target must move to be considered 'moving' rather than stationary")]
    [SerializeField] private float movementThreshold = 0.01f;
    [Tooltip("How often (in seconds) to rescan for targets in range. Set to 0 to scan every frame")]
    [SerializeField] private float scanInterval = 0.2f;

    [Header("Attention And Tracking")]
    [Tooltip("How long after a target has stopped moving before the looker looses interest, after interest is lost, retargets next nearest, or returns back to default if no others are found (Set this to 0 to never lose interest)")]
    public float attentionSpan = 0;
    [Tooltip("If enabled, only look at things that are not obstructed")]
    public bool requireLineOfSight = false;
    [Tooltip("These layers block line of sight")]
    public LayerMask lineOfSightMask;
    [Tooltip("How large the line of sight sphere cast is, thinner means it'll be easier for it to see through tighter gaps")]
    public float lineOfSightSphereCastRadius;

    //=-----------------=
    // Private Variables
    //=-----------------=
    private Quaternion defaultLocalRotation;
    private float scanTimer;
    private readonly Dictionary<Transform, TargetTrackingData> trackedTargets = new Dictionary<Transform, TargetTrackingData>();

    private class TargetTrackingData
    {
        public Vector3 lastPosition;
        public float stationaryTime;
        public bool isMoving;
    }

    //=-----------------=
    // Reference Variables
    //=-----------------=
    [Header("References")]
    [Tooltip("The object to rotate to look at something")]
    [SerializeField] private Transform rotatingTarget;
    [Tooltip("The object the rotating target is trying to look at (This is set by code)")]
    [SerializeField] private Transform currentTarget;
    [Tooltip("All targetable objects that are in visible range of the rotating target (This is set by code)")]
    [SerializeField] private List<Transform> visibleTargets;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        if (rotatingTarget == null)
            rotatingTarget = transform;

        defaultLocalRotation = rotatingTarget.localRotation;

        if (visibleTargets == null)
            visibleTargets = new List<Transform>();
    }

    private void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            ScanForTargets();
            scanTimer = scanInterval;
        }

        UpdateTargetMovementTracking();

        SelectCurrentTarget();

        ApplyRotation();
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=

    private void ScanForTargets()
    {
        visibleTargets.Clear();

        LookAtTargetable[] allTargetables = FindObjectsOfType<LookAtTargetable>();
        for (int i = 0; i < allTargetables.Length; i++)
        {
            LookAtTargetable targetable = allTargetables[i];
            if (targetable == null) continue;

            Transform targetTransform = targetable.transform;

            float distance = Vector3.Distance(rotatingTarget.position, targetTransform.position);
            if (distance > targetRange)
                continue;

            if (requireLineOfSight && !HasLineOfSight(targetTransform))
                continue;

            if (!visibleTargets.Contains(targetTransform))
                visibleTargets.Add(targetTransform);

            if (!trackedTargets.ContainsKey(targetTransform))
            {
                trackedTargets[targetTransform] = new TargetTrackingData
                {
                    lastPosition = targetTransform.position,
                    stationaryTime = 0f,
                    isMoving = false
                };
            }
        }

        if (trackedTargets.Count > 0)
        {
            List<Transform> toRemove = null;
            foreach (var kvp in trackedTargets)
            {
                if (!visibleTargets.Contains(kvp.Key))
                {
                    if (toRemove == null) toRemove = new List<Transform>();
                    toRemove.Add(kvp.Key);
                }
            }

            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                    trackedTargets.Remove(toRemove[i]);
            }
        }
    }

    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = rotatingTarget.position;
        Vector3 offset = target.position - origin;
        float distance = offset.magnitude;

        if (distance <= 0.001f)
            return true;

        Vector3 direction = offset / distance;

        return !Physics.SphereCast(origin, lineOfSightSphereCastRadius, direction, out _, distance, lineOfSightMask);
    }

    private void UpdateTargetMovementTracking()
    {
        for (int i = 0; i < visibleTargets.Count; i++)
        {
            Transform t = visibleTargets[i];
            if (t == null) continue;
            if (!trackedTargets.TryGetValue(t, out TargetTrackingData data)) continue;

            float distanceMoved = Vector3.Distance(t.position, data.lastPosition);
            float speed = Time.deltaTime > 0f ? distanceMoved / Time.deltaTime : 0f;

            if (speed > movementThreshold)
            {
                data.isMoving = true;
                data.stationaryTime = 0f;
            }
            else
            {
                data.isMoving = false;
                data.stationaryTime += Time.deltaTime;
            }

            data.lastPosition = t.position;
        }
    }

    private void SelectCurrentTarget()
    {
        Transform best = null;
        bool bestIsMoving = false;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < visibleTargets.Count; i++)
        {
            Transform t = visibleTargets[i];
            if (t == null) continue;
            if (!trackedTargets.TryGetValue(t, out TargetTrackingData data)) continue;

            if (attentionSpan > 0f && data.stationaryTime > attentionSpan)
                continue;

            float distance = Vector3.Distance(rotatingTarget.position, t.position);

            bool isBetter = best == null
                || (data.isMoving && !bestIsMoving)
                || (data.isMoving == bestIsMoving && distance < bestDistance);

            if (isBetter)
            {
                best = t;
                bestIsMoving = data.isMoving;
                bestDistance = distance;
            }
        }

        currentTarget = best;
    }

    private void ApplyRotation()
    {
        Quaternion desiredLocalRotation;

        if (currentTarget != null)
        {
            Vector3 directionToTarget = currentTarget.position - rotatingTarget.position;

            if (directionToTarget.sqrMagnitude < 0.0001f)
            {
                desiredLocalRotation = rotatingTarget.localRotation;
            }
            else
            {
                Quaternion desiredWorldRotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up) * Quaternion.Euler(defaultLookRotationOffset);

                desiredLocalRotation = rotatingTarget.parent != null
                    ? Quaternion.Inverse(rotatingTarget.parent.rotation) * desiredWorldRotation
                    : desiredWorldRotation;
            }
        }
        else
        {
            desiredLocalRotation = defaultLocalRotation;
        }

        Vector3 currentLocalEuler = rotatingTarget.localRotation.eulerAngles;
        Vector3 desiredLocalEuler = desiredLocalRotation.eulerAngles;

        Vector3 finalLocalEuler = new Vector3(
            enableXRotation ? desiredLocalEuler.x : currentLocalEuler.x,
            enableYRotation ? desiredLocalEuler.y : currentLocalEuler.y,
            enableZRotation ? desiredLocalEuler.z : currentLocalEuler.z
        );

        Quaternion finalLocalRotation = Quaternion.Euler(finalLocalEuler);

        rotatingTarget.localRotation = rotationSpeed <= 0f
            ? finalLocalRotation
            : Quaternion.RotateTowards(rotatingTarget.localRotation, finalLocalRotation, rotationSpeed * Time.deltaTime);
    }

    //=-----------------=
    // External Functions
    //=-----------------=
}