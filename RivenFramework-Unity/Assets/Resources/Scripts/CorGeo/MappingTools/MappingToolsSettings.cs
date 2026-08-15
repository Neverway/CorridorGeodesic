using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MappingToolsSettings : ScriptableObject
{
    public List<GameObject> m_RequiredLevelObjects = new List<GameObject>();
    public List<AssetLabelEntry> m_AssetLabels = new List<AssetLabelEntry>();
}

[System.Serializable]
public class AssetLabelEntry
{
    public string m_Label;
    public string m_DisplayName;
}