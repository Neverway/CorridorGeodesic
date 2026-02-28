using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorGeo_SlicedTriggerPart : MonoBehaviour
{
    public CorGeo_SliceableTriggerController parentTriggerController;

    private void Awake()
    {
        parentTriggerController.slicedTriggerParts.Add(this);
    }
    private void OnDestroy()
    {
        parentTriggerController.slicedTriggerParts.Remove(this);
    }

    private void OnTriggerEnter(Collider _other)
    {
        parentTriggerController.OnTriggerPartEnter(_other);
    }
    private void OnTriggerExit(Collider _other)
    {
        parentTriggerController.OnTriggerPartEnter(_other);
    }
}
