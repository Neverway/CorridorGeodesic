//==========================================( Neverway 2025 )=========================================================//
// Author
//  Connorses
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using System.Linq;
using UnityEngine;

public class CorGeo_Actor : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}

public class CorGeo_ActorData : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("If enabled, this object will not be disabled in a fully collapsed null-space")]
    [SerializeField] public bool activeInNullSpace = false;
    [Tooltip("Uncheck this if the object has a special death animation")]
    public bool destroyedInKillTrigger = true;
    
    [Header("Debugging")]
    [Tooltip("Used to restore static actors back to their initial position when uncollapsing rifts")]
    [SerializeField] public Vector3 homePosition;
    [Tooltip("Used to restore static actors back to their initial scale when uncollapsing rifts")]
    [SerializeField] public Vector3 homeScale;
    [Tooltip("Used to restore static actors back to their initial parent object when uncollapsing rifts")]
    [SerializeField] public Transform homeParent;
    [Tooltip("Check this if the actor can move around")]
    [SerializeField] public bool dynamic = false;
    // TODO: I don't understand what this is used for, can someone add a tooltip here? ~Liz
    // Nah. ~Connor
    [SerializeField] public bool crushInNullSpace = true;
    // TODO: This one doesn't make sense to me either. When do we not want to restore an actors transforms when undoing rifts? ~Liz
    [SerializeField] public bool isParentedIgnoreOffsets = false;

    [Tooltip("Enabled when an object is picked up by a pawn, this prevents the object from being moved during rift movements, otherwise the object would be pulled out of their hands")]
    public bool isHeld = false;
    public event Action OnRiftRestore;

    public enum Space { None, A, B, Null }

    public Space space = Space.None;
    
    [Tooltip("Used to keep track of if this game object should be re-enabled in the hierarchy when resetting rifts")]
    public bool wasActive;

    public bool debugLogData;

    //=-----------------=
    // Private Variables
    //=-----------------=
    // The velocity of the object before it was frozen by a rift movement
    private Vector3 previousVelocity;

    //=-----------------=
    // Reference Variables
    //=-----------------=
    new private Rigidbody rigidbody;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start ()
    {
        // Find references
        rigidbody= GetComponent<Rigidbody>();
        // Store initial transform data about this object, so it can be restored later when rifts are reset
        wasActive = gameObject.activeInHierarchy;
        homePosition = transform.position;
        homeScale = transform.localScale;
        homeParent = transform.parent;
        GI_RiftManager.CorGeo_Actors.Add(this);
        // Automatically avoid hiding lights when a rift is collapsed
        if (TryGetComponent<Light> (out Light light))
        {
            activeInNullSpace = true;
        }
    }

    private void OnDestroy ()
    {
        // Cleanly remove this from the list of tracked actors on the GeoGun when destoryed
        GI_RiftManager.CorGeo_Actors.Remove(this);
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    /// <summary>
    /// Called by the GeoGun when a rift is reset
    /// This resets the actors back to the transform state they were in prior to being affected by a rift
    /// </summary>
    /// 
    //todo: actually trigger GoHome and move the actor
    public void GoHome ()
    {
        OnRiftRestore?.Invoke();
        gameObject.SetActive(true); 
        if (isParentedIgnoreOffsets) return;
        transform.SetParent(homeParent);
        transform.localScale = homeScale;
        if (space == Space.Null && !dynamic)
        {
            transform.position = homePosition;
            return;
        }//todo: move this, or make the planes statics like they were previously
        if (space != Space.Null && GI_RiftManager.planeA.GetDistanceToPoint (transform.position) > 0)
        {
            if (!GI_RiftManager.riftActive) return;
            // Move actor away from collapse direction scaled by the rift timer's progress
            transform.position -= GI_RiftManager.currentRiftOffset;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Freeze ()
    {
        if (!rigidbody) return;

        previousVelocity = rigidbody.velocity;
        rigidbody.isKinematic = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void UnFreeze ()
    {
        if (!rigidbody) return;

        rigidbody.isKinematic = false;
        rigidbody.velocity = previousVelocity;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
