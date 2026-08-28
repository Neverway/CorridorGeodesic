using RivenFramework;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class RiftTransformUtils
{
    private static RiftContext _rifts;
    private static RiftContext Rifts 
    {
        get 
        {
            if (_rifts != null) return _rifts;
            _rifts = GameInstance.Get<RiftContext>();
            if (_rifts != null) return _rifts;
            throw new Exception($"{nameof(RiftContext)} could not be found for one of the methods used in {nameof(RiftTransformUtils)}");
        }
    }

    private static Vector3 BSpacePosDelta => Rifts.NSpaceScalePivot + (Rifts.RiftScaleDirection / Rifts.NSpaceScale);

    private static Exception Exception_RiftSpaceNotImplemented(RiftSpace space, [CallerMemberName] string function = "")
        => new NotImplementedException($"RiftSpace.{space} has not been handled for method:" +
            $" {nameof(RiftTransformUtils)}.{function}");
    private static Exception Exception_NegativeRiftNotImplemetned([CallerMemberName] string function = "")
        => new NotImplementedException($"Negative rifts have not yet been implemented for method: " +
            $"{nameof(RiftTransformUtils)}.{function}");

    public static RiftSpace GetRiftSpaceOfRiftedPos(this Vector3 pos)
    {
        if (!Rifts.IsRiftActive) return RiftSpace.none;

        Vector3 riftToPos = pos - Rifts.NSpaceScalePivot;
        if (Vector3.Dot(Rifts.RiftScaleDirection, riftToPos.normalized) <= 0f)
            return RiftSpace.A;

        Vector3 planeBPoint = -Rifts.PlaneB.normal * Rifts.PlaneB.distance;
        riftToPos = pos - planeBPoint;

        if (Vector3.Dot(-Rifts.RiftScaleDirection, riftToPos.normalized) <= 0f)
            return RiftSpace.B;

        return RiftSpace.NULLSpace;

    }
    public static RiftSpace GetRiftSpaceOfUnriftedPos(this Vector3 pos)
    {
        if (!Rifts.IsRiftActive) return RiftSpace.none;

        Vector3 riftToPos = pos - Rifts.NSpaceScalePivot;
        if (Vector3.Dot(Rifts.RiftScaleDirection, riftToPos.normalized) <= 0f)
            return RiftSpace.A;

        Vector3 planeBPoint = -Rifts.PlaneB.normal * Rifts.PlaneB.distance;
        planeBPoint -= BSpacePosDelta;

        riftToPos = pos - planeBPoint;
        if (Vector3.Dot(-Rifts.RiftScaleDirection, riftToPos.normalized) <= 0f)
            return RiftSpace.B;

        return RiftSpace.NULLSpace;
    }

    public static Vector3 UnapplyRiftToPos(this Vector3 pos) => UnapplyRiftSpaceToPos(pos, pos.GetRiftSpaceOfRiftedPos());
    public static Vector3 ApplyRiftToPos(this Vector3 pos) => ApplyRiftSpaceToPos(pos, pos.GetRiftSpaceOfUnriftedPos());
    public static Vector3 UnapplyRiftSpaceToPos(this Vector3 pos, RiftSpace space)
    {
        switch (space)
        {
            //There is no change to make if there is no rift, or position is in A space
            case RiftSpace.none: return pos;
            case RiftSpace.A: return pos;

            case RiftSpace.B: return UnapplyBSpaceToPos(pos);
            case RiftSpace.NULLSpace: return UnapplyNULLSpaceToPos(pos);
        }

        //This will only happen if a new RiftSpace is added and not been implemented here yet
        throw Exception_RiftSpaceNotImplemented(space);
    }
    public static Vector3 ApplyRiftSpaceToPos(this Vector3 pos, RiftSpace space)
    {
        switch (space)
        {
            //There is no change to make if there is no rift, or position is in A space
            case RiftSpace.none: return pos;
            case RiftSpace.A: return pos;

            case RiftSpace.B: return ApplyBSpaceToPos(pos);
            case RiftSpace.NULLSpace: return ApplyNULLSpaceToPos(pos);
        }

        //This will only happen if a new RiftSpace is added and not been implemented here yet
        throw Exception_RiftSpaceNotImplemented(space);
    }

    public static Vector3 ApplyBSpaceToPos(Vector3 pos)
    {
        //Make no change if no rift is active
        if (!Rifts.IsRiftActive) return pos;

        return pos + BSpacePosDelta;
    }
    public static Vector3 UnapplyBSpaceToPos(Vector3 pos)
    {
        //Make no change if no rift is active
        if (!Rifts.IsRiftActive) return pos;

        return pos - BSpacePosDelta;
    }
    public static Vector3 ApplyNULLSpaceToPos(Vector3 pos)
    {
        //Make no change if no rift is active
        if (!Rifts.IsRiftActive) return pos;

        return ScalePosOnNULLSpaceOrigin(pos, Rifts.RiftScaleDirection, Rifts.NSpaceScale);
    }
    public static Vector3 UnapplyNULLSpaceToPos(Vector3 pos)
    {
        //Make no change if no rift is active
        if (!Rifts.IsRiftActive) return pos;
        return ScalePosOnNULLSpaceOrigin(pos, Rifts.RiftScaleDirection, 1f / Rifts.NSpaceScale);
    }
    public static Vector3 ScalePosOnNULLSpaceOrigin(Vector3 pos, Vector3 dir, float scale)
    {
        if (scale < 0f) throw Exception_NegativeRiftNotImplemetned();

        Vector3 origin = Rifts.NSpaceScalePivot;

        pos -= origin;
        pos += (scale - 1f) * Vector3.Dot(pos, dir) * dir;
        return pos + origin;
    }







    public static Vector3 UnapplyRiftSpaceToDir(this Vector3 dir, RiftSpace space)
    {
        switch (space)
        {
            case RiftSpace.none: return dir;
            case RiftSpace.A: return dir;
            case RiftSpace.B: return dir;
            case RiftSpace.NULLSpace: return UnapplyNULLSpaceToDir(dir);
        }
        throw Exception_RiftSpaceNotImplemented(space);
    }
    public static Vector3 ApplyRiftSpaceToDir(this Vector3 dir, RiftSpace space)
    {
        switch (space)
        {
            case RiftSpace.none: return dir;
            case RiftSpace.A: return dir;
            case RiftSpace.B: return dir;
            case RiftSpace.NULLSpace: return ApplyNULLSpaceToDir(dir);
        }
        throw Exception_RiftSpaceNotImplemented(space);
    }

    public static Vector3 ApplyNULLSpaceToDir(Vector3 dir)
    {
        throw new NotImplementedException();
    }
    public static Vector3 UnapplyNULLSpaceToDir(Vector3 dir)
    {
        throw new NotImplementedException();
    }





}
