//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Show the loading screen widget
// Notes:
//
//=============================================================================

using UnityEngine;

namespace Neverway.Framework.PawnManagement
{
    public class LB_Travel : MonoBehaviour
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
        private GameInstance gameInstance;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            gameInstance = FindObjectOfType<GameInstance>();
            gameInstance.UI_ShowLoading();
        }


        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}