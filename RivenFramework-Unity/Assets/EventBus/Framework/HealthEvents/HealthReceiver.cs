
using System;
using System.Collections;
using RivenFramework;
using RivenFramework.EventBus;
using UnityEngine;

public class HealthReceiver : MonoBehaviour
{
    public BActor target;
    
    public int maxHealth = 100;
    public int currentHealth = 100;
    public bool isDead = false;
    public bool isInvulnerable = false;
    public float invulnerabilityTime = 0.25f;
    public bool despawnOnDeath = false;
    public float despawnOnDeathDelay = 1f;
    public event Action OnHurt;
    public event Action OnHeal;
    public event Action OnDeath;

    private void Start()
    {
        GameInstance.Get<GameInstance_EventDistributor_Health>().healthReceivers.Add(this);
    }

    private void OnDestroy()
    {
        GameInstance.Get<GameInstance_EventDistributor_Health>().healthReceivers.Remove(this);
    }

    private IEnumerator InvulnerabilityCooldown()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    public void ModifyHealth(Event_ModifyHealth eventModifyHealth)
    {
        if (isInvulnerable) return;
        StartCoroutine(InvulnerabilityCooldown());
        switch (eventModifyHealth.DamageAmount)
        {
            case > 0:
                OnHeal?.Invoke();
                isDead = false;
                break;
            case < 0:
                if (isDead) return;
                OnHurt?.Invoke();
                break;
        }

        if (currentHealth + eventModifyHealth.DamageAmount <= 0)
        {
            if (isDead) return;
            OnDeath?.Invoke();
            EventBus<Event_Death>.Publish(new Event_Death
            {
                Target = target,
                CauseOfDeath = eventModifyHealth
            });
            isDead = true;
            if (despawnOnDeath)
            {
                Destroy(gameObject, despawnOnDeathDelay);
            }
        }

        if (currentHealth + eventModifyHealth.DamageAmount > maxHealth) currentHealth = maxHealth;
        else if (currentHealth + eventModifyHealth.DamageAmount < 0) currentHealth = 0;
        else currentHealth += eventModifyHealth.DamageAmount;
    }
}