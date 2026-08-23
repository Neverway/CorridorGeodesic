using System;

[Serializable]
public class CheckpointHandle 
{
    public CheckpointHandle(Checkpoint target) => this.target = target;
    public Checkpoint target;
}