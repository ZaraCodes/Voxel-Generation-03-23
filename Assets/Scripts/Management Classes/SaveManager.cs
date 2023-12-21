using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static void SaveChunk(Chunk chunk)
    {
        var path = $"{Application.persistentDataPath}/{ChunkManager.Instance.Generator.threadedChunkBuilder.Seed}";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        using (var stream = File.Open($"{path}/{chunk.name.Replace('/', ',')}.chunk", FileMode.Create))
        {
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, false))
            {
                var sameBlockCount = 0;
                var currentType = EBlockType.Air;

                string state = string.Empty;
                state += $"pos={chunk.gameObject.transform.position}\n";
                state += $"width={ChunkManager.Instance.Width}\n";
                state += $"height={ChunkManager.Instance.chunkHeight}\n";
                state += $"blocks=";
                foreach (var block in chunk.subChunks)
                {
                    if (currentType == block) sameBlockCount++;
                    else
                    {
                        state += $"{sameBlockCount}x{(int)currentType};";
                        sameBlockCount = 1;
                        currentType = block;
                    }
                }
                state += $"{sameBlockCount}x{(int)currentType};";
                if (state.StartsWith("0x0;")) state = state.Remove(0, 4);
                writer.Write(state);
            }
        }
    }

    public static bool DoesChunkExist(Vector2Int chunkPos)
    {
        return File.Exists($"{Application.persistentDataPath}/{ChunkManager.Instance.Generator.threadedChunkBuilder.Seed}/{chunkPos.x},{chunkPos.y}.chunk");
    } 

    public static EBlockType[,,,] LoadChunk()
    {
        throw new NotImplementedException();
    }
}
