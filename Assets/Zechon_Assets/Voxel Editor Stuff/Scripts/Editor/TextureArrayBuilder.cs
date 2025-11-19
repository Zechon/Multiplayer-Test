#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlTypes;

public class TextureArrayBuilder
{
    [MenuItem("Voxel/Build Texture Arrays")]
    public static void BuildTextureArraysMenu()
    {
        string[] guids = AssetDatabase.FindAssets("t:BlockDatabase");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("TextureArrayBuilder", "No BlockDatabase asset found. Create one first and populate it with BlockData.", "Ok");
            return;
        }

        string dbPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        BlockDatabase db = AssetDatabase.LoadAssetAtPath<BlockDatabase>(dbPath);

        if (db == null || db.blocks == null || db.blocks.Length ==0)
        {
            EditorUtility.DisplayDialog("TextureArrayBuilder", "BlockDatabase is empty.", "Ok");
            return;
        }

        int texSize = EditorPrefs.GetInt("voxel_tex_size", 256);
        string input = EditorUtility.DisplayDialogComplex("Texture Array Size", "Choose texture size for the Texture2DArray. \nTextures will be resized to this size.", "16", "32", "Cancel") switch
        {
            0 => "16",
            1 => "32",
            _ => null
        };

        if (input == null) return;
        texSize = int.Parse(input);
        EditorPrefs.SetInt("voxel_tex_size", texSize);

        BuildFromDatabase(db, texSize);
    }

    public static void BuildFromDatabase(BlockDatabase db, int size = 32)
    {
        List<Texture2D> colorList = new List<Texture2D>();
        List<Texture2D> normalList = new List<Texture2D>();

        System.Func<Texture2D, List<Texture2D>, int> getOrAdd = (tex, list) =>
        {
            if (tex == null) return -1;
            int idx = list.IndexOf(tex);
            if (idx >= 0) return idx;
            list.Add(tex);
            return list.Count - 1;
        };

        foreach (var b in db.blocks)
        {
            if (b == null) continue;
            getOrAdd(b.topTexture, colorList);
            getOrAdd(b.bottomTexture, colorList);
            getOrAdd(b.sideTexture, colorList);
            getOrAdd(b.topNormal, normalList);
            getOrAdd(b.bottomNormal, normalList);
            getOrAdd(b.sideNormal, normalList);
        }

        if (colorList.Count == 0)
        {
            EditorUtility.DisplayDialog("TextureArrayBuilder", "No color textures found in BlockDatabase.", "Ok");
            return;
        }

        Texture2DArray colorArray = CreateTexture2DArray(colorList, size, true, "Voxel_ColorArray");
        Texture2DArray normalArray = CreateTexture2DArray(normalList, size, true, "Voxel_NormalArray");

        string folder = "Assets/Voxel/Generated";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Voxel", "Generated");

        string colorPath = $"{folder}/voxel_colorArray_{size}.asset";
        string normalPath = $"{folder}/voxel_normalArray_{size}.asset";

        AssetDatabase.CreateAsset(colorArray, colorPath);
        if (normalArray != null) AssetDatabase.CreateAsset(normalArray, normalPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (var b in db.blocks)
        {
            if (b == null) continue;

            b.topTexIndex = IndexOfTextureInList(b.topTexture, colorList);
            b.bottomTexIndex = IndexOfTextureInList(b.bottomTexture, colorList);
            b.sideTexIndex = IndexOfTextureInList(b.sideTexture, colorList);

            b.topNormalIndex = IndexOfTextureInList(b.topNormal, normalList);
            b.bottomNormalIndex = IndexOfTextureInList(b.bottomNormal, normalList);
            b.sideNormalIndex = IndexOfTextureInList(b.sideNormal, normalList);

            EditorUtility.SetDirty(b);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string matPath = $"{folder}/voxel_textureArrat_mat.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Shader shader = Shader.Find("Voxel/TextureArrayLit");
            if (shader == null)
            {
                EditorUtility.DisplayDialog("TextureArrayBuilder", "Please add the shader 'Voxel/TextureArrayLit' to the project (provided in instructions).", "OK");
            }
            mat = new Material(Shader.Find("Voxel/TextureArrayLit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }

        mat.SetTexture("_MainTexArray", colorArray);
        if (normalArray != null)
            mat.SetTexture("_NormalTexArray", normalArray);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("TextureArrayBuilder", $"Built color array ({colorList.Count}) and normal array ({normalList.Count}).\nAssets written to {folder}", "OK");
    }

    static int IndexOfTextureInList(Texture2D t, List<Texture2D> list)
    {
        if (t == null) return -1;
        return list.IndexOf(t);
    }

    static Texture2DArray CreateTexture2DArray(List<Texture2D> sources, int size, bool generateMips, string niceName)
    {
        if (sources == null || sources.Count == 0) return null;

        Texture2DArray arr = new Texture2DArray(size, size, sources.Count, TextureFormat.RGBA32, generateMips, false);
        arr.name = niceName;

        for (int i = 0; i < sources.Count; i++)
        {
            Texture2D src = sources[i];
            Texture2D resized = ResizeTextureTo(src, size);
            Color[] pixels = resized.GetPixels();
            arr.SetPixels(pixels, i);
        }
        arr.Apply(generateMips, false);
        return arr;
    }

    static Texture2D ResizeTextureTo(Texture2D src, int size)
    {
        if (src == null)
        {
            Texture2D clearTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            Color[] arr = new Color[size * size];
            for (int i = 0; i < arr.Length; i++) arr[i] = clear;
            clearTex.SetPixels(arr);
            clearTex.Apply();
            return clearTex;
        }

        RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return tex;
    }
}
#endif