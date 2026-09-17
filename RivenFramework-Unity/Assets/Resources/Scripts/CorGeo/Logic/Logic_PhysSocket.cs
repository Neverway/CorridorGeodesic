//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class Logic_PhysSocket : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public LogicInput<Rigidbody> connectedBody;
    public LogicOutput<bool> socketConnected = new(false);


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private bool socketJointWasConnected;
    public bool socketIsInvulnerable;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public GameObject spawnOnSocketBroken;
    public float destroyBrokenFXAfter=1;
    public JointType referenceJointType;
    public ConfigurableJoint referenceJoint;
    private ConfigurableJoint socketJoint;

    [Serializable]
    public enum JointType
    {
        ConfigurableJoint,
        HingeJoint
    }

    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        connectedBody.CallOnSourceChanged(SetConnectedBody);
        if (connectedBody.HasLogicOutputSource) SetConnectedBody();
    }

    private void Update()
    {
        socketConnected.Set(connectedBody == null);
        
        if (socketJoint == null && socketJointWasConnected)
        {
            socketJointWasConnected = false;
            var brokenFX = Instantiate(spawnOnSocketBroken, position:transform.position, rotation:transform.rotation, parent:null);
            Destroy(brokenFX, destroyBrokenFXAfter);
            connectedBody = null;
        }

        if (socketIsInvulnerable && socketJoint != null)
        {
            socketJoint.breakTorque = Single.PositiveInfinity;
            socketJoint.breakForce = Single.PositiveInfinity;
        }
        else if (!socketIsInvulnerable && socketJoint != null)
        {
            socketJoint.breakTorque = referenceJoint.breakTorque;
            socketJoint.breakForce = referenceJoint.breakForce;
        }
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void SetConnectedBody()
    {
        InitSocketJoint();
        socketJoint.connectedBody = connectedBody;
        connectedBody.Get().GetComponent<Object_PhysPickupAdvanced>().breakableAnchorPin = socketJoint;
        socketJointWasConnected = true;
    }

    private void InitSocketJoint()
    {
        if (socketJoint == null)
        {
            switch (referenceJointType)
            {
                case JointType.ConfigurableJoint:
                    socketJoint = gameObject.AddComponent<ConfigurableJoint>();
                    socketJoint.CloneFrom(referenceJoint);
                    break;
                case JointType.HingeJoint:
                    //socketJoint = gameObject.AddComponent<HingeJoint>();
                    //socketJoint.CloneFrom(referenceJoint);
                    break;
            }
        }
    }
    

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void SetSocketInvulnerability(bool _socketInvulnerable)
    {
        socketIsInvulnerable = _socketInvulnerable;
    }

    #endregion
}
