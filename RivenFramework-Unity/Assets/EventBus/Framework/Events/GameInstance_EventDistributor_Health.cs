using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// Distributes health events to the appropriate health receiver
    /// </summary>
    public class GameInstance_EventDistributor_Health : MonoBehaviour
    {
        public List<HealthReceiver> healthReceivers = new List<HealthReceiver>();
        private void OnEnable()
        {
            EventBus<Event_ModifyHealth>.Subscribe(DistributeModifyHealthEventToReceivers);
        }
        
        private void OnDisable()
        {
            EventBus<Event_ModifyHealth>.Unsubscribe(DistributeModifyHealthEventToReceivers);
        }

        private void DistributeModifyHealthEventToReceivers(Event_ModifyHealth eventModifyHealth)
        {
            Debug.Log("Event Called");
            Debug.Log($"count {healthReceivers.Count}");
            foreach (var healthReceiver in healthReceivers)
            {
                Debug.Log($"{healthReceiver.gameObject.name} {eventModifyHealth.Target} {healthReceiver.target}");
                if (eventModifyHealth.Target == healthReceiver.target)
                {
                    Debug.Log($"{healthReceiver.gameObject.name} is valid");
                    healthReceiver.ModifyHealth(eventModifyHealth);
                }
            }
        }
    }

    public class Event_ModifyHealth : BaseEvent
    {
        public override bool LogEventCalls() => true;

        public int DamageAmount;
        [Polymorphic, SerializeReference] public List<ActorFilter> AffectedActorsFilters;
        public Actor Target;
        public Actor Source;
        
        public override string GetEventDescription()
        {
            return $"DamageAmount:{DamageAmount}, AffectedActorsFilters:{AffectedActorsFilters}, Target:{Target}, Source:{Source}";
        }

    }

    public class Event_Death : BaseEvent
    {
        
        public Actor Target;
        public Event_ModifyHealth CauseOfDeath;
    }
