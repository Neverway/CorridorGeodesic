//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using RivenFramework;
using System;
using UnityEngine;

public class CorGeo_Fizzler : CorGeo_SliceableTriggerController
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public LogicInput<bool> inputDisableFizzler = new(false);

    [SerializeField] private string buddyID;
    public GameObject fizzleDust;
    public GameObject fizzleParty;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool IsFizzlerActive => !inputDisableFizzler.Get();

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private Item_Utility_Geogun geogun;

    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        if (inputDisableFizzler.HasLogicOutputSource) inputDisableFizzler.CallOnSourceChanged(InputDisableFizzlerChanged);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void InputDisableFizzlerChanged()
    {
        foreach(CorGeo_SlicedTriggerPart part in slicedTriggerParts)
        {
            part.GetComponent<Collider>().enabled = IsFizzlerActive;
            part.GetComponent<MeshRenderer>().enabled = IsFizzlerActive;
        }
    }
    
    private void ClearGeogunRifts()
    {
        if (geogun == null)
        {
            geogun = FindObjectOfType<Item_Utility_Geogun>();
            if (geogun == null) return;
        }
        geogun.DestroyMarkers();
    }

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    [Todo("The actor id is used to check if the object is a phys prop... this is stupid, past me is stupid. Pls fix ~Liz", TodoSeverity.Minor)]
    public override void OnTriggerPartEnter(Collider _other)
    {
        //Don't do anything if the fizzler is not active
        if (!IsFizzlerActive) return;

        //Clear rift if it is the player colliding with the fizzler
        if (_other.CompareTag("Pawn"))
        {
            ClearGeogunRifts();
            return;
        }

        //base.OnTriggerEnter(_other); // Call the base class method
        var actor = _other.GetComponentInChildren<Actor>();
        if (!actor) actor = _other.GetComponentInParent<Actor>();
        if (actor)
        {
            LemonBuddyTracker buddy = actor.GetComponentInChildren<LemonBuddyTracker>();
            if (buddy != null)
            {
                buddy.OnBuddyDestroyed();
                Instantiate(fizzleParty, actor.transform.position, actor.transform.rotation, null);
                Destroy(actor.gameObject);
            }
            else if (actor.id.Contains("Phys"))
            {
                Instantiate(fizzleDust, actor.transform.position, actor.transform.rotation, null);
                Destroy(actor.gameObject);
            }
        }
    }

    public override void OnTriggerPartExit(Collider _other) { }

    //These just call "InputDisableFizzlerChanged()" to update the visuals for them when there is a change to the trigger parts
    public override void OnPartAdded(CorGeo_SlicedTriggerPart part) => InputDisableFizzlerChanged();
    public override void OnPartRemoved(CorGeo_SlicedTriggerPart part) => InputDisableFizzlerChanged();

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    #endregion
}
