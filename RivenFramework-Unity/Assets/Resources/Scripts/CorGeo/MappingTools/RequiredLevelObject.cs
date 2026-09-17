using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to tie a required level object to it's prefab, so I can unpack them in the hierarchy to improve performance
/// </summary>
public class RequiredLevelObject : MonoBehaviour
{
    public string m_SourceGuid;
}
