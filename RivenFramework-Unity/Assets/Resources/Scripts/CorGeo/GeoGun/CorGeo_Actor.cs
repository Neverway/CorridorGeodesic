//==========================================( Neverway 2025 )=========================================================//
// Author
//  Connorses
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using UnityEngine;

public class CorGeo_Actor : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Check this if the actor can move around")]
    [SerializeField] public bool dynamic = false;
    [Tooltip("Set this true if the actor is a child of something, and shouldn't be moved by the rift")]
    [SerializeField] public bool isParentedIgnoreOffsets = false;
    [Tooltip("If enabled, this object will not be disabled in a fully collapsed null-space")]
    [SerializeField] public bool activeInNullSpace = false;
    [Tooltip("Uncheck this if the object has a special death animation")]
    [SerializeField] public bool destroyedInKillTrigger = true;

    public bool logHome;
    
    //todo: Either reimplement crushInNullSpace, or get rid of it. I'm considering replacing it with something like "dynamicCrushable" since it only applies to dynamic actors anyway.
    //[Tooltip("If true, this actor gets distorted when inside nullspace, for example a cube that's not held by player")]
    //[SerializeField] public bool crushInNullSpace = true;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public event Action OnRiftRestore;
    [Tooltip("If enabled, this object will print logs for which 'space' it's currently in when a rift is active")]
    public bool debugLogSpaceData;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    [Header("Debugging")]
    [Tooltip("Used to restore static actors back to their initial position when uncollapsing rifts")]
    [SerializeField] private Vector3 homePosition;
    [Tooltip("Used to restore static actors back to their initial scale when uncollapsing rifts")]
    [SerializeField] private Vector3 homeScale;
    [Tooltip("Used to restore static actors back to their initial parent object when uncollapsing rifts")]
    [SerializeField] private Transform homeParent;
    [Tooltip("Enabled when an object is picked up by a pawn, this prevents the object from being moved during rift movements, otherwise the object would be pulled out of their hands")]
    [SerializeField] public bool isHeld = false;
    [Tooltip("Used to keep track of if this game object should be re-enabled in the hierarchy when resetting rifts")]
    public bool wasActive;
    public enum Space { None, A, B, Null }
    public Space space = Space.None;
    [Tooltip("The velocity of the object before it was frozen by a rift movement")]
    private Vector3 previousVelocity;
    
    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    new private Rigidbody rigidbody;

    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start ()
    {
        if (logHome) print($"setting parent to {homeParent}");
        // Find references
        rigidbody = GetComponent<Rigidbody>();
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

    
    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
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
    
    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Called by the GeoGun when a rift is reset
    /// This resets the actors back to the transform state they were in prior to being affected by a rift.
    /// It also moved dynamic actors relative to rift-space.
    /// </summary>
    public void GoHome ()
    {
        if (logHome) print($"setting parent to {homeParent} from home func");
        OnRiftRestore?.Invoke();
        if (isParentedIgnoreOffsets) return;
        transform.SetParent (homeParent);
        transform.localScale = homeScale;
        if (dynamic)
        {
            space = Space.None;
            return;
        }
        gameObject.SetActive(true); //todo: make the special cases where this SetActive doesn't apply
        transform.position = homePosition;
        space = Space.None;
    }
    
    /// <summary>
    /// Finds which space (A/B/Null) the actor is in and sets the actor's space variable accordingly.
    /// </summary>
    public void DetermineRiftSpace ()
    {
        if (GI_RiftManager.planeB.GetDistanceToPoint (transform.position) < 0)
        {
            space = Space.B;
            if (debugLogSpaceData)
            {
                Debug.Log ("B Space");
            }
            return;
        }
        if (GI_RiftManager.planeA.GetDistanceToPoint (transform.position) < 0)
        {
            space = Space.A;
            if (debugLogSpaceData)
            {
                Debug.Log ("A Space");
            }
            return;
        }
        if (debugLogSpaceData)
        {
            Debug.Log ("Null Space");
        }
        space = Space.Null;
    }

    
    #endregion
}
