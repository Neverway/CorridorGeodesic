using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define_HealthEvent
{
    public struct E_Damage
    {
        public int Amount;
        public List<string> GroupFilter;
        public GameObject Target;
        public GameObject Source;
    }

    public struct E_Death
    {
        public GameObject Target;
        public E_Damage CauseOfDeath;
    }
}
