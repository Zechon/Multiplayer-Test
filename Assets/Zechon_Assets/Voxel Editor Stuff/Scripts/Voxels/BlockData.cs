using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockData", menuName = "Voxel/Block Data")]
public class BlockData : ScriptableObject
{
    public string blockName;

    [Header("Textures")]
    public Texture2D topTexture;
    public Texture2D bottomTexture;
    public Texture2D sideTexture;

    [Header("Normal Maps (optional)")]
    public Texture2D topNormal;
    public Texture2D bottomNormal;
    public Texture2D sideNormal;

    public bool isSolid = true;

    public Texture2D GetTextureForFace(Face face)
    {
        switch (face)
        {
            case Face.Top: return topTexture;
            case Face.Bottom: return bottomTexture;
            default: return sideTexture;
        }
    }

    public Texture2D GetNormalForFace(Face face)
    {
        switch (face)
        {
            case Face.Top: return topNormal;
            case Face.Bottom: return bottomNormal;
            default: return sideNormal;
        }
    }
}

public enum Face { Top, Bottom, Side }
