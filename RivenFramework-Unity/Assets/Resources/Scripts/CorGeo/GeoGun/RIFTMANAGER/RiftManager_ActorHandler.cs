//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M., Connorses, Errynei, Soulex
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

/// <summary>
/// ?????
/// </summary>
[Serializable]
public class RiftManager_ActorHandler : ILoggable
{
    /// <summary>
    /// Class constructor
    /// </summary>
    public RiftManager_ActorHandler(RiftManager riftManager)
    {
        this.riftManager = riftManager;
        EnableRuntimeLogging = riftManager.EnableRuntimeLogging;
    }
    
    
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public bool EnableRuntimeLogging { get; set; }


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("Link to parent class for logging")]
    private RiftManager riftManager;
    [Tooltip("A list of all CorGeo actors in the current level, every object with CorGeo_Actor adds itself here in their start method")]
    public static List<CorGeo_Actor> CorGeo_Actors = new List<CorGeo_Actor> { };


    #endregion


    #region=======================================( Functions )=======================================================//
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void RestoreActors()
    {
        foreach (CorGeo_Actor actor in CorGeo_Actors)
        {
            actor.GoHome();
        }
    }
    
    /// <summary>
    /// Calculate where an object in Null-Space should move to if the rift scales to the given percent.
    /// </summary>
    /// <param name="_position"></param>
    /// <param name="_newPercent"></param>
    /// <returns></returns>
    public Vector3 MovePositionWithNullSpace (Vector3 _position, float _newPercent)
    {
        
        //Calculate how far across null-space the transform is.
        float riftDistance = RiftManager.cutPlaneA.GetDistanceToPoint (_position);

        if (riftDistance == 0)
        {
            Debug.Log("EARLY EXIT");
            return _position;
        }

        float riftPercent = riftDistance / RiftManager.currentRiftWidth;
        //Calculate where the transform would be if null-space were not scaled.
        float newDistance = Mathf.Abs( riftPercent * (RiftManager.riftStartingWidth * _newPercent) );
        Vector3 answer = _position + ( RiftManager.riftNormal * (newDistance - riftDistance) );
        
        Debug.Log($"Pos {_position}, NPer {_newPercent}, Dis {riftDistance}, CPer {riftPercent}, CRW {RiftManager.currentRiftWidth}");
        return answer;
    }

    /// <summary>
    /// Calculate where an object in B-Space should move to if the rift scales to the given percent.
    /// </summary>
    /// <param name="_position"></param>
    /// <param name="_newPercent"></param>
    /// <returns></returns>
    public Vector3 MovePositionWithBSpace (Vector3 _position, float _newPercent)
    {
        float offset = Mathf.Abs(RiftManager.riftStartingWidth*RiftManager.currentRiftPercent)-Mathf.Abs(RiftManager.riftStartingWidth * _newPercent);

        return _position - (RiftManager.riftNormal * offset);
    }

    #endregion
}
