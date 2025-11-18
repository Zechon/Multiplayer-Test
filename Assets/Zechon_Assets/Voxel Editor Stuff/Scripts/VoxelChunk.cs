using NUnit.Framework.Internal;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelChunk : MonoBehaviour
{
    public const int chSize = 16;
    public int[,,] blocks = new int[chSize, chSize, chSize];

    [Header("Material Settings")]
    public Material defaultMaterial;
    public BlockDatabase blockDatabase;

    MeshFilter mf;
    MeshCollider mc;
    MeshRenderer mr;

    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();
        mr = GetComponent<MeshRenderer>();

        if (mr.sharedMaterial == null && defaultMaterial != null)
            mr.sharedMaterial = defaultMaterial;
    }

    public void GenerateMesh()
    {
        VoxelMesher.GenerateMesh(this, mf, mc, mr, defaultMaterial, blockDatabase);
    }
}
