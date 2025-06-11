//==========================================( Neverway 2025 )=========================================================//
// Author
//  Connorses
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a prefab object on a timer, and spawns it again if it is destroyed.
/// </summary>
public class Prop_Respawner : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] protected float spawnDelay;
    [Tooltip("Wait for a DestroySpawnedObject call before spawning the first object.")]
    [SerializeField] protected bool waitForRespawn = false;
    [Tooltip("If false, the spawner will always set waitForRespawn when the spawned object is destroyed.")]
    [SerializeField] protected bool autoRespawn = true;
    public LogicInput<bool> respawnProp = new(false);

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/

    
    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    protected Coroutine spawnWorker;
    protected GameObject spawnedObject { get; set; }

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] public GameObject propPrefab;
    
    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        // TODO: CallOnSourceChange needs HasLogicOutputSource to fix possible null refs for unlinked logic I/Os
        // Using this 'if' statement as a quick fix for now ~Liz
        if (respawnProp.HasLogicOutputSource is false) return;
        respawnProp.CallOnSourceChanged(RespawnProp);
    }

    private void Update()
    {
        if (waitForRespawn) return;
        
        if (spawnedObject == null && spawnWorker == null)
        {
            spawnWorker = StartCoroutine(SpawnWorker());
        }
    }

    private void OnDisable ()
    {
        if (spawnWorker != null)
        {
            StopCoroutine(spawnWorker);
        }
        spawnWorker = null;
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    protected virtual IEnumerator SpawnWorker()
    {
        yield return new WaitForSeconds(spawnDelay);
        spawnedObject = Instantiate(propPrefab, transform.position, transform.rotation);
        spawnWorker = null;
        if (autoRespawn == false)
        {
            waitForRespawn = true;
        }
    }
    
    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    /// <summary>
    /// Destroy the spawned object so that it will respawn.
    /// </summary>
    private void DestroySpawnedObject()
    {
        StartCoroutine(DestroyWorker());
    }

    /// <summary>
    /// Destroy the object after moving it to trigger OnTriggerExit for any objects it's inside of.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DestroyWorker()
    {
        waitForRespawn = false;
        if (spawnedObject == null)
        {
            yield break;
        }

        //Move the spawne dobject far away, triggering OnTriggerExit for anything it may have been inside of
        spawnedObject.transform.position = transform.position + Vector3.one * 10000f;
        //wait one frame, then destroy object
        yield return null;
        Destroy(spawnedObject);
    }
    
    
    private void RespawnProp()
    {
        if (!respawnProp.Get()) return;
        
        DestroySpawnedObject();
        if (spawnWorker is null) spawnWorker = StartCoroutine(SpawnWorker());
    }

    #endregion
}
