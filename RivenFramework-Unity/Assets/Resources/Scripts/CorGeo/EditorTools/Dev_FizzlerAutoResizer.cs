//==========================================( Neverway 2025 )=========================================================//
// Author
//  Andre Blunt
//
// Contributors
//  Errynei (Rehauled the script to provide more options and be more robust, as well as more comments)
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Dev_FizzlerAutoResizer : MonoBehaviour
{
#if UNITY_EDITOR
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
[Header("References (None can be null)")]

    [Tooltip("Reference to the first handle of the fizzler")]
    [SerializeField] private Transform fizzlerHandle1;

    [Tooltip("Reference to the second handle of the fizzler")]
    [SerializeField] private Transform fizzlerHandle2;

    [Tooltip("Reference to the volume of the fizzler, to be stretched between both handles")]
    [SerializeField] private Transform fizzlerVolume;

[Header("Auto Alignment Options")]
    [Tooltip("The local direction the fizzler is meant to travel in. (Snaps)")]
    [SerializeField] private Vector3 localFizzlerDirection = Vector3.zero;

    [Tooltip("Whether or not to automatically align the handles rotation to match the direction of the fizzler")]
    [SerializeField] private bool autoAlignHandles = true;

    [Tooltip("The rotation offset to apply to the handles after aligning rotation of handles to face each " +
        "other (In case their \"forward\" is not really forward")]
    [SerializeField] private Vector3 fizzlerEulerRotationOffset = Vector3.zero;


[Header("Fizzler Volume Sizing")]
    [Tooltip("The full height of the fizzler (before any padding is applied) \n(should be equal to height of the handles)")]
    [SerializeField] private float fizzlerHeight = 4f;

    [Tooltip("How thick the fizzler volume should be \n(the direction you walk through the fizzler)")]
    [SerializeField] private float volumeThickness = 0.05f;

    [Tooltip("The padding to reduce the size of the fizzler volume. \nX = width (distance from handles); \nY = height (distance from top and bottom of handles)")]
    [SerializeField] private Vector2 volumeSizePadding = Vector2.zero;


    //Old values of positions (Used to check if the positions and size of volume needs to be updated)
    private Vector3 oldVolumePos = Vector3.zero;
    private Vector3 oldHandle1Pos = Vector3.zero;
    private Vector3 oldHandle2Pos = Vector3.zero;
    private bool valueChanged = false;
    #endregion

    //True if there is a missing
    private bool HasMissingReference => fizzlerVolume == null || fizzlerHandle1 == null || fizzlerHandle2 == null;
    private bool VolumeOrHandlesNeedUpdate => valueChanged ||
                oldHandle1Pos != fizzlerHandle1.transform.position ||
                oldHandle2Pos != fizzlerHandle2.transform.position ||
                oldVolumePos != fizzlerVolume.transform.position;

    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void OnValidate() => valueChanged = !Application.isPlaying; //Set valueChanged flag only if not in play mode
    private void Update()
    {
        //Do not update anything during the game
        if (Application.isPlaying) return;

        //If there is a missing reference on this component, abort to avoid error
        if (HasMissingReference) return;

        //Update volume and handles if they need to be updated
        if (VolumeOrHandlesNeedUpdate)
        {
            FixHandlePositions();
            SetupVolumeSizeAndPosition();

            //Reset flags to avoid unecessary calls to setting up the volume and handles every frame
            ResetVolumeOrHandleNeedsUpdateFlags();
        }
        //Rotate handles to face each other in direction of fizzler
        AlignHandleRotations();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    /// <summary> Returns the given point snapped to the closest point on the line from 
    /// the local origin (0, 0, 0) in the direction of the fizzler (localFizzlerDirection) 
    /// in local space.
    /// (Used for restricting the handles movement to be lined up with the volume along the origin and defined local fizzler direction)
    /// </summary>
    private Vector3 SnapPointToFizzlerLine(Vector3 point)
        => localFizzlerDirection.normalized * GetDistanceAlongFizzlerLine(point);

    /// <summary> Returns the distance the given point is away from the local origin (0, 0, 0) along
    /// the line that intersects that origin in the direction of the fizzler (localFizzlerDirection)
    /// in local space
    /// </summary>
    private float GetDistanceAlongFizzlerLine(Vector3 point) 
        => Vector3.Dot(point, localFizzlerDirection.normalized);

    private void FixHandlePositions()
    {
        //Snap handles to the line intersecting local (0,0,0) in the direction: "localFizzlerDirection"
        fizzlerHandle1.localPosition = SnapPointToFizzlerLine(fizzlerHandle1.localPosition);
        fizzlerHandle2.localPosition = SnapPointToFizzlerLine(fizzlerHandle2.localPosition);

        //If handles cross paths, swap the references for the handles so that they can be properly rotated to face each other
        if (GetDistanceAlongFizzlerLine(fizzlerHandle1.localPosition) > GetDistanceAlongFizzlerLine(fizzlerHandle2.localPosition))
        {
            Transform temp = fizzlerHandle1;
            fizzlerHandle1 = fizzlerHandle2;
            fizzlerHandle2 = temp;
        }
    }

    /// <summary> Aligns both handle rotations to facing the direction of the fizzler (localFizzlerDirection)
    ///  as well as inwards towards each other
    /// </summary>
    private void AlignHandleRotations()
    {
        //Make handles face the direction of the fizzler
        fizzlerHandle1.localRotation = Quaternion.LookRotation(localFizzlerDirection, Vector3.up);
        fizzlerHandle2.localRotation = Quaternion.LookRotation(-localFizzlerDirection, Vector3.up);

        fizzlerHandle1.localRotation *= Quaternion.Euler(fizzlerEulerRotationOffset);
        fizzlerHandle2.localRotation *= Quaternion.Euler(fizzlerEulerRotationOffset);
    }

    private void SetupVolumeSizeAndPosition()
    {
        //Fizzler set to average of both handle positions
        fizzlerVolume.localPosition = (fizzlerHandle1.localPosition + fizzlerHandle2.localPosition) * 0.5f;
        //Since handles origins are at the bottom, shift center up based on height
        fizzlerVolume.localPosition += Vector3.up * fizzlerHeight * 0.5f;

        //Scale volume to fit the area between handles
        float distanceBetweenHandles = Vector3.Distance(fizzlerHandle1.localPosition, fizzlerHandle2.localPosition);
        fizzlerVolume.localScale = new Vector3(distanceBetweenHandles, fizzlerHeight, volumeThickness);
        //Reduce final volume size by padding, and clamp volume size so that it is never smaller than size 0
        fizzlerVolume.localScale -= new Vector3(volumeSizePadding.x, volumeSizePadding.y, 0) * 2f;
        fizzlerVolume.localScale = Vector3.Max(fizzlerVolume.localScale, Vector3.zero);
    }

    private void ResetVolumeOrHandleNeedsUpdateFlags()
    {
        valueChanged = false;
        oldHandle1Pos = fizzlerHandle1.position;
        oldHandle2Pos = fizzlerHandle2.position;
        oldVolumePos = fizzlerVolume.position;
    }

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/




    #endregion
#endif
}
