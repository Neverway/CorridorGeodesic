using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class WB_LevelSelect : WidgetBlueprint
{
    public Transform levelEntryListRoot;
    public GameObject levelEntryPrefab;
    private GI_LevelSelect levelSelectManager;

    private int lastGroup;
    private int currentLevelNumber;

    public override bool PausesPawns() => true;
    void Start()
    {
        levelSelectManager = GameInstance.Get<GI_LevelSelect>();
        lastGroup = 0;

        for (int i = 0; i < levelSelectManager.levels.Count; i++)
        {
            var level = levelSelectManager.levels[i];

            // Create a button for each level entry
            var newEntry = Instantiate(levelEntryPrefab, levelEntryListRoot).GetComponent<WB_LevelSelect_Entry>();
            // Update all the buttons text
            newEntry.displayText.text = GetDecoratedDisplayName(level, i);
            // Tie in all the buttons to communicate to this level selector
            if (level.targetScene)
            {
                newEntry.button.onClick.AddListener(delegate { SelectLevel(level); });
            }
            
            // Update the entry to reflect it interactability
            if (level.hideFromSelection) newEntry.gameObject.SetActive(false);
            if (level.locked) newEntry.button.interactable = false;
        }


    }

    void Update()
    {
        // Detect inputs for closing the widget
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWidget();
        }
    }

    private string GetDecoratedDisplayName(SelectableLevel targetLevel, int index)
    {
        var entryLength = 24;
        var startingNameLength = targetLevel.displayName.Length;
        var finalDisplayName = "";

        finalDisplayName += targetLevel.displayName;
        for (int i = 0; i < entryLength-startingNameLength; i++)
        {
            finalDisplayName += "-";
        }

        // C #-
        finalDisplayName += $"C {targetLevel.group}-";

        // ##
        if (lastGroup != targetLevel.group)
        {
            currentLevelNumber = index;
        }
        finalDisplayName += $"{index-currentLevelNumber}";
        lastGroup = targetLevel.group;
        
        // LEVEL NAME----------------C #-##
        return finalDisplayName;
    }
    
    public void OnDestroy()
    {
        foreach (var levelSelectBooth in FindObjectsOfType<LevelSelectBooth>())
        {
            levelSelectBooth.Reenable();
        }
    }

    public void CloseWidget()
    {
        Destroy(gameObject);
    }

    public void SelectLevel(SelectableLevel targetLevel)
    {
        // TODO Play the dialing sequence (Audio, animation, etc)
        
        // Load the targeted scene
        GameInstance.Get<GI_WorldLoader>().LoadWorld(targetLevel.targetScene.name);
    }
}
