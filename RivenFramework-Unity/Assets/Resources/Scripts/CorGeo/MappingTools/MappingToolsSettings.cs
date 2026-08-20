using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MappingToolsSettings : ScriptableObject
{
    public List<RequiredLevelObjectEntry> m_RequiredLevelObjects = new List<RequiredLevelObjectEntry>();
    public List<AssetLabelEntry> m_AssetLabels = new List<AssetLabelEntry>();
}

[System.Serializable]
public class AssetLabelEntry
{
    public string m_Label;
    public string m_DisplayName;
}

[System.Serializable]
public class RequiredLevelObjectEntry
{
    public GameObject m_Object;
    public bool m_KeepAsPrefab = false;
}