//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using Unity.Collections;
using UnityEngine;

public class CorGeo_VolumeFizzler : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private string buddyID;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public GameObject fizzleDust;
    public GameObject fizzleParty;


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    [Todo("The actor id is used to check if the object is a phys prop... this is stupid, past me is stupid. Pls fix ~Liz")]
    private void OnTriggerEnter(Collider _other)
    {
        //base.OnTriggerEnter(_other); // Call the base class method
        var actor = _other.GetComponentInChildren<Actor>();
        if (!actor) actor = _other.GetComponentInParent<Actor>();
        if (actor)
        {
            print("1");
            LemonBuddyTracker buddy = actor.GetComponentInChildren<LemonBuddyTracker>();
            if (buddy != null)
            {
                print("2");
                buddy.OnBuddyDestroyed();
                Instantiate(fizzleParty, actor.transform.position, actor.transform.rotation, null);
                Destroy(actor.gameObject);
            }
            else if (actor.id.Contains("Phys"))
            {
                print("3");
                Instantiate(fizzleDust, actor.transform.position, actor.transform.rotation, null);
                Destroy(actor.gameObject);
            }
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
