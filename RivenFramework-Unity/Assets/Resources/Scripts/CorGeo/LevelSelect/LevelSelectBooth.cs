using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class LevelSelectBooth : MonoBehaviour
{
    public GameObject triggerObject;
    public GameObject levelSelectWidget;
    private GI_WidgetManager widgetManager;
    
    public void Activate()
    {
        if (widgetManager == null) widgetManager = GameInstance.Get<GI_WidgetManager>();

        //widgetManager.AddWidget(levelSelectWidget);
        
        triggerObject.SetActive(false);
    }

    public void Reenable()
    {
        if (!triggerObject)
        {
            return;
        }
        triggerObject.gameObject.SetActive(true);
    }
}
