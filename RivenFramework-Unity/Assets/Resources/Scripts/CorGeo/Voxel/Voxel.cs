// Written by Liz M.
// Created following this guide: https://youtu.be/EubjobNVJdM


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Voxel
{
    public int ID; // What is this voxel, the id corresponds to the voxel type

    [Tooltip("A solid block next to another will avoid drawing the overlapping faces by default, " +
             "this specifies what block id's are 'solid' and have backface culling, " +
             "this can be ignored by enabling doNotSkipGeneratingBackfaces in the VoxWorldManager")]
    public bool isSolid
    {
        get
        {
            return ID != 0;
        }
    }
}

/* VOXEL IDs
 0 air
 1 solid
 2 water
 3 lava
 4 oil
 5 fire
 6 geolight
 */
