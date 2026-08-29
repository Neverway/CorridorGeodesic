using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Acts as an identifier for an object, assigning it an id, what groups it's a part of, and a human readable display name
/// </summary>
public class BActor : MonoBehaviour
{
    public string id;
    public string displayName;
    public List<ActorGroup> groups;

    public bool IsInAnyOfGroups(List<ActorGroup> _groups)
    {
        return groups.Intersect(_groups).Any();
    }
}

[Serializable]
public abstract class ActorFilter
{
    public abstract bool PassesFilter(BActor _actor);
}

[Serializable]
public class ActorFilter_IsNamed : ActorFilter
{
    public string name;
    
    public override bool PassesFilter(BActor _actor)
    {
        return _actor.displayName == name;
    }
}

[Serializable]
public class ActorFilter_IsID : ActorFilter
{
    public string id;

    public override bool PassesFilter(BActor _actor)
    {
        return _actor.id == id;
    }
}

[Serializable]
public class ActorFilter_IsInAnyGroup : ActorFilter
{
    public List<ActorGroup> groups;

    public override bool PassesFilter(BActor _actor)
    {
        return _actor.IsInAnyOfGroups(groups);
    }
}