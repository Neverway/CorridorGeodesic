using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Neverway.Framework.LogicSystem;
using RivenFramework;

public class BulbPowerSocket : MonoBehaviour, BulbCollisionBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public LogicOutput<bool> isPowered = new(false);

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

    [Todo("The new actor class does not have a variable called HomeParent yet ~Liz")]
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
        isPowered.Set(fittedBulb != null);
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
            bulb.MarkerBreak();
            return false;
        }

        fittedBulb = bulb;
        bulb.MarkerPinAt(attachPoint.position, attachPoint.forward);
        return true;
    }
}
