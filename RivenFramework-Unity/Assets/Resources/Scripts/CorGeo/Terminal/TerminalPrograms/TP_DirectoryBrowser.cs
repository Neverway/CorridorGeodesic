using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TP_DirectoryBrowser : TerminalProgramBase
{
    public TMP_Text terminalName;
    public TMP_Text directory;
    public TMP_Text page;
    public TMP_Text controls;

    public Transform listContainer;
    public WB_Terminal_Entry listEntryPrefab;
    public Button home;
    public Button prevPage;
    public Button nextPage;

    public const int entriesPerPage = 12;
    
    private const string GatewayProgramId = "__connected_fs_gateway__";
    private const string BackToRiftdeckId = "__back_to_riftdeck__";
    
    private VirtualFileSystem activeFS;
    private FileSystemNode currentDir;
    private int currentPage;
    private readonly List<WB_Terminal_Entry> spawnedEntries = new List<WB_Terminal_Entry>();

    protected override void OnLaunch()
    {
        activeFS = session.riftdeckFS;
        currentDir = session.currentRiftdeckDir;
        currentPage = 0;
 
        home.onClick.AddListener(GoHome);
        prevPage.onClick.AddListener(() => ChangePage(-1));
        nextPage.onClick.AddListener(() => ChangePage(1));
 
        Render();
    }

    protected override void OnTerminate()
    {
        home.onClick.RemoveListener(GoHome);
    }

    private void GoHome()
    {
        activeFS = session.riftdeckFS;
        currentDir = session.riftdeckFS.ResolvePath(session.controller.defaultRiftdeckPath);
        currentPage = 0;
        session.currentRiftdeckDir = currentDir;
        Render();
    }
    
    private void SwitchToConnectedFS()
    {
        activeFS = session.connectedFS;
        currentDir = session.connectedFS.root;
        currentPage = 0;
        Render();
    }

    private void SwitchToRiftdeck()
    {
        activeFS = session.riftdeckFS;
        currentDir = session.currentRiftdeckDir ?? session.riftdeckFS.ResolvePath(session.controller.defaultRiftdeckPath);
        currentPage = 0;
        Render();
    }

    private void ChangePage(int delta)
    {
        currentPage = Mathf.Max(0, currentPage + delta);
        Render();
    }

    private void OnEntrySelected(FileSystemNode node)
    {
        if (node.programID == GatewayProgramId)
        {
            SwitchToConnectedFS(); return;
        }

        if (node.programID == BackToRiftdeckId)
        {
            SwitchToRiftdeck(); return;
        }

        if (node.name == "..")
        {
            currentDir = node.parent ?? currentDir;
            currentPage = 0;
            if (activeFS == session.riftdeckFS) session.currentRiftdeckDir = currentDir;
            Render();
            return;
        }
 
        if (node.IsDirectory)
        {
            if (node.accessDenied) return;
            currentDir = node;
            currentPage = 0;
            if (activeFS == session.riftdeckFS) session.currentRiftdeckDir = currentDir;
            Render();
            return;
        }

        switch (node.fileKind)
        {
            case FSFileKind.ProgramLauncher:
                if (activeFS == session.riftdeckFS) session.currentRiftdeckDir = currentDir;
                RequestLaunchProgram(node.programID);
                break;
            case FSFileKind.Log:
                if (activeFS == session.riftdeckFS) session.currentRiftdeckDir = currentDir;
                RequestLaunchProgram("log_reader", node.contentID);
                break;
            default:
                Debug.Log($"No handler for file kind {node.fileKind} on '{node.name}'");
                break;
        }
    }

    private void Render()
    {
        bool onConnectedFS = activeFS == session.connectedFS;
        terminalName.text = onConnectedFS ? "root@KBM-5100" : "kellory@KBM-5100";
        directory.text = $"DIR:  {activeFS.GetPath(currentDir)}";
 
        foreach (var e in spawnedEntries) Destroy(e.gameObject);
        spawnedEntries.Clear();
 
        var rows = new List<FileSystemNode>();

        if (currentDir.parent != null)
        {
            rows.Add(new FileSystemNode
            {
                name = "..",
                nodeType = FSNodeType.Directory,
                parent = currentDir.parent,
                permissions = "-",
                sizeBytes = 0
            });
        }
        else if (onConnectedFS)
        {
            rows.Add(new FileSystemNode
            {
                name = "..",
                nodeType = FSNodeType.File,
                fileKind = FSFileKind.ProgramLauncher,
                programID = BackToRiftdeckId,
                permissions = "-",
                owner = "kellory",
                sizeBytes = 0
            });
        }

        if (!onConnectedFS && currentDir == session.riftdeckFS.root && session.connectedFSUnlocked)
        {
            rows.Add(new FileSystemNode
            {
                name = "[LOCAL SYSTEM]",
                nodeType = FSNodeType.File,
                fileKind = FSFileKind.ProgramLauncher,
                programID = GatewayProgramId,
                permissions = "drwxr-xr-x",
                owner = "system",
                sizeBytes = 4096
            });
        }

        rows.AddRange(currentDir.children);
 
        int totalPages = Mathf.Max(1, Mathf.CeilToInt(rows.Count / (float)entriesPerPage));
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);
 
        var pageRows = rows.Skip(currentPage * entriesPerPage).Take(entriesPerPage);
        foreach (var row in pageRows)
        {
            var widget = Instantiate(listEntryPrefab, listContainer);
            widget.Bind(row, OnEntrySelected);
            spawnedEntries.Add(widget);
        }
 
        page.text = $"(Page {currentPage + 1}/{totalPages})";
        controls.text = "[ESC] Eject  |  [RETURN] Select";

    }
}
