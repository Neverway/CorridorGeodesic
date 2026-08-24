//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using RivenFramework;

public class FPPawnActions : PawnActions
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private RaycastHit slopeHit;
    public bool isCrouching;
    private GameObject viewCamera;
    private bool isBufferingJump;
    private bool hasJumped;
    private bool hasCoyoteGrace;
    private bool wasOnGroundLastFrame;
    private object pauseToken; // Used to track player pause state when starting camera sequences
    private Vector3 lastViewCamPos; // Used to store view pos when starting camera sequences
    private Vector3 lastViewCamRot; // Used to store view rot when starting camera sequences
    private Transform cameraParent;
    private bool cameraSequenceInProgress;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    [SerializeField] private float cameraTweenDuration = 1f;
    [SerializeField] private Ease cameraTweenEase = Ease.InOutSine;
    private Sequence cameraSequence;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Make the pawn move, using velocity, in a specified direction
    /// </summary>
    /// <param name="_pawn">A reference to the owning pawn</param>
    /// <param name="_rigidbody">A reference to the owning rigidbody</param>
    /// <param name="_direction">The direction to move in (x-axis is left/right, y-axis is forward/backward, and z-axis is up/down (which is only really used for flying enemies))</param>
    /// <param name="_speed">The speed to move the pawn at (set this to 0 to just use the stats movement speed)</param>
    public void Move(FPPawn _pawn, Vector3 _direction, float _speed=0)
    {
        //if (GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _direction, _speed })) return;
        
        if (_speed == 0)
        {
            _speed = ((FPPawnStats)_pawn.currentStats).movementSpeed;
        }

        var rigidbody = _pawn.GetComponent<Rigidbody>();
        
        // Make sure that the axis passed for the direction are always relative to the direction the pawn is facing
        var localMoveDirection = _pawn.transform.right * _direction.x + _pawn.transform.up * _direction.y + _pawn.transform.forward * _direction.z;
        var currentVelocity = rigidbody.velocity;
        
        // Get desired velocities
        var desiredGroundVelocity = localMoveDirection.normalized * _speed;
        IsOnSlope(_pawn); // Calculate IsOnSlope to get the result of slopeHit
        var slopMoveDirection = Vector3.ProjectOnPlane(localMoveDirection, slopeHit.normal);
        var desiredSlopeVelocity = slopMoveDirection * _speed;
        var desiredAirVelocity = localMoveDirection.normalized * (_speed * ((FPPawnStats)_pawn.currentStats).airMovementMultiplier);
        var desiredCrouchVelocity = localMoveDirection.normalized * (_speed * ((FPPawnStats)_pawn.currentStats).crouchMovementMultiplier);
        
        // Define acceleration rates
        var groundAccelerationRate = ((FPPawnStats)_pawn.currentStats).groundAccelerationRate;
        var slopeAccelerationRate = ((FPPawnStats)_pawn.currentStats).slopeAccelerationRate;
        var airAccelerationRate = ((FPPawnStats)_pawn.currentStats).airAccelerationRate;
        
        // Landing reset
        if (IsOnGround(_pawn) && !wasOnGroundLastFrame)
        {
            hasJumped = false;
        }
        
        // Update coyote time
        if (!IsOnGround(_pawn) && wasOnGroundLastFrame && !hasCoyoteGrace)
        {
            GameInstance.SendCoroutine(CoyoteTime(_pawn));
        }
        wasOnGroundLastFrame = IsOnGround(_pawn);
        
        // Ground Movement
        if (IsOnGround(_pawn) && !IsOnSlope(_pawn) && !isCrouching)
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).groundDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredGroundVelocity.x && desiredGroundVelocity.x > 0f || currentVelocity.x > desiredGroundVelocity.x && desiredGroundVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredGroundVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredGroundVelocity.y && desiredGroundVelocity.y > 0f || currentVelocity.y > desiredGroundVelocity.y && desiredGroundVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredGroundVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredGroundVelocity.z && desiredGroundVelocity.z > 0f || currentVelocity.z > desiredGroundVelocity.z && desiredGroundVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredGroundVelocity.z*groundAccelerationRate);
            }
        }
        // Crouch Movement
        else if (IsOnGround(_pawn) && !IsOnSlope(_pawn) && isCrouching)
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).groundDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredCrouchVelocity.x && desiredCrouchVelocity.x > 0f || currentVelocity.x > desiredCrouchVelocity.x && desiredCrouchVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredCrouchVelocity.x*groundAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredCrouchVelocity.y && desiredCrouchVelocity.y > 0f || currentVelocity.y > desiredCrouchVelocity.y && desiredCrouchVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredCrouchVelocity.y*groundAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredCrouchVelocity.z && desiredCrouchVelocity.z > 0f || currentVelocity.z > desiredCrouchVelocity.z && desiredCrouchVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredCrouchVelocity.z*groundAccelerationRate);
            }
        }
        // Slope Movement
        else if (IsOnGround(_pawn) && IsOnSlope(_pawn))
        {
            rigidbody.useGravity = false;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).slopeDrag;
            // if current is less than target and target is positive, or current is greater than target and target is negative
            if (currentVelocity.x < desiredSlopeVelocity.x && desiredSlopeVelocity.x > 0f || currentVelocity.x > desiredSlopeVelocity.x && desiredSlopeVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredSlopeVelocity.x*slopeAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredSlopeVelocity.y && desiredSlopeVelocity.y > 0f || currentVelocity.y > desiredSlopeVelocity.y && desiredSlopeVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredSlopeVelocity.y*slopeAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredSlopeVelocity.z && desiredSlopeVelocity.z > 0f || currentVelocity.z > desiredSlopeVelocity.z && desiredSlopeVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredSlopeVelocity.z*slopeAccelerationRate);
            }
        }
        // Air Movement
        else
        {
            rigidbody.useGravity = true;
            rigidbody.drag = ((FPPawnStats)_pawn.currentStats).airDrag;
            
            // if current is less than target and target is positive, or current is greater than target and target is negative
            // THIS IS THE OLD AIR MOVEMENT CODE THAT COULD FEEL GUMMY
            /*
            if (currentVelocity.x < desiredAirVelocity.x && desiredAirVelocity.x > 0f || currentVelocity.x > desiredAirVelocity.x && desiredAirVelocity.x < 0f )
            {
                rigidbody.velocity += new Vector3(desiredAirVelocity.x*airAccelerationRate, 0, 0);
            }
            if (currentVelocity.y < desiredAirVelocity.y && desiredAirVelocity.y > 0f || currentVelocity.y > desiredAirVelocity.y && desiredAirVelocity.y < 0f )
            {
                rigidbody.velocity += new Vector3(0, desiredAirVelocity.y*airAccelerationRate, 0);
            }
            if (currentVelocity.z < desiredAirVelocity.z && desiredAirVelocity.z > 0f || currentVelocity.z > desiredAirVelocity.z && desiredAirVelocity.z < 0f )
            {
                rigidbody.velocity += new Vector3(0, 0, desiredAirVelocity.z*airAccelerationRate);
            }*/
            
            // Quake style wishing acceleration
            if (localMoveDirection.sqrMagnitude > 0f)
            {
                var wishDir = new Vector3(localMoveDirection.x, 0, localMoveDirection.z).normalized;
                float wishSpeed = _speed * ((FPPawnStats)_pawn.currentStats).airMovementMultiplier;
                var horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
    
                float currentSpeedInWishDir = Vector3.Dot(horizontalVelocity, wishDir);
                float addSpeed = wishSpeed - currentSpeedInWishDir;
                if (addSpeed <= 0f) return;

                float accelAmount = ((FPPawnStats)_pawn.currentStats).airAccelerationRate * wishSpeed * Time.fixedDeltaTime;
                if (accelAmount > addSpeed) accelAmount = addSpeed;
                rigidbody.velocity += new Vector3(wishDir.x * accelAmount, 0, wishDir.z * accelAmount);
            }
        }
    }
    
    /// <summary>
    /// TODO Make the pawn move in a direct path to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveTo(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// TODO Make the pawn path-find it's way to a specified position
    /// </summary>
    /// <param name="_position"></param>
    public void MoveToSmart(Vector3 _position)
    {
        
    }
    
    /// <summary>
    /// Make the pawn turn to face a specified amount
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_direction">The direction to rotate in (x-axis is left/right, y-axis is up/down)</param>
    public void FaceTowardsDirection(FPPawn _pawn, Transform _viewPoint, Vector2 _direction, float _platformYOffset = 0f)
    {
        //if(GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn,  _viewPoint, _direction })) return;
        
        _viewPoint.localRotation = Quaternion.Euler(_direction.x, 0, 0); // Rotate the head for up/down
        _pawn.transform.rotation = Quaternion.Euler(0, _direction.y + _platformYOffset, 0); // Rotate the body for left/right
    }
    
    /// <summary>
    /// Make the pawn face at a specified point
    /// </summary>
    /// <param name="_pawn">A reference to the root of the pawn (this is needed to rotate the body to look left and right)</param>
    /// <param name="_viewPoint">A reference to the object that represents the head of the pawn (this is needed to rotate the head to look up and down)</param>
    /// <param name="_position"></param>
    /// <param name="_speed"></param>
    public void FaceTowardsPosition(FPPawn _pawn, Transform _viewPoint, Vector3 _position, float _speed)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _viewPoint, _position, _speed });
        
        var vectorToTarget = _pawn.transform.position - _position;

        // Rotate the body for left/right
        var bodyLookRotation = Mathf.Atan2(vectorToTarget.x, vectorToTarget.z) * Mathf.Rad2Deg;
        _pawn.transform.rotation = Quaternion.Euler(0, bodyLookRotation+180, 0);
        
        // Rotate the head for up/down
        var headLookRotation = Quaternion.LookRotation(vectorToTarget, _pawn.transform.up).eulerAngles;
        var desiredRotation = new Vector3(-headLookRotation.x, headLookRotation.y + 180, headLookRotation.z);
        _viewPoint.transform.eulerAngles = desiredRotation;
    }
    
    /// <summary>
    /// Make the pawn jump using a force applied to the rigidbody
    /// </summary>
    /// <param name="_pawn">A reference to the pawn to get its jump force & IsOnGround state</param>
    /// <param name="_rigidbody"></param>
    public void Jump(FPPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });

        if (IsOnGround(_pawn) is false && hasCoyoteGrace is false)
        {
            if (!isBufferingJump)
                GameInstance.SendCoroutine(BufferJump(_pawn));
            return;
        }
        if (hasJumped) return;
        
        hasJumped = true;
        hasCoyoteGrace = false;
        
        var rigidbody = _pawn.GetComponent<Rigidbody>();
        if (IsOnGround(_pawn)) ApplyABH(_pawn, rigidbody);
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        rigidbody.AddForce(Vector3.up * ((FPPawnStats)_pawn.currentStats).jumpForce, ForceMode.Impulse);
    }

    private IEnumerator BufferJump(FPPawn _pawn)
    {
        isBufferingJump = true;
        float time = Time.unscaledTime;
        float bufferDuration = 0.2f;
        
        while (bufferDuration+time > Time.unscaledTime)
        {
            if (IsOnGround(_pawn))
            {
                Jump(_pawn);
                isBufferingJump = false;
                yield break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
        isBufferingJump = false;
    }
    
    private void ApplyABH(FPPawn _pawn, Rigidbody _rigidbody)
    {
        var stats = (FPPawnStats)_pawn.currentStats;
    
        var horizontalVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
    
        float speedCap = isCrouching ? stats.abhCrouchSpeedCap : stats.abhSpeedCap;
        if (currentSpeed <= speedCap) return;
        
        float excessSpeed = currentSpeed - speedCap;
        
        float dot = Vector3.Dot(horizontalVelocity.normalized, _pawn.transform.forward);

        if (dot >= 0f) return;

        float boostAmount = excessSpeed * stats.abhCorrectionStrength;

        _rigidbody.velocity += horizontalVelocity.normalized * boostAmount;
    }
    
    private IEnumerator CoyoteTime(FPPawn _pawn)
    {
        hasCoyoteGrace = true;
        yield return new WaitForSeconds(_pawn.FPCurrentStats.coyoteTime);
        hasCoyoteGrace = false;
    }
    
    /// <summary>
    /// Make the pawn crouch by reducing its capsule collider height (and also trigger Move to change to a crouching movement speed)
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_enable"></param>
    public void Crouch(FPPawn _pawn, bool _enable)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn, _enable });
        var stats = (FPPawnStats)_pawn.currentStats;
        var colliderTransform = _pawn.bodyCollider.transform;
        
        if (_enable && isCrouching is false)
        {
            float standingHeight = 1;
            float crouchedHeight = standingHeight - stats.crouchDistance;
            float scaleY = crouchedHeight / standingHeight;

            colliderTransform.localScale = new Vector3(colliderTransform.localScale.x, scaleY, colliderTransform.localScale.z);

            float heightDelta = standingHeight - crouchedHeight;
            colliderTransform.localPosition += new Vector3(0, heightDelta * 1f, 0);
            isCrouching = true;
        }
        if (_enable is false && isCrouching && IsHeadClear(_pawn))
        {
            float standingHeight = 1;
            float crouchedHeight = standingHeight - stats.crouchDistance;
            float heightDelta = standingHeight - crouchedHeight;

            colliderTransform.localScale = new Vector3(colliderTransform.localScale.x, 1f, colliderTransform.localScale.z);

            colliderTransform.localPosition -= new Vector3(0, heightDelta * 1f, 0);

            _pawn.transform.position += new Vector3(0, stats.crouchDistance, 0);
            isCrouching = false;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void Interact(FPPawn _pawn, GameObject _interactionTrigger, Transform _viewPoint)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn,  _interactionTrigger, _viewPoint });
        
        var interaction = Object.Instantiate(_interactionTrigger, _viewPoint);
        interaction.transform.GetChild(0).GetComponent<VolumeTriggerInteraction>().owningPawn = _pawn;
        Object.Destroy(interaction,  0.2f);
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="_action"></param>
    public void ItemUseAction(Pawn_Inventory _inventory, int _action = 0, string _mode = "press")
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _inventory, _action, _mode });
        
        var item = _inventory.GetComponentInChildren<Item>(false);
        if (item is null) return;

        switch (_action)
        {
            case 0:
                item.UsePrimary(_mode);
                break;
            case 1:
                item.UseSecondary(_mode);
                break;
            case 2:
                item.UseTertiary(_mode);
                break;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public void SwitchItem()
    {
        
    }
    
    public bool IsHeadClear(FPPawn _pawn)
    {
        RaycastHit hit;
        if (Physics.SphereCast(_pawn.transform.position + ((FPPawnStats)_pawn.currentStats).headCheckOffset, ((FPPawnStats)_pawn.currentStats).headCheckRadius, _pawn.transform.up, out hit, ((FPPawnStats)_pawn.currentStats).headCheckDistance, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }
        return true;
    }
    
    public bool IsOnGround(FPPawn _pawn)
    {
        // Move the ground check position upwards if the pawn is crouching to account for their change in height
        Vector3 crouchingOffset = new Vector3(0,0,0);
        if (isCrouching) crouchingOffset = new Vector3(0, ((FPPawnStats)_pawn.currentStats).crouchDistance, 0);
        
        return Physics.CheckSphere(_pawn.transform.position - ((FPPawnStats)_pawn.currentStats).groundCheckOffset + crouchingOffset, ((FPPawnStats)_pawn.currentStats).groundCheckRadius, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore);
    }

    public bool IsOnSlope(FPPawn _pawn)
    {
        /*
        This function does not account for crouching offsets. Meaning if a pawn is crouched, the slope detection will likely fail and the pawn will slip off the slope.
        This is a bug, but I'm deciding to keep it in since it's super fun to be able to crouch when falling at a slope to slide down it!
        If this needs to be patched out for any reason, update this function to account for the crouch offset. If you're not sure how to do that, check IsOnGround function above. It correctly accounts for the crouch offset.
        Happy sliding! ~Liz
        //*/
        if (Physics.Raycast(_pawn.transform.position, Vector3.down, out slopeHit, ((FPPawnStats)_pawn.currentStats).slopeCheckDistance, ((FPPawnStats)_pawn.currentStats).groundMask, QueryTriggerInteraction.Ignore))
        {
            return slopeHit.normal != Vector3.up;
        }

        return false;
    }

    public void EnableViewCamera(FPPawn _pawn, bool _setActive)
    {
        if (viewCamera is null)
        {
            // Try to get a view camera
            viewCamera =_pawn.GetComponentInChildren<Camera>(true).gameObject;
            if (viewCamera is null) return;
        }
        
        viewCamera.SetActive(_setActive);
    }

    /// <summary>
    /// Disables pawn movement and lerps their view camera to a location
    /// </summary>
    public void StartCameraSequence(FPPawn _pawn, Transform _cameraTransformTarget)
    {
        if (cameraSequenceInProgress) return;
        cameraSequenceInProgress = true;
        viewCamera =_pawn.GetComponentInChildren<Camera>(true).gameObject;

        _pawn.Unpause(pauseToken);
        
        cameraParent = viewCamera.transform.parent;
        lastViewCamPos = viewCamera.gameObject.transform.position;
        lastViewCamRot = viewCamera.gameObject.transform.rotation.eulerAngles;
        viewCamera.transform.parent = null;
        
        // Tween camera to _cameraTransformTarget
        cameraSequence?.Kill();
        cameraSequence = DOTween.Sequence();
        cameraSequence.Join(viewCamera.transform.DOMove(_cameraTransformTarget.position, cameraTweenDuration).SetEase(cameraTweenEase));
        cameraSequence.Join(viewCamera.transform.DORotate(_cameraTransformTarget.eulerAngles, cameraTweenDuration).SetEase(cameraTweenEase));
    }

    /// <summary>
    /// Disables pawn movement and lerps their view camera to a location
    /// </summary>
    public void EndCameraSequence(FPPawn _pawn)
    {
        if (!cameraSequenceInProgress) return;

        _pawn.Unpause(pauseToken);
        
        // Tween camera to stored
        cameraSequence?.Kill();
        cameraSequence = DOTween.Sequence();
        cameraSequence.Join(viewCamera.transform.DOMove(lastViewCamPos, cameraTweenDuration).SetEase(cameraTweenEase));
        cameraSequence.Join(viewCamera.transform.DORotate(lastViewCamRot, cameraTweenDuration).SetEase(cameraTweenEase));
        cameraSequence.OnComplete(() =>
        {
            viewCamera.transform.parent = cameraParent;
            cameraSequenceInProgress = false;
        });
    }

    /// <summary>
    /// Clears and populates the lists of visible pawns
    /// </summary>
    /// <param name="_pawn"></param>
    /// <param name="_distance"></param>
    public void Look(FPPawn _pawn, float _distance)
    {
        // Clear the list of visible pawns
        _pawn.visiblePawns.Clear();
        _pawn.visibleHostiles.Clear();
        _pawn.visibleAllies.Clear();
        foreach (var target in Physics.OverlapSphere(_pawn.transform.position, _distance))
        {
            // Object is pawn
            var targetPawn = target.GetComponent(typeof(FPPawn)) as FPPawn;
            if (targetPawn)
            {
                if (targetPawn.gameObject == _pawn.gameObject) continue;
                // Pawn is not occluded by something
                //if (!Physics.Raycast(_pawn.viewPoint.transform.position, _pawn.transform.position - target.transform.position, 9999, _pawn.currentStats.groundMask))
                //{
                    // Add it to the list of visible pawns
                    _pawn.visiblePawns.Add(targetPawn);
                    // If it's an enemy, add it to the list of visible hostiles
                    if (_pawn.FPCurrentStats.opposedTeams.Contains(((FPPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleHostiles.Add(targetPawn);
                    }
                    // If it's a friend, add it to the list of visible allies
                    if (((FPPawnStats)_pawn.currentStats).alliedTeams.Contains(((FPPawnStats)targetPawn.currentStats).team))
                    {
                        _pawn.visibleAllies.Add(targetPawn);
                    }
                //}
            }
        }
    }
    
    public void Listen()
    {
        
    }
    
    public void ThrowPhysProp(FPPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var attachedObject = _pawn.physObjectAttachmentPoint.attachedObject;
        
        attachedObject.GetComponent<Rigidbody>().AddForce((viewCamera.transform.forward * ((FPPawnStats)_pawn.currentStats).throwForce));
        
        var physPickup = attachedObject.GetComponent<Object_PhysPickup>();
        if (physPickup) physPickup.Drop();
        else attachedObject.GetComponent<Object_PhysPickupAdvanced>().Drop();
    }

    public void DropPhysProp(FPPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var attachedObject = _pawn.physObjectAttachmentPoint.attachedObject;
        var physPickup = attachedObject.GetComponent<Object_PhysPickup>();
        if (physPickup) physPickup.Drop();
        else attachedObject.GetComponent<Object_PhysPickupAdvanced>().Drop();
    }

    public FPPawn GetClosest(FPPawn _pawn, List<Pawn> _pawns)
    {
        var closestDistance = 999999f;
        FPPawn closestPawn = null;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= closestDistance)
            {
                closestDistance = distanceToTarget;
                closestPawn = ((FPPawn)target);
            }
        }

        return closestPawn;
    }

    public float GetCollectiveAllyCourage(FPPawn _pawn, List<Pawn> _pawns)
    {
        float collectiveAllyCourage = 0;
        foreach (var target in _pawns)
        {
            var distanceToTarget = Vector3.Distance(_pawn.transform.position, target.transform.position);
            if (distanceToTarget <= ((FPPawnStats)_pawn.currentStats).comfortableAllyDistance)
            {
                collectiveAllyCourage += ((FPPawnStats)_pawn.currentStats).courage;
            }
        }
        /*foreach (var VARIABLE in COLLECTION)
        {
            Vector3.Distance(closestAlly.transform.position, _pawn.transform.position) > ((FPS_Stats)_pawn.stats).comfortableAllyDistance
        }*/
        return collectiveAllyCourage;
    }
    
    public void ItemSwapNext(FPPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchNext();
    }

    public void ItemSwapPrevious(FPPawn _pawn)
    {
        //GameInstance.Get<GI_ReplayEventTimeline>().RecordThisEvent(this, new object[]{ _pawn });
        
        var inventory = _pawn.GetComponentInChildren<Pawn_Inventory>();
        if (inventory is null) return;
        inventory.SwitchPreviouse();
    }
}
