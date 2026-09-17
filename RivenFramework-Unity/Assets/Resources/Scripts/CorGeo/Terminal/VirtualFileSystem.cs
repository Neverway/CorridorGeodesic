using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirtualFileSystem
{
    public readonly FileSystemNode root;

    public VirtualFileSystem(FileSystemNode _root)
    {
        root = _root;
        _root.parent = null;
    }

    public FileSystemNode ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return root;

        var parts = path.Split("/").Where(p => p.Length > 0);
        var current = root;

        foreach (var part in parts)
        {
            if (current == null) return null;
            if (part == current.name) continue;
            if (!current.IsDirectory) return null;
            current = current.children.FirstOrDefault(c => c.name == part);
        }

        return current;
    }

    public string GetPath(FileSystemNode node)
    {
        if (node == null) return "";

        var segments = new List<string>();
        var cur = node;
        while (cur != null)
        {
            segments.Insert(0, cur.name);
            cur = cur.parent;
        }

        return "/" + string.Join("/", segments);
    }
}
