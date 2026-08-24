using ErryLib;
using RivenFramework;
using UnityEngine;

public class Checkpoint : GUIDComponent
{
    public Transform spawnLocation;
    public bool allowBacktrackingTo = false;

    public LogicInput<Checkpoint> previousCheckpoint = new(null);
    public LogicOutput<Checkpoint> thisCheckpoint = new(null);
    public LogicInput<bool> activateCheckpoint = new(false);
    public LogicOutput<bool> isCurrentCheckpoint = new(false);
    public LogicInput<bool> isCurrentOrPreviousCheckpoint = new(false);

    public Checkpoint PreviousCheckpoint => previousCheckpoint.Get();

    public void Awake()
    {
        thisCheckpoint.Set(this);
        activateCheckpoint.CallOnSourceChanged(() => { TryActivateCheckpoint(); });
    }

    public bool TryActivateCheckpoint() => GameInstance.Get<GI_CheckpointManager>().TryActivateCheckpoint(this);

    public bool IsBeforeCheckpoint(Checkpoint other)
    {
        Checkpoint original = other;
        while (other != null)
        {
            other = other.previousCheckpoint;
            if (other == this) return true;
            if (other == original) return false;
        }
        return false;
    }
}
