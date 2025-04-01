//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Data structure for the stats on a pawn
// Notes:
//
//=============================================================================

using UnityEngine;

namespace Neverway.Framework.PawnManagement
{
    [CreateAssetMenu(
        fileName = "Character_FirstPerson", 
        menuName = "Gamemodes/FirstPerson/Characters/Character_FirstPerson")]
    public class CharacterData_FirstPerson : CharacterData
    {
        // Add project specific variables below this line!
        public float groundDrag;
        public float airDrag;
        public float maxHorizontalMovementSpeed;
        public float maxHorizontalAirSpeed;
        public float movementMultiplier;
        public float airMovementMultiplier;
        public float gravityMultiplier;
        public float jumpForce;
        [Tooltip("The multiplier added to the movementSpeed while grounded and sprinting")]
        public float sprintSpeedMultiplier;
        [Tooltip("Essentially how long it takes you to get up to speed while sprinting")]
        public float sprintAcceleration;
        [Tooltip("The maximum a player can set upwards in units when they hit a wall that's potentially a step")]
        public float maxStepHeight;
        [Tooltip("How much to overshoot into the direction a potential step in units when testing. High values prevent player from walking up tiny steps but may cause problems.")]
        public float stepSearchOvershoot;
        [Tooltip("How steep of an angle can you walk normally/jump on?")]
        public float steepSlopeAngle;
        [Tooltip("Whether to use the health regen per second or not.")]
        public bool autoRegenHealth;
        public float healthRegenPerSecond;
        [Tooltip("Delay after taking damage before health regenerates.")]
        public float healthRegenDelay;
        public float fallDamage;
        public float minFallDamage;
        public float fallDamageVelocity;
        public float minFallDamageVelocity;
    }
}