using System.Collections.Generic;
using UnityEngine;

public abstract class CorGeo_SliceableTriggerController : MonoBehaviour
{
    [HideInInspector] public HashSet<CorGeo_SlicedTriggerPart> slicedTriggerParts = new();

    public abstract void OnTriggerPartEnter(Collider _other);
    public abstract void OnTriggerPartExit(Collider _other);

    public abstract void OnPartAdded(CorGeo_SlicedTriggerPart part);
    public abstract void OnPartRemoved(CorGeo_SlicedTriggerPart part);

    public void AddPart(CorGeo_SlicedTriggerPart part)
    {
        slicedTriggerParts.Add(part);
        OnPartAdded(part);
    }
    public void RemovePart(CorGeo_SlicedTriggerPart part)
    {
        slicedTriggerParts.Remove(part);
        OnPartRemoved(part);
    }
}
