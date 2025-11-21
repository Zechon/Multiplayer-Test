using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public int[,,] blocks = new int[VoxelData.ChunkSize, VoxelData.ChunkSize, VoxelData.ChunkSize];

    private Mesh mesh;
    private List<Vector3> verts = new();
    private List<int> tris = new();
    private List<Vector2> uvs = new();

    public float cubeSize = 0.5f;

    private int numTexs;

    public BlockManager blockManager;

    private static readonly Vector3[] faceDirs = {
    Vector3.forward, Vector3.back,
    Vector3.right, Vector3.left,
    Vector3.up, Vector3.down
};

    private static readonly Vector3[,] faceVerts = {
    {new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,0,1)},
    {new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0), new Vector3(0,0,0)},
    {new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(1,0,0)},
    {new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(0,0,1)},
    {new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1)},
    {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0)}
};

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        if(blockManager != null )
        {
            GetComponent<MeshRenderer>().material.mainTexture = blockManager.atlas;
            GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", blockManager.normalAtlas);
            numTexs = blockManager.allBlocks.Length;
        }

        for (int x = 0; x < VoxelData.ChunkSize; x++)
            for (int y = 0; y < VoxelData.ChunkSize; y++)
                for (int z = 0; z < VoxelData.ChunkSize; z++)
                {
                    if (y < 4) blocks[x, y, z] = 1; // floor
                    else if (y >= 4 && y < 10) blocks[x, y, z] = 2;
                    else if (y == 10) blocks[x, y, z] = 3;
                }

        int centerX = VoxelData.ChunkSize / 2;
        int centerZ = VoxelData.ChunkSize / 2;
        int pyramidBaseSize = 8;
        int pyramidHeight = 5;
        int startY = 11;
        int blockID = 5;

        for (int layer = 0; layer < pyramidHeight; layer++)
        {
            int y = startY + layer;
            int halfSize = pyramidBaseSize / 2 - layer;

            for (int x = centerX - halfSize; x <= centerX + halfSize; x++)
                for (int z = centerZ - halfSize; z <= centerZ + halfSize; z++)
                    if (x >= 0 && x < VoxelData.ChunkSize && z >= 0 && z < VoxelData.ChunkSize && y < VoxelData.ChunkSize)
                        blocks[x, y, z] = blockID;
        }

        GenerateChunkMesh();
        ApplyMesh();
    }

    bool IsVoxelSolid(int x, int y, int z)
    {
        if (x < 0 || x >= VoxelData.ChunkSize ||
            y < 0 || y >= VoxelData.ChunkSize ||
            z < 0 || z >= VoxelData.ChunkSize)
            return false;

        return blocks[x, y, z] != 0;
    }

    void GenerateChunkMesh()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();

        for (int x = 0; x < VoxelData.ChunkSize; x++)
            for (int y = 0; y < VoxelData.ChunkSize; y++)
                for (int z = 0; z < VoxelData.ChunkSize; z++)
                {
                    if (blocks[x,y,z] != 0)
                    {
                        AddVoxelFaces(x, y, z);
                    }
                }
    }

    void AddVoxelFaces(int x, int y, int z)
    {
        Vector3 blockPos = new Vector3(x, y, z);

        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            Vector3 dir = faceDirs[faceIndex];

            // Neighbor check
            int nx = x + (int)dir.x;
            int ny = y + (int)dir.y;
            int nz = z + (int)dir.z;

            if (!IsVoxelSolid(nx, ny, nz))
                AddFace(blockPos, faceIndex);
        }
    }

    void AddFace(Vector3 blockPos, int faceIndex)
    {
        int start = verts.Count;

        for (int i = 0; i < 4; i++)
        {
            verts.Add(blockPos * cubeSize + faceVerts[faceIndex, i] * cubeSize);
        }

        tris.Add(start + 2);
        tris.Add(start + 1);
        tris.Add(start);
        tris.Add(start);
        tris.Add(start + 3);
        tris.Add(start + 2);

        int x = (int)blockPos.x;
        int y = (int)blockPos.y;
        int z = (int)blockPos.z;
        int blockID = blocks[x, y, z];

        if (blockID == 0) return;

        BlockClass block = blockManager.allBlocks[blockID];
        Texture2D faceTex = block.blockFaceTextures[0];
        if (faceIndex == 4) faceTex = block.blockFaceTextures[1];
        else if (faceIndex == 5) faceTex = block.blockFaceTextures[2];

        float tileSizeU = 1f / numTexs;
        float tileSizeV = 1f / 3f;

        float xOffset = blockID * tileSizeU;
        float yOffset = (faceIndex == 4) ? tileSizeV * 1 : (faceIndex == 5) ? tileSizeV * 2 : 0f;

        uvs.Add(new Vector2(xOffset, yOffset));                // bottom-left
        uvs.Add(new Vector2(xOffset, yOffset + tileSizeV));   // top-left
        uvs.Add(new Vector2(xOffset + tileSizeU, yOffset + tileSizeV)); // top-right
        uvs.Add(new Vector2(xOffset + tileSizeU, yOffset));   // bottom-right
    }

    void ApplyMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
