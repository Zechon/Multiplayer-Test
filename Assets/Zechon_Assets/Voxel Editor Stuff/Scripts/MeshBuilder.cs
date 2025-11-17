using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder
{ 
    public List<Vector3> verts = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Color> cols = new List<Color>();

    public void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color)
    {
        int start = verts.Count;
        verts.Add(v0);
        verts.Add(v1);
        verts.Add(v2);
        verts.Add(v3);

        tris.Add(start + 0);
        tris.Add(start + 1);
        tris.Add(start + 2);

        tris.Add(start + 0);
        tris.Add(start + 2);
        tris.Add(start + 3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));

        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
        cols.Add(color);
    }
    public Mesh ToMesh()
    {
        Mesh m = new Mesh();
        m.indexFormat = verts.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.SetUVs(0, uvs);
        m.SetColors(cols);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    public void Clear()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        cols.Clear();
    }
}
