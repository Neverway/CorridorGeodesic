//==========================================( Neverway 2026 )=========================================================//
// Author
// Liz M.
//
// Contributors
//  Errynei, Connorses, Soulex
//
//====================================================================================================================//

using ErryLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NewRivenFramework.Core
{
    /// <summary>
    /// Acts as the base identifier for an asset that can be placed in a map
    /// Assigning it an id, what groups it's a part of, and a human-readable display name
    /// All placeable assets must contain this component on their root
    /// </summary>
    public class Actor : GUIDComponent
    {
        #region========================================( Variables )====================================================== //

        /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
        [Header("Actor Data")]
        [Tooltip("This ID refers to the type of actor this is, (NOT unique per instance) which is used for identifying, saving, and loading from map files")]
        public string id;
        [Tooltip("This is how this actor is listed in things like an asset browser, or in game like in an inventory")]
        public string displayName;
        [Tooltip("This is what tags this actor is associated with, it's used to filter between different kinds of objects when handling things like logic volumes")]
        public List<ActorTag> actorTags;


        /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


        /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
        [ContextMenu("Generate ID")]
        private void GenerateID()
        {
            id = gameObject.name;
        }
    
        [ContextMenu("Generate Display Name")]
        private void GenerateDisplayName()
        {
            displayName = Regex.Replace(gameObject.name, "([a-z])([A-Z])", "$1 $2");
            displayName = Regex.Replace(displayName, "^[^_]*_", "");
        }

        /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



        #endregion


        #region=======================================( Functions )======================================================= //

        /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/

        
        /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


        /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
        public bool HasActorTag(List<ActorTag> _actorTags)
        {
            return actorTags.Intersect(_actorTags).Any();
        }

        #endregion
    }
    
    [Serializable]
    public abstract class ActorFilter
    {
        public abstract bool PassesFilter(Actor _actor);
    }

    [Serializable]
    public class ActorFilter_IsNamed : ActorFilter
    {
        public string name;
    
        public override bool PassesFilter(Actor _actor)
        {
            return _actor.displayName == name;
        }
    }

    [Serializable]
    public class ActorFilter_IsID : ActorFilter
    {
        public string id;

        public override bool PassesFilter(Actor _actor)
        {
            return _actor.id == id;
        }
    }

    [Serializable]
    public class ActorFilter_HasActorTag : ActorFilter
    {
        public List<ActorTag> groups;

        public override bool PassesFilter(Actor _actor)
        {
            return _actor.HasActorTag(groups);
        }
    }
}

