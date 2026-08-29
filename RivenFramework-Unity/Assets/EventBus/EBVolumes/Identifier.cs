using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Identifier : MonoBehaviour
{
    public List<string> groups;

    public bool IsInAnyOfGroups(List<string> _groups)
    {
        return groups.Intersect(_groups).Any();
    }
}
