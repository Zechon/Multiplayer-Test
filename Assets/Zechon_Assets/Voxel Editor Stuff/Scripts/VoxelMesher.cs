using System.Collections.Generic;
using UnityEngine;

public static class VoxelMesher
{
    public static void GenerateMesh(
        VoxelChunk chunk,
        MeshFilter meshFilter,
        MeshCollider meshCollider,
        MeshRenderer meshRenderer,
        Material defaultMaterial,
        BlockDatabase blockDatabase)
    {
        // Assign material if none exists
        if (meshRenderer.sharedMaterial == null && defaultMaterial != null)
            meshRenderer.sharedMaterial = defaultMaterial;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int faceCount = 0;

        for (int x = 0; x < VoxelChunk.chSize; x++)
        {
            for (int y = 0; y < VoxelChunk.chSize; y++)
            {
                for (int z = 0; z < VoxelChunk.chSize; z++)
                {
                    int blockID = chunk.blocks[x, y, z];
                    if (blockID == 0) continue;

                    BlockData blockData = blockDatabase.GetBlock(blockID);
                    if (blockData == null) continue;

                    // Generate each face IF exposed
                    TryAddFace(Direction.Up, x, y, z, chunk, vertices, triangles, uvs, ref faceCount, blockData, Face.Top);
                    TryAddFace(Direction.Down, x, y, z, chunk, vertices, triangles, uvs, ref faceCount, blockData, Face.Bottom);
                    TryAddFace(Direction.North, x, y, z, chunk, vertices, triangles, uvs, ref faceCount, blockData, Face.Side);
                    TryAddFace(Direction.South, x, y, z, chunk, vertices, triangles, uvs, ref faceCount, blockData, Face.Side);
                    TryAddFace(Direction.East, x, y, z, chunk, vertices, triangles, uvs, ref faceCount, blockData, Face.Side);
                    TryAddFace(Direction.West, x, y, z, chunk, vertices, triangles, uvs, ref faceCount, blockData, Face.Side);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    enum Direction { Up, Down, North, South, East, West }

    static readonly Vector3[] faceDirections = {
        Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left
    };

    static readonly Vector3[,] faceVertices = {
        // Up
        { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0) },
        // Down
        { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) },
        // North
        { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) },
        // South
        { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) },
        // East
        { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) },
        // West
        { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0) }
    };

    static void TryAddFace(Direction dir, int x, int y, int z, VoxelChunk chunk, List<Vector3> verts, List<int> tris, List<Vector2> uvs, ref int faceCount, BlockData blockData, Face faceType)
    {
        Vector3Int neighbor = new Vector3Int(x, y, z) + Vector3Int.RoundToInt(faceDirections[(int)dir]);

        bool neighborInside =
            neighbor.x >= 0 && neighbor.x < VoxelChunk.chSize &&
            neighbor.y >= 0 && neighbor.y < VoxelChunk.chSize &&
            neighbor.z >= 0 && neighbor.z < VoxelChunk.chSize;

        // Only draw face if neighbor is air or outside chunk
        if (neighborInside && chunk.blocks[neighbor.x, neighbor.y, neighbor.z] != 0)
            return;

        int vertStart = verts.Count;

        // Add vertices for this face
        for (int i = 0; i < 4; i++)
            verts.Add(new Vector3(x, y, z) + faceVertices[(int)dir, i]);

        // Two triangles per face
        tris.Add(vertStart + 0);
        tris.Add(vertStart + 1);
        tris.Add(vertStart + 2);
        tris.Add(vertStart + 0);
        tris.Add(vertStart + 2);
        tris.Add(vertStart + 3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));

        faceCount++;
    }
}
