using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neverway.Framework.LogicSystem;

public class BulbPowerSocket : LogicComponent, BulbCollisionBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=

    //=-----------------=
    // Private Variables
    //=-----------------=
    private Projectile_Marker fittedBulb;

    //=-----------------=
    // Reference Variables
    //=-----------------=
    [field: SerializeField] public Transform attachPoint { get; private set; }

    //=-----------------=
    // Mono Functions
    //=-----------------=

    private IEnumerator Start()
    {
        yield return null;
        attachPoint.SetParent(null);
        if (attachPoint.TryGetComponent<CorGeo_Actor> (out var actor))
        {
            // TODO the new actor class does not have a variable called HomeParent yet ~Liz
            //actor.homeParent = null;
        }
    }
    private void Update()
    {
        isPowered = fittedBulb != null;
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    public bool OnBulbCollision(Projectile_Marker bulb, RaycastHit hit)
    {
        if (fittedBulb != null)
        {
            //bulb.KillProjectile();
            return true;
        }

        fittedBulb = bulb;
        //bulb.Attach(attachPoint.position, attachPoint.forward);
        return true;
    }
}
