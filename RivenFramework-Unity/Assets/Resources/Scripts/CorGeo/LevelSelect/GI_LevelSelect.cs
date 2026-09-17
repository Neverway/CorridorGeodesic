using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GI_LevelSelect : MonoBehaviour
{
    public List<SelectableLevel> levels;
}

[Serializable]
public struct SelectableLevel
{
    [Tooltip("The name to show on the level select screen")]
    public string displayName;
    [Tooltip("This is just used however you like to separate displayed entries further. In corgeo this is used to separate the levels by chapter in the level select book")]
    public int group;
    [Tooltip("The scene that can be loaded via level select")]
    public Scene targetScene;
    [Tooltip("If you want to track all levels in the game, check this to hide levels that should not show up in the player's level select menu")]
    public bool hideFromSelection;
    [Tooltip("When a level has not been visited yet, it is blacked out on the level select menu")]
    public bool locked;
    //[Tooltip("When starting this level from the level select menu, this is the point the player will spawn at")]
    //public Vector3 spawnPoint;
}