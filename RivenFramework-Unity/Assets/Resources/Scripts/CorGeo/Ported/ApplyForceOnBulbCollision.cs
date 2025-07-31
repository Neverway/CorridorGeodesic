using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyForceOnBulbCollision : MonoBehaviour, BulbCollisionBehaviour
{
    public float force;
    private new Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
            Destroy(this);
    }
    public bool OnBulbCollision(Projectile_Marker bulb, RaycastHit hit)
    {
        // TODO Projectile_Marker replaced Projectile_Vacumm and causes these lines to break! ~Liz
        //rigidbody.AddForceAtPosition(bulb.moveVector * force, hit.point, ForceMode.Impulse);
        //bulb.KillProjectile();
        return false; // TODO THis returned true
    }
}
