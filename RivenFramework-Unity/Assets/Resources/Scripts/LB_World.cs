//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes: 
//
//=============================================================================

using System.Collections;
using UnityEngine;
using Neverway.Framework.PawnManagement;

namespace Neverway
{
    public class LB_World : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private GI_WidgetManager widgetManager;
        [SerializeField] private GameObject HUDWidget;
        
        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            widgetManager = FindObjectOfType<GI_WidgetManager>();
            widgetManager.AddWidget(HUDWidget);
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=
        

        //=-----------------=
        // External Functions
        //=-----------------=
    }
}