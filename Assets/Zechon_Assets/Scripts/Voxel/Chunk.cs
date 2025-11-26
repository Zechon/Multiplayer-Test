using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mesh;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public int[,,] blocks = new int[16, 16, 16];

    public Mesh mesh;
    public Vector3Int chunkCoord;
    public float cubeSize = 0.5f;
    public int numTexs;

    [Header("References")]
    private ChunkMeshData meshData;
    public ChunkMetadata metadata;
    public BlockManager blockManager;

    private void Awake()
    {
        meshData = new ChunkMeshData(16);
    }

    #region Mesh Generation

    public void GenerateChunkMesh()
    {
        meshData.Clear();

        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                for (int z = 0; z < 16; z++)
                    if (blocks[x, y, z] != 0)
                        AddVoxelFaces(x, y, z, meshData);
    }

    private void AddVoxelFaces(int x, int y, int z, ChunkMeshData meshData)
    {
        Vector3 blockPos = new Vector3(x, y, z);

        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            Vector3Int neighborPos = new Vector3Int(x, y, z) + Vector3Int.RoundToInt(ChunkMeshData.faceDirs[faceIndex]);
            if (ShouldRenderFace(neighborPos, faceIndex))
            {
                AddFace(blockPos, faceIndex, meshData);
            }
        }
    }

    private bool ShouldRenderFace(Vector3Int neighborPos, int faceIndex)
    {
        int neighborID = VoxelWorld.Instance.GetBlock(
            chunkCoord.x * 16 + neighborPos.x,
            chunkCoord.y * 16 + neighborPos.y,
            chunkCoord.z * 16 + neighborPos.z
        );

        if (neighborID == 0) return true;

        BlockClass neighbor = blockManager.allBlocks[neighborID];
        if (neighbor == null) return true;

        if (faceIndex == 4) return neighbor.height < 1f;
        if (faceIndex == 5) return neighbor.height < 1f;

        return neighbor.height < 1f;
    }

    private void AddFace(Vector3 blockPos, int faceIndex, ChunkMeshData meshData)
    {
        int start = meshData.verts.Count;
        int x = (int)blockPos.x;
        int y = (int)blockPos.y;
        int z = (int)blockPos.z;
        int blockID = blocks[x, y, z];
        if (blockID == 0) return;

        BlockClass block = blockManager.allBlocks[blockID];

        for (int i = 0; i < 4; i++)
        {
            Vector3 vert = ChunkMeshData.faceVerts[faceIndex, i];
            if (faceIndex != 4 && faceIndex != 5)
                vert.y *= block.height;
            else if (faceIndex == 4)
                vert.y = block.height;
            else
                vert.y = 0f;

            meshData.verts.Add(blockPos * cubeSize + vert * cubeSize);
        }

        meshData.tris.Add(start + 2);
        meshData.tris.Add(start + 1);
        meshData.tris.Add(start);
        meshData.tris.Add(start);
        meshData.tris.Add(start + 3);
        meshData.tris.Add(start + 2);

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

        meshData.uvs.Add(new Vector2(xOffset, yOffset));        // bottom-left
        meshData.uvs.Add(new Vector2(xOffset, yOffset + uvHeight)); // top-left
        meshData.uvs.Add(new Vector2(xOffset + tileSizeU, yOffset + uvHeight)); // top-right
        meshData.uvs.Add(new Vector2(xOffset + tileSizeU, yOffset)); // bottom-right
    }

    #endregion

    #region Apply Mesh

    public void ApplyMesh()
    {
        if (mesh == null)
            mesh = new Mesh();
        else
            mesh.Clear();

        if (meshData.verts.Count > 65000)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(meshData.verts);
        mesh.SetTriangles(meshData.tris, 0);
        mesh.SetUVs(0, meshData.uvs);
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
