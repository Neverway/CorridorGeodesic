using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Events;

public class Func_TerminalInteraction : MonoBehaviour
{
    [SerializeField] private TerminalController terminalController;
    [SerializeField] private TerminalFileSystemDefinition connectedFileSystemDefinition;
    public bool useDefaultConnectedFS = true;
    private bool terminalIsInUse;
    public UnityEvent OnPlugIn, OnPlugOut;
    private InputActions.FirstPersonActions inputActions;

    public void Start()
    {
        inputActions = new InputActions().FirstPerson;
        inputActions.Enable();
    }

    public void Update()
    {
        if (!terminalIsInUse) return;

        if (inputActions.Pause.WasPressedThisFrame())
        {
            PlugOut();
            var pauseWidget = GameInstance.Get<GI_WidgetManager>().GetExistingWidget("WB_Pause");
            if (pauseWidget) Destroy(pauseWidget);
        }
    }

    public void PlugIn()
    {
        if (terminalIsInUse) return;
        terminalIsInUse = true;
        OnPlugIn.Invoke();
        var riftdeckFS = GI_Terminal.Instance.FileSystem;
 
        VirtualFileSystem connectedFS = connectedFileSystemDefinition != null ? connectedFileSystemDefinition.Build() : VFSF_Riftdeck.BuildDefault();
 
        terminalController.ConnectRiftdeck(riftdeckFS, connectedFS);
    }
    
    public void PlugOut()
    {
        if (!terminalIsInUse) return;
        OnPlugOut.Invoke();
        terminalIsInUse = false;
        terminalController.Eject();
    }
}
