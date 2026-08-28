using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTransformUtils : MonoBehaviour
{
    public Transform targetToMoveToMyRiftedPos;
    public RiftSpace CurrentTransformRiftSpace;
    //public RiftSpace RiftSpaceOfRiftedPos;

    public void Update()
    {
        //RiftSpaceOfRiftedPos = transform.position.ApplyRiftToPos().GetRiftSpaceOfRiftedPos();
        CurrentTransformRiftSpace = transform.position.GetRiftSpaceOfUnriftedPos();
        if (targetToMoveToMyRiftedPos != null)
        {
            targetToMoveToMyRiftedPos.transform.position = transform.position.ApplyRiftToPos();
        }
    }
}
