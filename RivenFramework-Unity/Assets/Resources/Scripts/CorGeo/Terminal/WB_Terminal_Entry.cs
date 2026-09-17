using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WB_Terminal_Entry : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text sizeText;
    public TMP_Text permissionsText;
    public Button button;

    private FileSystemNode node;
    private Action<FileSystemNode> OnSelect;

    public void Bind(FileSystemNode node, Action<FileSystemNode> OnSelect)
    {
        this.node = node;
        this.OnSelect = OnSelect;

        bool locked = node.accessDenied;
        string displayName = node.IsDirectory ? "./" + node.name : node.name;

        nameText.text = displayName;
        sizeText.text = node.parent == null && node.name == ".." ? "-" : node.sizeBytes.ToString();
        permissionsText.text = $"{node.permissions}  {node.owner}";

        button.interactable = !locked;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.OnSelect?.Invoke(this.node));
    }
}
