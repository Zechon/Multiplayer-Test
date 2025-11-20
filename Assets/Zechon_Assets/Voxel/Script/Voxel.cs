using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Voxel : MonoBehaviour
{
    public Texture2D texAtlas;
    public BlockClass blockClass;

    public Vector3 pos;

    public Mesh mesh;

    // Mesh data
    public List<Vector3> verts;
    public List<int> tris;
    public List<Vector2> uvs;

    public Vector3 cubeSize = Vector3.one * 0.5f;

    private static readonly Vector3[] faceDirs = {
        Vector3.forward, Vector3.back,
        Vector3.right, Vector3.left,
        Vector3.up, Vector3.down
    };

    private static readonly Vector3[,] faceVerts = {
        // Front
        {new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,0,1)},
        // Back
        {new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0), new Vector3(0,0,0)},
        // Right
        {new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(1,1,0), new Vector3(1,0,0)},
        // Left
        {new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(0,0,1)},
        // Top
        {new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,1,1)},
        // Bottom
        {new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,0,0)}
    };

    private void Start()
    {
        mesh = new Mesh();

        verts = new List<Vector3>(24);
        tris = new List<int>(36);
        uvs = new List<Vector2>(24);

        DrawTexAtlas();

        DrawBlock();

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.SetUVs(0, uvs.ToArray());
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void DrawTexAtlas()
    {
        texAtlas = new Texture2D(blockClass.blockFaceTextures[0].width * 6, blockClass.blockFaceTextures[0].width * 6);
    }

    void DrawBlock()
    {
        for (int i = 0; i < 6; i++)
        {
            AddFace(i);
        }
    }

    void AddFace(int faceIndex)
    {
        int lastVertex = verts.Count;

        // Add vertices
        for (int i = 0; i < 4; i++)
        {
            verts.Add(pos + Vector3.Scale(faceVerts[faceIndex, i], cubeSize));
        }

        // Add triangles (two per face)
        tris.Add(lastVertex + 2);
        tris.Add(lastVertex + 1);
        tris.Add(lastVertex);

        tris.Add(lastVertex);
        tris.Add(lastVertex + 3);
        tris.Add(lastVertex + 2);

        // Add placeholder UVs (all 0s for now)
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
        uvs.Add(Vector2.zero);
    }
}
