using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Call_HealthEvent : MonoBehaviour
{
    public int damageAmount;
    public List<string> groupFilter;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Identifier>() is null) return;
        
        if (other.GetComponentInParent<Identifier>().IsInAnyOfGroups(groupFilter) is false) return;
        
        EventBus<Define_HealthEvent.E_Damage>.Publish(new Define_HealthEvent.E_Damage
        {
            Amount = damageAmount,
            GroupFilter = groupFilter,
            Target = other.GetComponentInParent<Identifier>().gameObject,
            Source = this.gameObject
        });
    }
}
*/