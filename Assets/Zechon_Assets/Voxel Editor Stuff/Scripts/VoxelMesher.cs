using System.Collections.Generic;
using UnityEngine;

public static class VoxelMesher
{
    public static void GenerateMesh(VoxelChunk chunk, MeshFilter meshFilter, MeshCollider meshCollider, MeshRenderer meshRenderer, Material defaultMaterial, BlockDatabase blockDatabase)
    {
        // Assign material if none exists
        if (meshRenderer.sharedMaterial == null && defaultMaterial != null)
            meshRenderer.sharedMaterial = defaultMaterial;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector2> uv2 = new List<Vector2>();

        for (int x = 0; x < VoxelChunk.chSize; x++)
        {
            for (int y = 0; y < VoxelChunk.chSize; y++)
            {
                for (int z = 0; z < VoxelChunk.chSize; z++)
                {
                    int id = chunk.blocks[x, y, z];
                    if (id == 0) continue;

                    BlockData block = blockDatabase.GetBlock(id);
                    if (block == null) continue;

                    TryAddFace(Direction.Up, Face.Top, x, y, z, chunk, block, vertices, triangles, uv, uv2);
                    TryAddFace(Direction.Down, Face.Bottom, x, y, z, chunk, block, vertices, triangles, uv, uv2);
                    TryAddFace(Direction.North, Face.Side, x, y, z, chunk, block, vertices, triangles, uv, uv2);
                    TryAddFace(Direction.South, Face.Side, x, y, z, chunk, block, vertices, triangles, uv, uv2);
                    TryAddFace(Direction.East, Face.Side, x, y, z, chunk, block, vertices, triangles, uv, uv2);
                    TryAddFace(Direction.West, Face.Side, x, y, z, chunk, block, vertices, triangles, uv, uv2);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uv);
        mesh.SetUVs(1, uv2);

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

    static void TryAddFace(
        Direction dir,
        Face face,
        int x, int y, int z,
        VoxelChunk chunk,
        BlockData block,
        List<Vector3> verts,
        List<int> tris,
        List<Vector2> uv,
        List<Vector2> uv2)
    {
        // neighbor check
        Vector3Int n = new Vector3Int(x, y, z) + Vector3Int.RoundToInt(faceDirections[(int)dir]);

        bool neighborInside =
            n.x >= 0 && n.x < VoxelChunk.chSize &&
            n.y >= 0 && n.y < VoxelChunk.chSize &&
            n.z >= 0 && n.z < VoxelChunk.chSize;

        if (neighborInside && chunk.blocks[n.x, n.y, n.z] != 0) return;

        int vertStart = verts.Count;

        // add vertices
        for (int i = 0; i < 4; i++)
            verts.Add(new Vector3(x, y, z) + faceVertices[(int)dir, i]);

        // triangles
        tris.Add(vertStart + 0);
        tris.Add(vertStart + 1);
        tris.Add(vertStart + 2);
        tris.Add(vertStart + 0);
        tris.Add(vertStart + 2);
        tris.Add(vertStart + 3);

        // Standard quad UVs
        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(1, 0));
        uv.Add(new Vector2(1, 1));
        uv.Add(new Vector2(0, 1));

        // Texture array index goes in UV2.x
        int texIndex = block.GetTextureIndex(face);
        uv2.Add(new Vector2(texIndex, 0));
        uv2.Add(new Vector2(texIndex, 0));
        uv2.Add(new Vector2(texIndex, 0));
        uv2.Add(new Vector2(texIndex, 0));
    }
}
