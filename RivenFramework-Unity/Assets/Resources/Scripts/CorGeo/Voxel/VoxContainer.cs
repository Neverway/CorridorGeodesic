// Written by Liz M.
// Created following this guide: https://youtu.be/EubjobNVJdM

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class VoxContainer : MonoBehaviour
{
    public Vector3 containerPosition;
    public Dictionary<Vector3, Voxel> containerData;
    
    [Header("Debugging")]
    [Tooltip("In a standard voxel system there is no need to generate a voxel for air, enable this if you want to generate air blocks")]
    public bool doNotSkipGeneratingAirBlocks = false;
    [Tooltip("In a standard voxel system you don't usually want to draw faces that can't be seen or are covering each other, enable this if you want to generate touching faces")]
    public bool doNotSkipGeneratingBackfaces = false;
    
    private VoxMeshData voxMeshData;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public void Initialize(Material material, Vector3 position)
    {
        ConfigureComponents();
        containerData = new Dictionary<Vector3, Voxel>();
        meshRenderer.sharedMaterial = material;
        containerPosition = position;
    }

    public void ClearData()
    {
        containerData.Clear();
    }

    /// <summary>
    /// Get the mesh components required for rendering the voxels
    /// </summary>
    private void ConfigureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void GenerateMesh()
    {
        voxMeshData.ClearData();

        Vector3 blockPos;
        Voxel block;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];
        
        VoxelColor voxelColor;
        Color voxelColorAlpha;
        Vector2 voxelSmoothness;
        
        // Iterate over each face direction
        foreach (KeyValuePair<Vector3, Voxel> kvp in containerData)
        {
            // Don't bother creating a voxel for air
            if (kvp.Value.ID == 0 && !doNotSkipGeneratingAirBlocks) continue;
            
            blockPos = kvp.Key;
            block = kvp.Value;
            
            voxelColor = VoxWorldManager.Instance.worldColors[block.ID - 1];
            voxelColorAlpha = voxelColor.color;
            voxelColorAlpha.a = 1;
            voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);
            
            int voxelFacesCount = 6;
            for (int i = 0; i < voxelFacesCount; i++)
            {
                // Backface culling
                if (this[blockPos + voxelFaceChecks[i]].isSolid && !doNotSkipGeneratingBackfaces) continue;
                
                // Draw this face
                // Collect the appropriate vertices from the default vertices and add the block position
                int faceVerticesCount = 4;
                for (int j = 0; j < faceVerticesCount; j++)
                {
                    faceVertices[j] = voxelVertices[voxelVertexIndex[i, j]] + blockPos;
                    faceUVs[j] = voxelUVs[j];
                }

                for (int j = 0; j < 6; j++)
                {
                    voxMeshData.vertices.Add(faceVertices[voxelTris[i,j]]);
                    voxMeshData.UVs.Add(faceUVs[voxelTris[i,j]]);
                    voxMeshData.colors.Add(voxelColorAlpha);
                    voxMeshData.UVs2.Add(voxelSmoothness);
                    voxMeshData.triangles.Add(counter++);
                }
            }
        }
    }

    public void UploadMesh()
    {
        voxMeshData.UploadMesh();

        if (meshRenderer == null)
        {
            ConfigureComponents();
        }

        meshFilter.mesh = voxMeshData.mesh;
        if (voxMeshData.vertices.Count > 3)
        {
            meshCollider.sharedMesh = voxMeshData.mesh;
        }
    }

    public Voxel this[Vector3 index]
    {
        get
        {
            if (containerData.ContainsKey(index))
            {
                return containerData[index];
            }
            else
            {
                return emptyVoxel;
            }
        }

        set
        {
            if (containerData.ContainsKey(index))
            {
                containerData[index] = value;
            }
            else
            {
                containerData.Add(index, value);
            }
        }
    }
    
    public static Voxel emptyVoxel = new Voxel() { ID = 0 };
    
    #region  Voxel Mesh Data
    public struct VoxMeshData
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> UVs;
        public List<Vector2> UVs2;
        public List<Color> colors;

        public bool initialized;

        public void ClearData()
        {
            if (!initialized)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                UVs = new List<Vector2>();
                UVs2 = new List<Vector2>();
                colors = new List<Color>();
                
                initialized = true;
                mesh = new Mesh();
            }
            else
            {
                vertices.Clear();
                triangles.Clear();
                UVs.Clear();
                UVs2.Clear();
                colors.Clear();
                mesh.Clear();
            }
        }
        /// <summary>
        /// Assign the vertices, triangles, and uvs HEHEHEHA
        /// </summary>
        /// <param name="sharedVerticies"></param>
        public void UploadMesh(bool sharedVerticies = false)
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetColors(colors);
            mesh.SetUVs(0, UVs);
            mesh.SetUVs(2, UVs2);
            
            mesh.Optimize();
            
            mesh.RecalculateNormals();
            
            mesh.RecalculateBounds();
            
            mesh.UploadMeshData(false);
        }
    }
    #endregion
    
    #region Voxel Statics

    /// <summary>
    /// Defines the basic shape of a cubic voxel's vertex points
    /// </summary>
    private static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0, 0, 0), // vertex 0 (bottom left)
        new Vector3(1, 0, 0), // vertex 1 (bottom right)
        new Vector3(0, 1, 0), // vertex 2 (top right)
        new Vector3(1, 1, 0), // vertex 3 (top left)
        
        new Vector3(0, 0, 1), // vertex 4 (bottom left)
        new Vector3(1, 0, 1), // vertex 5 (bottom right)
        new Vector3(0, 1, 1), // vertex 6 (top right)
        new Vector3(1, 1, 1), // vertex 7 (top left)
    };

    private static Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0, 0, -1), // Back
        new Vector3(0, 0, 1), // Front
        new Vector3(-1, 0, 0), // Left
        new Vector3(1, 0, 0), // Right
        new Vector3(0, -1, 0), // Bottom
        new Vector3(0, 1, 0), // Top
    };

    /// <summary>
    /// I believe this defines the basic shape of a cubic voxel's vertex connections
    /// </summary>
    private static readonly int[,] voxelVertexIndex = new int[6, 4]
    {
        { 0, 1, 2, 3 },
        { 4, 5, 6, 7 },
        { 4, 0, 6, 2 },
        { 5, 1, 7, 3 },
        { 0, 1, 4, 5 },
        { 2, 3, 6, 7 },
    };

    private static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1)
    };

    private static readonly int[,] voxelTris = new int[6, 6]
    {
        { 0, 2, 3, 0, 3, 1 },
        { 0, 1, 2, 1, 3, 2 },
        { 0, 2, 3, 0, 3, 1 },
        { 0, 1, 2, 1, 3, 2 },
        { 0, 1, 2, 1, 3, 2 },
        { 0, 2, 3, 0, 3, 1 }
    };

    #endregion
}
