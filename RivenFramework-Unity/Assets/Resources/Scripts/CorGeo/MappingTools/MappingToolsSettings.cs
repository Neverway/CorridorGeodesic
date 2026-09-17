using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MappingToolsSettings : ScriptableObject
{
    public List<RequiredLevelObjectEntry> m_RequiredLevelObjects = new List<RequiredLevelObjectEntry>();
    public List<AssetLabelEntry> m_AssetLabels = new List<AssetLabelEntry>();
    
    [Tooltip("The Level Organizer asset the Maps tab reads and edits")]
    public LevelOrganizer m_LevelOrganizer;
 
    [Tooltip("Duplicated by 'New Map From Template' in the Maps tab to create new scenes")]
    public SceneAsset m_TemplateMapScene;

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