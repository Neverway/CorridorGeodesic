//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Neverway.Framework.PawnManagement;

namespace Neverway.Framework.LogicSystem
{
    public class Volume_LevelChange : Volume
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        public string worldID;
        public bool useIndexInsteadOfID;
        public bool indexBackwards;



        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private WorldLoader worldLoader;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private new void OnTriggerEnter2D(Collider2D _other)
        {
            if (GetPlayerInTrigger())
            {
                if (!worldLoader) worldLoader = FindObjectOfType<WorldLoader>();
                worldLoader.LoadWorld(worldID);
            }
        }

        private new void OnTriggerEnter(Collider _other)
        {
            if (_other.GetComponent<Pawn>())
            {
                DevConsole.Log("Player has entered a load trigger", "WorldLoader");
                if (!_other.GetComponent<Pawn>().isPossessed) return;
                if (!worldLoader) worldLoader = FindObjectOfType<WorldLoader>();
                if (useIndexInsteadOfID)
                {
                    worldLoader.LoadByIndex(SceneManager.GetActiveScene().buildIndex + 1);
                }
                else if (!useIndexInsteadOfID)
                {
                    DevConsole.Log($"Firing StreamLoadWorld({worldID})", "WorldLoader");
                    worldLoader.StreamLoadWorld(worldID);
                }
            }
        }

        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}