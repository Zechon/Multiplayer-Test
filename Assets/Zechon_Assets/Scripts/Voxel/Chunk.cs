using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public const int SIZE = 16;
    public int[,,] blocks = new int[SIZE, SIZE, SIZE];

    public Mesh mesh;
    public Vector3Int chunkCoord;
    public float cubeSize = 0.5f;
    public int numTexs;

    [Header("References")]
    public ChunkMetadata metadata;
    public BlockManager blockManager;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    private ChunkMeshData meshData;

    private void Awake()
    {
        meshData = new ChunkMeshData(16);
    }

    #region Mesh Generation

    public void GenerateChunkMesh()
    {
        meshData.Clear();

        meshData.EnsureCapacity(Chunk.SIZE * Chunk.SIZE * Chunk.SIZE * 6 * 4,Chunk.SIZE * Chunk.SIZE * Chunk.SIZE * 6 * 6, Chunk.SIZE * Chunk.SIZE * Chunk.SIZE * 6 * 4);

        for (int x = 0; x < Chunk.SIZE; x++)
            for (int y = 0; y < Chunk.SIZE; y++)
                for (int z = 0; z < Chunk.SIZE; z++) 
                {
                    int BlockID = blocks[x, y, z];
                    if (BlockID == 0) continue;

                    Vector3Int globalPos = chunkCoord * Chunk.SIZE + new Vector3Int(x, y, z);
                    AddVoxelFaces(globalPos, meshData);
                }
    }

    private void AddVoxelFaces(Vector3Int globalPos, ChunkMeshData meshData)
    {
        int blockID = VoxelWorld.Instance.GetBlock(globalPos.x, globalPos.y, globalPos.z);
        if (blockID == 0) return;

        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
            Vector3Int neighborPos = globalPos + Vector3Int.RoundToInt(ChunkMeshData.faceDirs[faceIndex]);
            if (ShouldRenderFace(globalPos, faceIndex))
            {
                AddFace(globalPos, faceIndex, meshData);
            }
        }
    }

    private bool ShouldRenderFace(Vector3Int globalPos, int faceIndex)
    {
        Vector3Int neighborPos = globalPos + Vector3Int.RoundToInt(ChunkMeshData.faceDirs[faceIndex]);
        int neighborID = VoxelWorld.Instance.GetBlock(neighborPos.x, neighborPos.y, neighborPos.z);

        if (neighborID == 0) return true;

        BlockClass neighbor = blockManager.allBlocks[neighborID];
        int selfID = VoxelWorld.Instance.GetBlock(globalPos.x, globalPos.y, globalPos.z);
        BlockClass self = blockManager.allBlocks[selfID];

        if (neighbor == null || self == null) return true;

        if (faceIndex == 4) // top
            return self.height < 1f || neighbor.height < 1f;
        if (faceIndex == 5) // bottom
            return self.height < 1f || neighbor.height < 1f;

        return self.height < 1f || neighbor.height < 1f;
    }

    private void AddFace(Vector3 globalPos, int faceIndex, ChunkMeshData meshData)
    {
        int blockID = VoxelWorld.Instance.GetBlock((int)globalPos.x, (int)globalPos.y, (int)globalPos.z);
        if (blockID == 0) return;

        BlockClass block = blockManager.allBlocks[blockID];

        // Determine face height for vertex positioning
        float faceHeight = 0f;
        if (faceIndex < 4) faceHeight = block.height;   // side faces
        else if (faceIndex == 4) faceHeight = block.height; // top
        else faceHeight = 0f; // bottom

        int startIndex = meshData.verts.Count;

        // Add vertices
        for (int i = 0; i < 4; i++)
        {
            Vector3 vert = ChunkMeshData.faceVerts[faceIndex, i];

            // Adjust Y for side faces based on block height
            if (faceIndex < 4) vert.y *= faceHeight;
            else vert.y = faceIndex == 4 ? faceHeight : 0f;

            meshData.verts.Add(globalPos * cubeSize + vert * cubeSize);
        }

        // Add triangles
        meshData.tris.Add(startIndex + 2);
        meshData.tris.Add(startIndex + 1);
        meshData.tris.Add(startIndex);
        meshData.tris.Add(startIndex);
        meshData.tris.Add(startIndex + 3);
        meshData.tris.Add(startIndex + 2);

        // Calculate UVs
        float tileSizeU = 1f / numTexs;
        float tileSizeV = 1f / 3f;

        float xOffset = blockID * tileSizeU;
        float yOffset = faceIndex switch
        {
            4 => tileSizeV * 1,  // top
            5 => tileSizeV * 2,  // bottom
            _ => 0f              // sides
        };

        float uvHeight = (block.isSlab && faceIndex < 4) ? tileSizeV * 0.5f : tileSizeV;

        meshData.uvs.Add(new Vector2(xOffset, yOffset));           // bottom-left
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

        mesh.SetVertexBufferParams(meshData.verts.Count,
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        );

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
