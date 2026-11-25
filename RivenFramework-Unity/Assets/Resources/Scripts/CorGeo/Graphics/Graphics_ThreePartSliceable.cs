//==========================================( Neverway 2027 )=========================================================//
// Author
//  Connorses
//
// Contributors
//  Liz M.
//
// Description
//  Manages the 3 Graphics_SliceableSection objects that make up a sliceable object that needs to animate.
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

    /// <summary>
    /// Manages the 3 Graphics_SliceableSection objects that make up a sliceable object that needs to animate.
    /// </summary>
    public class Graphics_ThreePartSliceable : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("Enable for physics props and other moving objects to keep the three copies following the original while a rift is active")]
            [SerializeField] private bool followsRootWhileSliced;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            [Tooltip("A reference to the sliceable section that represents the object in A space")]
            [SerializeField] Graphics_SliceableSection sectionA;
            [Tooltip("A reference to the sliceable section that represents the object in B space")]
            [SerializeField] Graphics_SliceableSection sectionB;
            [Tooltip("A reference to the sliceable section that represents the object in Null space")]
            [SerializeField] Graphics_SliceableSection sectionNull;
            private RiftManager riftManager;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private bool sliceStarted = false;
            
            private struct SectionFollow
            {
                public Graphics_SliceableSection section;
                public Matrix4x4 rootToSection;
                public Matrix4x4 containerRestWorldToLocal;
            }
            private SectionFollow[] followers;
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                riftManager = FindObjectOfType<RiftManager> ();
                sectionA.space = SliceSpace.Plane1;
                sectionB.space = SliceSpace.Plane2;
                sectionNull.space = SliceSpace.Null;
                sectionB.gameObject.SetActive (false);
                sectionNull.gameObject.SetActive (false);
            }
            
            private void LateUpdate()
            {
                if (!sliceStarted || followers == null || riftManager == null) return;

                // What space the objects transform is in (a = 0, b= 1, 2 = null)
                int home = 0;
                switch (riftManager.GetRiftSpaceOfPoint(transform.position))
                {
                    case RiftSpace.B: 
                        home = 1; break;
                    case RiftSpace.NULLSpace: 
                        home = 2; break;
                }

                SectionFollow h = followers[home];
                Matrix4x4 curWorldToLocalHome = h.section.transform.parent.worldToLocalMatrix;
                if (Mathf.Abs(curWorldToLocalHome.determinant) < 1e-6f) return;

                // Undo POSITION compression so we can calculate POSITION shtuff in world-space
                Vector3 restPosition = h.containerRestWorldToLocal.inverse.MultiplyPoint3x4(
                    curWorldToLocalHome.MultiplyPoint3x4(transform.position));
                Matrix4x4 restPose = Matrix4x4.TRS(restPosition, transform.rotation, transform.lossyScale);

                foreach (SectionFollow f in followers)
                {
                    Matrix4x4 local = f.containerRestWorldToLocal * restPose * f.rootToSection;
                    Transform t = f.section.transform;
                    t.localPosition = local.GetColumn(3);
                    t.localRotation = local.rotation;
                    t.localScale = local.lossyScale;
                }
            }

            private void OnEnable ()
            {
                RiftManager_StateHandler.OnStateChanged += OnStateChanged;
            }

            private void OnDisable ()
            {
                RiftManager_StateHandler.OnStateChanged -= OnStateChanged;
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            /// <summary>
            /// Enable the Graphics_SliceableSection objects and assign them to separate spaces.
            /// </summary>
            public void StartSlicing()
            {
                if (sliceStarted) return;
                sliceStarted = true;
                //SectionA should always be enabled so it renders when there's no slice happening.
                sectionB.gameObject.SetActive(true);
                sectionNull.gameObject.SetActive(true);

                sectionA.StartSlicing ();
                sectionB.StartSlicing ();
                sectionNull.StartSlicing ();

                sectionA.transform.SetParent(riftManager.spaceController.spaceContainerA.transform, true);
                sectionB.transform.SetParent (riftManager.spaceController.spaceContainerB.transform, true);
                sectionNull.transform.SetParent(riftManager.spaceController.spaceContainerNull.transform, true);

                if (followsRootWhileSliced) BuildFollowers();
            }

            /// <summary>
            /// Disable all the Graphics_SliceableSection objects.
            /// </summary>
            public void StopSlicing()
            {
                sectionB.gameObject.SetActive (false);
                sectionNull.gameObject.SetActive (false);

                if (sliceStarted == false) return;
                sliceStarted = false;

                sectionA.StopSlicing ();
                sectionB.StopSlicing ();
                sectionNull.StopSlicing ();
                
                if (followers != null)
                {
                    foreach (SectionFollow follower in followers)
                    {
                        Transform trans = follower.section.transform;
                        trans.localPosition = follower.rootToSection.GetColumn(3);
                        trans.localRotation = follower.rootToSection.rotation;
                        trans.localScale = follower.rootToSection.lossyScale;
                    }
                    followers = null;
                }
            }

            public void SetBool (string _name, bool _isPowered)
            {
                sectionA.SetBool (_name, _isPowered);
                sectionB.SetBool (_name, _isPowered);
                sectionNull.SetBool (_name, _isPowered);
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            /// <summary>
            /// Called by RiftManager_StateHandler OnStateChanged event
            /// </summary>
            private void OnStateChanged ()
            {
                // Whoops, we need this reference, but it's not here!
                if (riftManager is null) riftManager = GameInstance.Get<RiftManager>();
                // Still didn't find it? Okay, stop everything else
                if (riftManager is null) return;

                // If the space containers don't exist don't bother trying to continue since the rift clearly isn't actually active        
                if (!riftManager.spaceController.spaceContainerA) return;

                if (riftManager.stateHandler.IsState<RiftState_None>())
                {
                    StopSlicing ();
                }
                else
                {
                    //If the rift is real
                    StartSlicing ();
                }
            }
            
            private void BuildFollowers()
            {
                followers = new SectionFollow[3];
                var sections = new[] { sectionA, sectionB, sectionNull };
                for (int i = 0; i < 3; i++)
                {
                    Transform t = sections[i].transform;
                    followers[i] = new SectionFollow
                    {
                        section = sections[i],
                        rootToSection = transform.worldToLocalMatrix * t.localToWorldMatrix,
                        containerRestWorldToLocal = t.parent.worldToLocalMatrix
                    };
                }
            }
        #endregion
        #endregion
    }
