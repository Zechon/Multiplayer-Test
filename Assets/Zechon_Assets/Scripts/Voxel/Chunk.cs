using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public int[,,] blocks = new int[VoxelData.ChunkSize, VoxelData.ChunkSize, VoxelData.ChunkSize];

    public ChunkMetadata metadata;

    public Mesh mesh;
    private List<Vector3> verts = new();
    private List<int> tris = new();
    private List<Vector2> uvs = new();

    public BlockManager blockManager;
    public Vector3Int chunkCoord;
    public float cubeSize = 0.5f;
    public int numTexs;

    private static readonly Vector3[] faceDirs = {
        Vector3.forward, Vector3.back,
        Vector3.right, Vector3.left,
        Vector3.up, Vector3.down
    };

    private static readonly Vector3[,] faceVerts = {
        {new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,0,1)}, // Front
        {new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0), new Vector3(0,0,0)}, // Back
        {new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(1,0,0)}, // Right
        {new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(0,0,1)}, // Left
        {new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1)}, // Top
        {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0)}  // Bottom
    };

    #region Mesh Generation

    public void GenerateChunkMesh()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();

        for (int x = 0; x < VoxelData.ChunkSize; x++)
            for (int y = 0; y < VoxelData.ChunkSize; y++)
                for (int z = 0; z < VoxelData.ChunkSize; z++)
                    if (blocks[x, y, z] != 0)
                        AddVoxelFaces(x, y, z);
    }

    private void AddVoxelFaces(int x, int y, int z)
    {
        Vector3 blockPos = new(x, y, z);
        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            Vector3 dir = faceDirs[faceIndex];
            int nx = x + (int)dir.x;
            int ny = y + (int)dir.y;
            int nz = z + (int)dir.z;

            if (!IsNeighborSolid(nx, ny, nz, faceIndex, y))
                AddFace(blockPos, faceIndex);
        }
    }

    private bool IsNeighborSolid(int x, int y, int z, int faceIndex, int currentY)
    {
        int neighborID = VoxelWorld.Instance.GetBlock(
            chunkCoord.x * VoxelData.ChunkSize + x,
            chunkCoord.y * VoxelData.ChunkSize + y,
            chunkCoord.z * VoxelData.ChunkSize + z
        );

        if (neighborID == 0) return false;

        BlockClass neighbor = blockManager.allBlocks[neighborID];
        if (neighbor == null) return false;

        // For sides: cull only if neighbor fully overlaps vertically
        if (faceIndex < 4)
        {
            return neighbor.height >= 1f; // full block or full-height slab
        }
        else if (faceIndex == 4) // top face
        {
            return false; // always show top face
        }
        else // bottom face
        {
            return false; // always show bottom face
        }
    }

    private void AddFace(Vector3 blockPos, int faceIndex)
    {
        int start = verts.Count;
        int x = (int)blockPos.x;
        int y = (int)blockPos.y;
        int z = (int)blockPos.z;
        int blockID = blocks[x, y, z];
        if (blockID == 0) return;

        BlockClass block = blockManager.allBlocks[blockID];

        // --- Vertex positions ---
        for (int i = 0; i < 4; i++)
        {
            Vector3 vert = faceVerts[faceIndex, i];
            if (faceIndex != 4 && faceIndex != 5)
                vert.y *= block.height; // side face height
            else if (faceIndex == 4)
                vert.y = block.height; // top
            else
                vert.y = 0f; // bottom

            verts.Add(blockPos * cubeSize + vert * cubeSize);
        }

        // --- Triangles ---
        tris.Add(start + 2);
        tris.Add(start + 1);
        tris.Add(start);
        tris.Add(start);
        tris.Add(start + 3);
        tris.Add(start + 2);

        // --- UVs ---
        float tileSizeU = 1f / numTexs;
        float tileSizeV = 1f / 3f;

        float xOffset = blockID * tileSizeU;
        float yOffset = faceIndex switch
        {
            4 => tileSizeV * 1,
            5 => tileSizeV * 2,
            _ => 0f
        };

        float uvHeight = (block.isSlab && faceIndex < 4) ? tileSizeV * 0.5f : tileSizeV;

        uvs.Add(new Vector2(xOffset, yOffset));        // bottom-left
        uvs.Add(new Vector2(xOffset, yOffset + uvHeight)); // top-left
        uvs.Add(new Vector2(xOffset + tileSizeU, yOffset + uvHeight)); // top-right
        uvs.Add(new Vector2(xOffset + tileSizeU, yOffset)); // bottom-right
    }

    #endregion

    #region Apply Mesh

    public void ApplyMesh()
    {
        if (mesh == null)
            mesh = new Mesh();
        else
            mesh.Clear();

        if (verts.Count > 65000)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.MarkDynamic();

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;
    }

    #endregion
}
