//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
// Takes a source mesh and creates a new one with all of its vertices moved by sourceToTarget
// This is used by CorGeo to fix a quirk with colliders accuracy drifting when a mesh is scaled on a non-linear axis
//====================================================================================================================//

using UnityEngine;

    /// <summary>
    /// Takes a source mesh and creates a new one with all of its vertices moved by sourceToTarget
    /// This is used by CorGeo to fix a quirk with colliders accuracy drifting when a mesh is scaled on a non-linear axis
    /// </summary>
    public static class RiftMeshBaker
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion

        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            /// <summary>
            /// 
            /// </summary>
            /// <param name="_source">The input mesh that needs its axis aligned</param>
            /// <param name="_sourceToTarget">The axis transformation</param>
            /// <returns>A new mesh with its axis aligned</returns>
            public static Mesh BakeToSpace(Mesh _source, Matrix4x4 _sourceToTarget)
            {
                Mesh baked = Object.Instantiate(_source);
                baked.name = _source.name + "_riftBaked";

                Vector3[] vertices = _source.vertices;
                Vector3[] normals = _source.normals;
                Vector4[] tangents = _source.tangents;

                Matrix4x4 normalMatrix = _sourceToTarget.inverse.transpose;
                bool mirrored = _sourceToTarget.determinant < 0f;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = _sourceToTarget.MultiplyPoint3x4(vertices[i]);
                }

                if (normals.Length == vertices.Length)
                {
                    for (int i = 0; i < normals.Length; i++)
                    {
                        normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized;
                    }
                    baked.normals = normals;
                }

                if (tangents.Length == vertices.Length)
                {
                    for (int i = 0; i < tangents.Length; i++)
                    {
                        Vector3 t = _sourceToTarget.MultiplyVector(new Vector3(tangents[i].x, tangents[i].y, tangents[i].z)).normalized;
                        tangents[i] = new Vector4(t.x, t.y, t.z, mirrored ? -tangents[i].w : tangents[i].w);
                    }
                    baked.tangents = tangents;
                }

                baked.vertices = vertices;

                if (mirrored)
                {
                    for (int s = 0; s < baked.subMeshCount; s++)
                    {
                        int[] tris = baked.GetTriangles(s);
                        for (int i = 0; i < tris.Length; i += 3)
                        {
                            (tris[i], tris[i + 1]) = (tris[i + 1], tris[i]);
                        }
                        baked.SetTriangles(tris, s);
                    }
                }

                baked.RecalculateBounds();
                return baked;
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
