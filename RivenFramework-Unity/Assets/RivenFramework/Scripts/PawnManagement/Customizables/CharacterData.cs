//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Data structure for the stats on a pawn
// Notes:
//
//=============================================================================

using System;
using UnityEngine;

namespace Neverway.Framework.PawnManagement
{
    public class CharacterData : Actor
    {
        public string characterName;
        public float health;
        public float invulnerabilityTime;
        public float movementSpeed;
        public string team;
        public RuntimeAnimatorController animationController;
        public CharacterSounds characterSounds;
        public Vector3 groundCheckOffset;
        public float groundCheckRadius;
        [Tooltip("The collision layers that will be checked when testing if the entity is grounded")]
        public LayerMask groundMask;
    }

    [Serializable]
    public class CharacterSounds
    {
        public AudioClip hurt;
        public AudioClip heal;
        public AudioClip death;

        public AudioClip alerted;
    }
}