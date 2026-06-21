//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose: Always make a gameobject face the camera 
// Notes: (usually used for 2D objects like sprites to make them appear 3D)
//
//=============================================================================

using UnityEngine;

namespace Neverway.Framework
{
    public class Object_Billboard : MonoBehaviour
    {
        //=-----------------=
        // Public Variables
        //=-----------------=
        [SerializeField] private float updateRate = 1;


        //=-----------------=
        // Private Variables
        //=-----------------=


        //=-----------------=
        // Reference Variables
        //=-----------------=
        private CameraManager cameraManager;
        private Camera activeRenderingCamera;


        //=-----------------=
        // Mono Functions
        //=-----------------=
        private void Start()
        {
            cameraManager = FindObjectOfType<CameraManager>();
            InvokeRepeating(nameof(UpdateBillboard), 0, updateRate);
        }

        private void UpdateBillboard()
        {
            if (!cameraManager)
            {
                cameraManager = FindObjectOfType<CameraManager>();
                return;
            }

            if (!activeRenderingCamera)
            {
                activeRenderingCamera = cameraManager.GetActiveRenderingCamera();
                return;
            }
            
            
            transform.LookAt(activeRenderingCamera.transform.position, activeRenderingCamera.transform.up);
        }

        //=-----------------=
        // Internal Functions
        //=-----------------=


        //=-----------------=
        // External Functions
        //=-----------------=
    }
}