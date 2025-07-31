//==========================================( Neverway 2025 )=========================================================//
// Author
//  Andre Blunt
//
// Contributors
//  Liz M., Connorses
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UProjectile : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("How long the projectile can exist before being destroyed")]
    [SerializeField] protected float lifetime;
    [Tooltip("The size of the projectile")]
    [SerializeField] protected float radius;
    [Tooltip("How fast the projectile moves")]
    [SerializeField] protected float moveSpeed;
    [Tooltip("Optional field that is used for a motion tween to fake the projectile being shot from the gun barrel.")]
    [SerializeField] private GameObject projectileGraphics;
    [Tooltip("What layers the projectile collides with")]
    [SerializeField] private LayerMask layerMask;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
