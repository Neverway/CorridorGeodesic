using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GI_Terminal : MonoBehaviour
{
    public static GI_Terminal Instance { get; private set; }

    public VirtualFileSystem FileSystem { get; private set; }
 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
 
        FileSystem = VFSF_Riftdeck.BuildDefault();
    }
}
