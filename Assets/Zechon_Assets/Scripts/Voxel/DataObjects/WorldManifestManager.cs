using System;
using System.IO;
using UnityEngine;

public static class WorldManifestManager
{
    public static string GetManifestPath(string worldName)
    {
        string dir = Path.Combine(Application.persistentDataPath, "Worlds", worldName);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "manifest.json");
    }

    public static void SaveManifest(WorldManifest manifest)
    {
        string path = GetManifestPath(manifest.worldName);
        string json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(path, json);
    }

    public static WorldManifest LoadManifest(string worldName)
    {
        string path = GetManifestPath(worldName);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<WorldManifest>(json);
    }

    public static void AddChunk(WorldManifest manifest, Vector3Int coord)
    {
        if (!manifest.chunkCoordinates.Contains(coord))
            manifest.chunkCoordinates.Add(coord);
        manifest.lastSavedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
