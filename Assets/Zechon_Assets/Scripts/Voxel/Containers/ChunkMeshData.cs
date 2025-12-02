using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshData
{
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector2> uvs;

    public static readonly Vector3[] faceDirs = {
        Vector3.forward, Vector3.back,
        Vector3.right, Vector3.left,
        Vector3.up, Vector3.down
    };

    public static readonly Vector3[,] faceVerts = {
        {new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,0,1)}, // Front
        {new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0), new Vector3(0,0,0)}, // Back
        {new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(1,0,0)}, // Right
        {new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(0,0,1)}, // Left
        {new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1)}, // Top
        {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0)}  // Bottom
    };

    public ChunkMeshData(int chunkSize)
    {
        int maxFaces = chunkSize * chunkSize * chunkSize * 6;
        verts = new List<Vector3>(maxFaces * 4);
        tris = new List<int>(maxFaces * 6);
        uvs = new List<Vector2>(maxFaces * 4);
    }

    public void Clear()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
    }

    public void EnsureCapacity(int expectedVerts, int expectedTris, int expectedUVs)
    {
        if (verts.Capacity < expectedVerts) verts.Capacity = expectedVerts;
        if (tris.Capacity < expectedTris) tris.Capacity = expectedTris;
        if (uvs.Capacity < expectedUVs) uvs.Capacity = expectedUVs;
    }
}

