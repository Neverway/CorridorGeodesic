
using UnityEngine;

/// <summary>
/// Listens to every event that is sent through the event bus and logs the information to a file
/// </summary>
public class GameInstance_EventDistributor_Timeline : MonoBehaviour
{
    private void OnEnable()
    {
        EventBusAnyEvent.Subscribe(LogEvent);
    }
    
    private void OnDisable()
    {
        EventBusAnyEvent.Unsubscribe(LogEvent);
    }

    private void LogEvent(BaseEvent _event)
    {
        DebugConsole.Log(_event, _event.GetEventDescription());
    }
}
