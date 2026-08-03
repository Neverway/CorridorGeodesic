using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FSNodeType
{
    Directory, 
    File
}

public enum FSFileKind
{
    Generic,
    Log,
    ProgramLauncher,
}

[System.Serializable]
public class FileSystemNode
{
    public string name;
    public FSNodeType nodeType;
    public string owner = "kellory";
    public string permissions = "-rw-r--r--";
    public int sizeBytes;

    public List<FileSystemNode> children = new List<FileSystemNode>();
    
    public FSFileKind fileKind = FSFileKind.Generic;
    public string programID;
    public string contentID;

    public bool accessDenied;
    public string accessDeniedReason;

    public FileSystemNode parent;
    
    public bool IsDirectory => nodeType == FSNodeType.Directory;

    public FileSystemNode AddChild(FileSystemNode child)
    {
        child.parent = this;
        children.Add(child);
        return child;
    }

    public static FileSystemNode NewDirectory(string name, string owner = "kellory", string permissions = "drwxr-xr-x")
    {
        return new FileSystemNode
        {
            name = name,
            nodeType = FSNodeType.Directory,
            owner = owner,
            permissions = permissions,
            sizeBytes = 4096
        };
    }
    
    public static FileSystemNode NewFile(string name, FSFileKind kind = FSFileKind.Generic, int size = 0, string owner = "kellory", string permissions = "-rw-r--r--")
    {
        return new FileSystemNode
        {
            name = name,
            nodeType = FSNodeType.File,
            fileKind = kind,
            owner = owner,
            permissions = permissions,
            sizeBytes = size
        };
    }
 
    public static FileSystemNode NewLockedDirectory(string name, string reason, string owner = "root")
    {
        var node = NewDirectory(name, owner, "d---------");
        node.accessDenied = true;
        node.accessDeniedReason = reason;
        return node;
    }

}
