using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalSession
{
    public VirtualFileSystem riftdeckFS;
    public VirtualFileSystem connectedFS;
    public FileSystemNode currentRiftdeckDir;
    public TerminalController controller;
    public string[] launchArgs;
    public bool connectedFSUnlocked;
}
