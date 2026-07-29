using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalController : MonoBehaviour
{
    public string defaultRiftdeckPath = "/riftdeck/kellory";
    public Transform programHost;
    public TerminalProgramRegistry programRegistry;
    public string defaultProgramId = "directory_browser";
    public TerminalBootLoader BootLoader;
    public VirtualFileSystem riftdeckFS { get; private set; }
    public VirtualFileSystem connectedFS { get; private set; }
    private TerminalProgramBase activeProgram;
    private TerminalSession session;

    public void ConnectRiftdeck(VirtualFileSystem _riftdeckFS, VirtualFileSystem _connectedFS)
    {
        riftdeckFS = _riftdeckFS;
        connectedFS = _connectedFS;

        session = new TerminalSession
        {
            riftdeckFS = riftdeckFS,
            connectedFS = connectedFS,
            currentRiftdeckDir = riftdeckFS.ResolvePath(defaultRiftdeckPath),
            controller = this
        };

        if (BootLoader != null)
        {
            BootLoader.OnBootComplete = () => LaunchProgram(defaultProgramId);
            BootLoader.Play();
        }
        else
        {
            LaunchProgram(defaultProgramId);
        }
    }

    public void LaunchProgram(string programId, params string[] args)
    {
        var prefab = programRegistry.GetPrefab(programId);
        if (prefab == null)
        {
            Debug.LogWarning($"TerminalController does not have a program registered for id {programId}");
            return;
        }

        if (activeProgram != null)
        {
            activeProgram.Terminate();
            Destroy(activeProgram.gameObject);
            activeProgram = null;
        }

        session.launchArgs = args;
        activeProgram = Instantiate(prefab, programHost);
        activeProgram.Launch(session);
    }

    public void Eject()
    {
        if (activeProgram != null)
        {
            activeProgram.Terminate();
            Destroy(activeProgram.gameObject);
            activeProgram = null;
        }
        BootLoader.bootPanel.SetActive(true);

        riftdeckFS = null;
        connectedFS = null;
        session = null;
    }
}
