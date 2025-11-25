using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class ChunkMetadata
{
    public string worldName = "DefaultWorld";
    public string author = "";
    public long timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public string notes = "";
    public int saveToolVersion = 1;
}

[System.Serializable]
public class ChunkData
{
    public Vector3Int coord;
    public int[,,] blocks;
}

[Serializable]
public class WorldManifest
{
    public string worldName;
    public int formatVersion = 1;
    public int saveToolVersion = 1;
    public List<Vector3Int> chunkCoordinates = new();
    public long lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}