using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFSF_Riftdeck
{
    public static VirtualFileSystem BuildDefault()
    {
        var root = FileSystemNode.NewDirectory("riftdeck", "root", "drwxr-xr-x");

        var kellory = FileSystemNode.NewDirectory("kellory");
        root.AddChild(kellory);

        var savedLogs = FileSystemNode.NewDirectory("Saved Logs");
        var programs = FileSystemNode.NewDirectory("Programs");

        var hackDesktop = FileSystemNode.NewFile("HackTerminal.desktop", FSFileKind.ProgramLauncher, 7584,
            "kellory", "-rwxr-xr-x");
        hackDesktop.programID = "hack_terminal";

        kellory.AddChild(savedLogs);
        kellory.AddChild(programs);
        kellory.AddChild(hackDesktop);

        root.AddChild(FileSystemNode.NewLockedDirectory("jamiey", "owner uid mismatch"));
        root.AddChild(FileSystemNode.NewLockedDirectory("osiris", "owner uid mismatch"));

        return new VirtualFileSystem(root);
    }

    public static void AddSavedLog(VirtualFileSystem riftdeckFS, string logName, string contentId)
    {
        var logsDir = riftdeckFS.ResolvePath("/riftdeck/kellory/Saved Logs");
        if (logsDir == null) return;

        var log = FileSystemNode.NewFile(logName, FSFileKind.Log, 1024);
        log.contentID = contentId;
        logsDir.AddChild(log);
    }

    public static void AddProgram(VirtualFileSystem riftdeckFS, string displayName, string programId, int size = 4096)
    {
        var programsDir = riftdeckFS.ResolvePath("/riftdeck/kellory/Programs");
        if (programsDir == null) return;
        
        var file = FileSystemNode.NewFile(displayName + ".desktop", FSFileKind.ProgramLauncher, size, "kellory", "-rwxr-xr-x");
        file.programID = programId;
        programsDir.AddChild(file);

    }
}
