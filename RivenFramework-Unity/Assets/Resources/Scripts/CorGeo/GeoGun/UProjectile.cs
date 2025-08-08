//==========================================( Neverway 2025 )=========================================================//
// Author
//  Andre Blunt
//
// Contributors
//  Liz M., Connorses
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RivenFramework;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class UProjectile : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("How long the projectile can exist before being destroyed (Set to 0 for infinite)")]
    [SerializeField] protected float lifetime;
    [Tooltip("The size of the projectile")]
    [SerializeField] protected float radius;
    [Tooltip("How fast the projectile moves")]
    [SerializeField] protected float moveSpeed;
    [Tooltip("The direction the projectile is moving in (used for detecting raycast collisions and handling movement)")]
    [SerializeField] public Vector3 moveVector;
    [Tooltip("Optional field that is used for a motion tween to fake the projectile being shot from the gun barrel")]
    [SerializeField] protected GameObject projectileGraphics;
    [Tooltip("What layers the projectile collides with")]
    [SerializeField] protected LayerMask layerMask;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    [Tooltip("When true, the projectile no longer checks for collisions or updates its movement")]
    public bool disableMovement;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    protected Coroutine lifetimeCoroutine;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/


    #endregion


    #region=======================================( Functions )======================================================= //
    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    protected virtual void Start()
    {
        if (lifetime != 0)
        {
            lifetimeCoroutine = StartCoroutine(ILifetime());
        }
    }
    protected virtual void Update ()
    {
        if (disableMovement) return;

        moveVector = transform.forward * (moveSpeed * Time.deltaTime);
        if (HasCollided() is false) UpdateMovement();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    protected virtual void UpdateMovement ()
    {
        transform.position += moveVector;
    }
    
    protected virtual bool HasCollided()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.green, 1);
        if (Physics.Raycast (transform.position, transform.forward, out RaycastHit hit, moveVector.magnitude + radius, layerMask))
        {
            OnProjectileCollision(hit);
            return true;
        }
        return false;
    }

    protected virtual void OnProjectileCollision (RaycastHit hit)
    {
        StopProjectile();
    }
    
    private IEnumerator ILifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    public void InitializeProjectile (float moveSpeed, Vector3 position, Vector3 forward)
    {
        this.moveSpeed = moveSpeed;
        transform.position = position;
        transform.forward = forward;
    }
    
    public void InitializeProjectile (float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    /// <summary>
    /// Spawn projectile and tween it's graphics to toward its origin,
    /// </summary>
    /// <param name="_moveSpeed"></param>
    /// <param name="_graphicsPosition"></param>
    /// <param name="_distance"></param>
    public void InitializeProjectile (float _moveSpeed, Vector3 _graphicsPosition, float _distance = 0)
    {
        this.moveSpeed = _moveSpeed;
        if (projectileGraphics == null)
        {
            return;
        }
        float time = (_distance * _moveSpeed) - 0.1f;
        if (time < 0) time = 0;
        projectileGraphics.transform.position = _graphicsPosition;
        projectileGraphics.transform.DOLocalMove (Vector3.zero, time);
    }

    protected void StopProjectile()
    {
        disableMovement = true;
        if (projectileGraphics != null)
        {
            projectileGraphics.transform.DOKill();
            projectileGraphics.transform.localPosition = Vector3.zero;
        }
        StopCoroutine(lifetimeCoroutine);
    }

    #endregion
}
