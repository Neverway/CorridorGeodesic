using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class Recieve_HealthEvent : MonoBehaviour
{
    public int currentHealth;
    
    private void OnEnable()
    {
        EventBus<Define_HealthEvent.E_Damage>.Subscribe(TakeDamage);
    }
    
    private void OnDisable()
    {
        EventBus<Define_HealthEvent.E_Damage>.Unsubscribe(TakeDamage);
    }

    private void TakeDamage(Define_HealthEvent.E_Damage damageEvent)
    {
        if (damageEvent.Target != gameObject) return;
        
        currentHealth -= damageEvent.Amount;

        if (currentHealth <= 0)
        {
            EventBus<Define_HealthEvent.E_Death>.Publish(new Define_HealthEvent.E_Death
            {
                Target = this.gameObject,
                CauseOfDeath = damageEvent
            });
        }
    }
}
*/