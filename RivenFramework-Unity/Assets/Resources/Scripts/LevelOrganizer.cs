using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelOrganizer", menuName = "Maps/Level Organizer")]
[Serializable]
public class LevelOrganizer : ScriptableObject
{
    public List<LevelSetGroup> LevelSetGroups;
}

/// <summary>
/// A level
/// </summary>
//[CreateAssetMenu(fileName = "Level", menuName = "Maps/Level")]
[Serializable]
public class Level
{
    public string displayName;
    public SceneReference scene;
}

/// <summary>
/// A set of levels
/// </summary>
[Serializable]
public class LevelSet
{
    public string name;
    [TextArea] public string description;
    public List<SceneReference> levels;
}

/// <summary>
/// A set of sets of levels
/// </summary>
[Serializable]
public class LevelSetGroup
{
    public string name;
    [TextArea] public string description;
    public List<LevelSet> levelSets;
}