using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public int[,,] blocks = new int[VoxelData.ChunkSize, VoxelData.ChunkSize, VoxelData.ChunkSize];

    public Mesh mesh;
    private List<Vector3> verts = new();
    private List<int> tris = new();
    private List<Vector2> uvs = new();

    public float cubeSize = 0.5f;

    public int numTexs;

    public BlockManager blockManager;

    public Vector3Int chunkCoord; // assigned by VoxelWorld

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

        GenerateChunkMesh();
        ApplyMesh();
    }

    bool IsVoxelSolid(int x, int y, int z, int faceIndex, int currentY)
    {
        int cs = VoxelWorld.Instance.chunkSize;
        int gx = chunkCoord.x * cs + x;
        int gy = chunkCoord.y * cs + y;
        int gz = chunkCoord.z * cs + z;

        int neighborID = VoxelWorld.Instance.GetBlock(gx, gy, gz);
        if (neighborID == 0) return false;

        BlockClass neighbor = blockManager.allBlocks[neighborID];
        if (neighbor == null) return false;

        float neighborBottom = 0f;
        float neighborTop = neighbor.height;

        // Check based on face type
        switch (faceIndex)
        {
            case 4: // top
                return neighborTop >= 1f; // full block above hides top
            case 5: // bottom
                return neighborBottom <= 0f; // bottom face rarely hidden
            default: // sides
                     // Determine current block’s vertical span
                float currentTop = 1f;

                // Cull side if neighbor fully overlaps vertical span
                return neighborTop >= currentTop;
        }
    }

    public void GenerateChunkMesh()
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
            int nx = x + (int)dir.x;
            int ny = y + (int)dir.y;
            int nz = z + (int)dir.z;

            if (!IsVoxelSolid(nx, ny, nz, faceIndex, y))
                AddFace(blockPos, faceIndex);
        }
    }

    void AddFace(Vector3 blockPos, int faceIndex)
    {
        float yOffset = 0f;

        int start = verts.Count;

        int x = (int)blockPos.x;
        int y = (int)blockPos.y;
        int z = (int)blockPos.z;
        int blockID = blocks[x, y, z];

        if (blockID == 0) return;

        BlockClass block = blockManager.allBlocks[blockID];

        if (block.isSlab)
            yOffset = 0f;
        else
            yOffset = 0f;

        for (int i = 0; i < 4; i++)
        {
            Vector3 vert = faceVerts[faceIndex, i];
  
            if (faceIndex != 4 && faceIndex != 5)
                vert.y = vert.y * block.height + yOffset;
            else if (faceIndex == 4)
                vert.y = block.height + yOffset;
            else if (faceIndex == 5)
                vert.y = 0f + yOffset;

            verts.Add(blockPos * cubeSize + vert * cubeSize);
        }

        tris.Add(start + 2);
        tris.Add(start + 1);
        tris.Add(start);
        tris.Add(start);
        tris.Add(start + 3);
        tris.Add(start + 2);

        Texture2D faceTex = block.blockFaceTextures[0];
        if (faceIndex == 4) faceTex = block.blockFaceTextures[1];
        else if (faceIndex == 5) faceTex = block.blockFaceTextures[2];

        float uvHeightScale = 1f;
        if (block.isSlab && faceIndex != 4 && faceIndex != 5)
            uvHeightScale = block.uvSideHeight;

        float tileSizeU = 1f / numTexs;
        float tileSizeV = 1f / 3f;

        float xOffset = blockID * tileSizeU;
        if (block.isSlab && faceIndex != 4 && faceIndex != 5)
        {
            yOffset = tileSizeV * 0f;
        }
        else
        {
            yOffset = (faceIndex == 4) ? tileSizeV * 1 : (faceIndex == 5) ? tileSizeV * 2 : 0f;
        }

        float vMin = yOffset;
        float vMax = yOffset + tileSizeV * (block.isSlab && faceIndex != 4 && faceIndex != 5 ? 0.5f : 1f);

        uvs.Add(new Vector2(xOffset, vMin));        // bottom-left
        uvs.Add(new Vector2(xOffset, vMax));        // top-left
        uvs.Add(new Vector2(xOffset + tileSizeU, vMax)); // top-right
        uvs.Add(new Vector2(xOffset + tileSizeU, vMin)); // bottom-right
    }

    public void ApplyMesh()
    {
        mesh.Clear();

        if (verts.Count > 65000)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        // CREATE COLLISION
        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null)
            mc = gameObject.AddComponent<MeshCollider>();

        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
    }
}
