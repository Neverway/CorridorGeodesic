//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: A duplicate data structure of characterData since dataStructure
//  runs into issues with data being written directly to the Scriptable Object
// Notes: Greetings future me! You are probably here trying to figure out why
//  the Pawn class has defaultState inheriting this class, but currentState
//  inheriting PlayerStateData.
//  To simplify, Scriptable Objects are instance based.
//  So if we tried to change the currentState's movement speed, it would change it 
//  for ALL playerStates. (THAT'S BAD!) So instead we use PlayerStateData to 
//  separate the data from the scriptable object. "Well why not change player
//  state to not be a SO?" I hear you say. Well my dear sweet coding idiot,
//  we still need PlayerState to be a SO so we can instance separate character
//  stats for each type of character. Good effin luck! ~Liz [06:01-Mar09-2024]
//  
//  Ah, well past me is an idiot. I have moved CharacterData to be its own SO
//  class. I am changing this class to now just be called CharacterState since
//  calling it CharacterStateData would be redundant and also this is for characters
//  not just for players. Good effin luck! ~Liz [22:47-Apr04-2024]
//
//=============================================================================

using System;
using UnityEngine;

namespace Neverway.Framework.PawnManagement
{
}