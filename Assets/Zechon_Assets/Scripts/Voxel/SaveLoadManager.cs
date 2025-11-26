using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveLoadManager
{
    const string MAGIC = "VXL1";
    const byte FORMAT_VERSION = 1;

    const byte FLAG_COMPRESSED_GZIP = 1 << 0;

    public static bool UseGzipCompression = true;

    public static string GetWorldBasePath(string worldName)
    {
        #if UNITY_EDITOR
        string path = Path.Combine(Application.dataPath, "../Worlds", worldName);
        #else
        // In builds, save next to the exe
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string path = Path.Combine(exeDir, "Worlds", worldName);
        #endif
        Directory.CreateDirectory(path);
        return path;
    }

    public static string GetChunkFilePath(string worldName, Vector3Int coord)
    {
        string worldPath = GetWorldBasePath(worldName);
        return Path.Combine(worldPath, $"chunk_{coord.x}_{coord.y}_{coord.z}.vox");
    }

    public static void SaveChunk(string worldName, Chunk chunk, ChunkMetadata meta = null)
    {
        string file = GetChunkFilePath(worldName, chunk.chunkCoord);
        meta ??= new ChunkMetadata { worldName = worldName };

        using var fs = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None);
        using var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);

        bw.Write(Encoding.ASCII.GetBytes(MAGIC));
        bw.Write(FORMAT_VERSION);

        byte flags = UseGzipCompression ? FLAG_COMPRESSED_GZIP : (byte)0;
        bw.Write(flags);

        string json = JsonUtility.ToJson(meta);
        byte[] metaBytes = Encoding.UTF8.GetBytes(json);
        bw.Write(metaBytes.Length);
        bw.Write(metaBytes);

        int cs = 16;

        using var payloadMs = new MemoryStream();
        using (var payloadWriter = new BinaryWriter(payloadMs, Encoding.UTF8, leaveOpen: true))
        {
            for (int x = 0; x < cs; x++)
                for (int y = 0; y < cs; y++)
                    for (int z = 0; z < cs; z++)
                        payloadWriter.Write(chunk.blocks[x, y, z]);
        }

        payloadMs.Position = 0;
        if ((flags & FLAG_COMPRESSED_GZIP) != 0)
        {
            using var compressMs = new MemoryStream();
            using (var gzip = new GZipStream(compressMs, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
            {
                payloadMs.CopyTo(gzip);
            }
            byte[] compressed = compressMs.ToArray();
            bw.Write(compressed.Length);
            bw.Write(compressed);
        }
        else
        {
            byte[] raw = payloadMs.ToArray();
            bw.Write(raw.Length);
            bw.Write(raw);
        }

        Debug.Log(Application.persistentDataPath);
    }

    public static (ChunkMetadata meta, int[,,] blocks)? LoadChunk(string worldName, Vector3Int coord)
    {
        string file = GetChunkFilePath(worldName, coord);
        if (!File.Exists(file)) return null;

        int cs = 16;
        int[,,] blocks = new int[cs, cs, cs];

        using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

        // Read header
        byte[] magicBytes = br.ReadBytes(4);
        string magic = Encoding.ASCII.GetString(magicBytes);

        if (magic != MAGIC)
        {
            // Legacy chunk
            Debug.Log("Legacy chunk detected, using raw block data.");
            bool success = TryReadLegacyChunk(fs, cs, ref blocks);
            if (!success)
                return null;

            var defaultMeta = new ChunkMetadata
            {
                worldName = worldName,
                saveToolVersion = 0,
                timestampUtc = 0
            };

            return (defaultMeta, blocks);
        }

        // Modern chunk format
        byte fileVersion = br.ReadByte();
        byte flags = br.ReadByte();

        int metaLen = br.ReadInt32();
        string json = Encoding.UTF8.GetString(br.ReadBytes(metaLen));
        var meta = JsonUtility.FromJson<ChunkMetadata>(json);

        int payloadLen = br.ReadInt32();
        byte[] payload = br.ReadBytes(payloadLen);

        byte[] rawBlockBytes = ((flags & FLAG_COMPRESSED_GZIP) != 0)
            ? DecompressGzip(payload)
            : payload;

        int expectedBytes = cs * cs * cs * sizeof(int);
        if (rawBlockBytes.Length != expectedBytes)
            Debug.LogWarning($"VoxelSaveSystem: block payload size mismatch (expected {expectedBytes}, got {rawBlockBytes.Length})");

        ReadBlocksFromBytes(rawBlockBytes, cs, ref blocks);

        return (meta, blocks);
    }

    private static bool TryReadLegacyChunk(Stream fs, int cs, ref int[,,] blocks)
    {
        try
        {
            fs.Position = 0;
            using var brLegacy = new BinaryReader(fs);
            for (int x = 0; x < cs; x++)
                for (int y = 0; y < cs; y++)
                    for (int z = 0; z < cs; z++)
                        blocks[x, y, z] = brLegacy.ReadInt32();

            return true;
        }
        catch
        {
            Debug.LogWarning("Failed to read legacy chunk.");
            return false;
        }
    }

    private static byte[] DecompressGzip(byte[] compressed)
    {
        using var inMs = new MemoryStream(compressed);
        using var outMs = new MemoryStream();
        using (var gz = new GZipStream(inMs, CompressionMode.Decompress))
            gz.CopyTo(outMs);
        return outMs.ToArray();
    }

    private static void ReadBlocksFromBytes(byte[] rawBlockBytes, int cs, ref int[,,] blocks)
    {
        using var ms = new MemoryStream(rawBlockBytes);
        using var br = new BinaryReader(ms);
        for (int x = 0; x < cs; x++)
            for (int y = 0; y < cs; y++)
                for (int z = 0; z < cs; z++)
                    blocks[x, y, z] = br.ReadInt32();
    }

    public static Task SaveChunkAsync(string worldName, Chunk chunk, ChunkMetadata meta = null)
    {
        return Task.Run(() => SaveChunk(worldName, chunk, meta));
    }

    public static Task<(ChunkMetadata meta, int[,,] blocks)?> LoadChunkAsync(string worldName, Vector3Int coord)
    {
        return Task.Run(() => LoadChunk(worldName, coord));
    }

    public static (ChunkMetadata meta, int[,,] blocks)? LoadChunkRaw(string filePath)
    {
        // This method is safe to run in a background thread
        int cs = 16;
        int[,,] blocks = new int[cs, cs, cs];

        using var fs = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        using var br = new System.IO.BinaryReader(fs, System.Text.Encoding.UTF8, leaveOpen: true);

        byte[] magicBytes = br.ReadBytes(4);
        string magic = System.Text.Encoding.ASCII.GetString(magicBytes);

        if (magic != "VXL1")
            return null; // legacy support can be added if needed

        byte fileVersion = br.ReadByte();
        byte flags = br.ReadByte();

        int metaLen = br.ReadInt32();
        string json = System.Text.Encoding.UTF8.GetString(br.ReadBytes(metaLen));
        var meta = JsonUtility.FromJson<ChunkMetadata>(json);

        int payloadLen = br.ReadInt32();
        byte[] payload = br.ReadBytes(payloadLen);

        byte[] rawBlockBytes = (flags & 1) != 0 ? DecompressGzip(payload) : payload;

        ReadBlocksFromBytes(rawBlockBytes, cs, ref blocks);

        return (meta, blocks);
    }

}
