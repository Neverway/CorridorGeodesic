using System;
using UnityEngine;

namespace RivenFramework.EventBus
{
    public class EventBus<T> where T : BaseEvent
    {
        private static event Action<T> OnEvent;

        public static void Subscribe(Action<T> listener) => OnEvent += listener;
        public static void Unsubscribe(Action<T> listener) => OnEvent -= listener;
        public static void Publish(T eventData)
        { 
            OnEvent?.Invoke(eventData);
            EventBusAnyEvent.Publish(eventData);
        }
    }
    
    public class EventBusAnyEvent
    {
        private static event Action<BaseEvent> OnAnyEvent;
        
        public static void Subscribe(Action<BaseEvent> listener) => OnAnyEvent += listener;
        public static void Unsubscribe(Action<BaseEvent> listener) => OnAnyEvent -= listener;
        internal static void Publish(BaseEvent eventData)
        { 
            OnAnyEvent?.Invoke(eventData);
        }
    }

    public abstract class BaseEvent : ILoggable
    {
        public bool EnableRuntimeLogging { get => LogEventCalls(); set { } }
        public virtual bool LogEventCalls() => false;

        public virtual string GetEventDescription()
        {
            return $"{GetType()} has been called. [No context available]";
        }
    }
}
