using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TerminalFileNodeDef
{
    [Tooltip("The full path to this file from root. Any subfolders that don't already exists will be auto-created")]
    public string path;

    public FSNodeType NodeType = FSNodeType.Directory;
    [Tooltip("default (system)")]
    public string owner = "system";
    [Tooltip("default (drwxr-xr-x)")]
    public string permissions = "drwxr-xr-x";
    [Tooltip("default (4096)")]
    public int sizeBytes = 4096;
    
    [Header("File Fields")]
    public FSFileKind FileKind = FSFileKind.Generic;
    public string programID;
    public string contentID;
    
    [Header("Directory Fields")]
    public bool accessDenied;
    public string accessDeniedReason;
}

[CreateAssetMenu(fileName = "TerminalFileSystemDefinition", menuName = "Terminal/Terminal File System Definition")]
public class TerminalFileSystemDefinition : ScriptableObject
{
    
    public string rootName = "terminal";
    public List<TerminalFileNodeDef> entries = new List<TerminalFileNodeDef>();
 
    public VirtualFileSystem Build()
    {
        var root = FileSystemNode.NewDirectory(rootName, "system", "drwxr-xr-x");
        var lookup = new Dictionary<string, FileSystemNode> { { "", root } };
 
        var sorted = entries.OrderBy(e => SegmentCount(e.path)).ToList();
 
        foreach (var e in sorted)
        {
            string trimmed = (e.path ?? "").Trim('/');
            if (trimmed.Length == 0) continue;
 
            int lastSlash = trimmed.LastIndexOf('/');
            string parentPath = lastSlash >= 0 ? trimmed.Substring(0, lastSlash) : "";
            string name = lastSlash >= 0 ? trimmed.Substring(lastSlash + 1) : trimmed;
 
            var parent = EnsureDirectoryPath(lookup, parentPath);
 
            FileSystemNode node;
            if (e.NodeType == FSNodeType.Directory)
            {
                node = FileSystemNode.NewDirectory(name, e.owner, e.permissions);
                if (e.accessDenied)
                {
                    node.accessDenied = true;
                    node.accessDeniedReason = e.accessDeniedReason;
                }
            }
            else
            {
                node = FileSystemNode.NewFile(name, e.FileKind, e.sizeBytes, e.owner, e.permissions);
                node.programID = e.programID;
                node.contentID = e.contentID;
            }
 
            parent.AddChild(node);
            lookup[trimmed] = node;
        }
 
        return new VirtualFileSystem(root);
    }
 
    private FileSystemNode EnsureDirectoryPath(Dictionary<string, FileSystemNode> lookup, string path)
    {
        if (lookup.TryGetValue(path, out var existing)) return existing;
 
        int lastSlash = path.LastIndexOf('/');
        string parentPath = lastSlash >= 0 ? path.Substring(0, lastSlash) : "";
        string name = lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
 
        var parent = EnsureDirectoryPath(lookup, parentPath);
        var dir = FileSystemNode.NewDirectory(name);
        parent.AddChild(dir);
        lookup[path] = dir;
        return dir;
    }
 
    private static int SegmentCount(string path) => (path ?? "").Trim('/').Split('/').Length;
}
