using System;
using RivenFramework;
using UnityEngine;

[RequireComponent(typeof(LightProbeProxyVolume))]
public class LPPVController : MonoBehaviour
{
    private LightProbeProxyVolume _lppv;
    private RiftManager _riftManager;
    [Tooltip("How many frames to wait during rift motion to update light sampling. Decreasing this improves the responsiveness of lighting updates, but impacts performance quite a bit!")]
    [SerializeField] private float updateEveryFrames = 26;
    private int frameCounter;
    private N_RiftState lastRiftState;

    void Awake()
    {
        _lppv = GetComponent<LightProbeProxyVolume>();
        _lppv.refreshMode = LightProbeProxyVolume.RefreshMode.ViaScripting;
    }

    private void FixedUpdate()
    {
        if (!_riftManager)
        {
            _riftManager = GameInstance.Get<RiftManager>();
            return;
        }
        
        //                                                             ___   (All aboard the OR train!)
        //       ____   ____   ____   ____   ____   ____   ____   _____||0
        //      [_or_]>[_or_]>[_or_]>[_or_]>[_ro_]>[_or_]>[_or_]>[_^w^_]|==|\
        //       *  *   *  *   *  *   *  *   *  *   *  *   *  *   *  * *|  |_\
        if (_riftManager.stateHandler.IsState<RiftState_Preview>() || _riftManager.stateHandler.IsState<RiftState_Collapsing>() || _riftManager.stateHandler.IsState<RiftState_Expanding>() || _riftManager.stateHandler.IsState<RiftState_ExpandingFromCrush>() || _riftManager.stateHandler.IsState<RiftState_DestroyRestoring>() || _riftManager.stateHandler.IsState<RiftState_Destroy>())
        {
            // Refresh the light sampling if the rift distorts anything
            
            // Reset the frame counter if the rift state changed
            if (lastRiftState != _riftManager.stateHandler.currentState)
            {
                frameCounter = 0;
            }
            
            // Refresh the light sampling every 'N' frames while a rift is in motion
            lastRiftState = _riftManager.stateHandler.currentState;
            frameCounter++;
            if (frameCounter >= updateEveryFrames)
            {
                frameCounter = 0;
                Refresh();
            }
        }

        // Refresh the light sampling once when a rift is first cleared
        if (lastRiftState != _riftManager.stateHandler.currentState && _riftManager.stateHandler.IsState<RiftState_None>())
        {
            lastRiftState = _riftManager.stateHandler.currentState;
            Refresh();
        }
    }

    public void Refresh()
    {
        _lppv.Update();
    }
}