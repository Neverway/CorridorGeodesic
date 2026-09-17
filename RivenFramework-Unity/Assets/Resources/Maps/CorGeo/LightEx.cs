using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEx : MonoBehaviour
{
    public Light light;
    // Start is called before the first frame update
    
    
    [ExecuteAlways]
    void Start()
    {
        if (!light) light = GetComponent<Light>();
        light.renderMode = LightRenderMode.ForceVertex;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
