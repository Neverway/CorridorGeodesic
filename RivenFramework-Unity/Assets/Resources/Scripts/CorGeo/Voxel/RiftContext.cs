//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

public class RiftContext : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool IsRiftActive = false;

    public Plane PlaneA;
    public Plane PlaneB;

    public float NSpaceScale = 1f;
    public Vector3 NSpaceScalePivot = Vector3.zero;
    public Vector3 BSpaceShift = Vector3.zero;


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void UpdateRift(bool _active, Plane _planeA, Plane _planeB, float _NSpaceScale, Vector3 _NSpaceScalePivot, Vector3 _BSpaceShift)
    {
        IsRiftActive = _active;
        PlaneA = _planeA;
        PlaneB = _planeB;
        NSpaceScale = _NSpaceScale;
        NSpaceScalePivot = _NSpaceScalePivot;
        BSpaceShift = _BSpaceShift;
    }

    public void DeactivateRift()
    {
        IsRiftActive = false;
    }

    private void OnDrawGizmos()
    {
        if (!IsRiftActive) return;
        
        DrawPlaneGizmo(PlaneA, new Color(0.2f, 0.6f, 1f, 0.35f), "Plane A");
        DrawPlaneGizmo(PlaneB, new Color(1f, 0.4f, 1f, 0.35f), "Plane B");

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(NSpaceScalePivot, 0.15f);
        UnityEditor.Handles.Label(NSpaceScalePivot + Vector3.up * 0.3f, $"NSpaceScalePivot {NSpaceScale:F2}");

        Gizmos.color = new Color(1, 0.4f, 0.1f, 1f);
        Vector3 planeBPoint = -PlaneB.normal * PlaneB.distance;
        Gizmos.DrawLine(planeBPoint, planeBPoint+BSpaceShift);
        Gizmos.DrawSphere(planeBPoint + BSpaceShift, 0.1f);
    }

    private static void DrawPlaneGizmo(Plane _plane, Color _color, string _label)
    {
        Vector3 center = -_plane.normal * _plane.distance;
        Vector3 right = Vector3.Cross(_plane.normal, Vector3.up);
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(_plane.normal, Vector3.forward);
        right.Normalize();
        Vector3 up = Vector3.Cross(right, _plane.normal).normalized;

        float size = 5f;
        
        Vector3 c0 = center + ( right + up) * size;
        Vector3 c1 = center + (-right + up) * size;
        Vector3 c2 = center + (-right - up) * size;
        Vector3 c3 = center + ( right - up) * size;
 
        Gizmos.color = _color;
        Gizmos.DrawLine(c0, c1);
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c2, c3);
        Gizmos.DrawLine(c3, c0);
        Gizmos.DrawLine(c0, c2);
        Gizmos.DrawLine(c1, c3);

        Gizmos.color = new Color(_color.r, _color.g, _color.b, 1f);
        Gizmos.DrawLine(center, center + _plane.normal * 1.5f);
        Gizmos.DrawSphere(center + _plane.normal * 1.5f, 0.08f);

        UnityEditor.Handles.Label(center + Vector3.up * (size + 0.3f), _label);
    }


    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}