using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVSetter : MonoBehaviour
{
    public static Vector2[] GetUVs(BlockType blockType)
    {
        Vector2[] uvs = new Vector2[4];
        switch (blockType)
        {
            case BlockType.Stone:
                uvs[0] = tileCoordinates[BlockType.Stone] + new Vector2(.002f, .998f) / 32f;
                uvs[1] = tileCoordinates[BlockType.Stone] + new Vector2(.002f, .002f) / 32f;
                uvs[2] = tileCoordinates[BlockType.Stone] + new Vector2(.998f, .002f) / 32f;
                uvs[3] = tileCoordinates[BlockType.Stone] + new Vector2(.998f, .998f) / 32f;
                break;
        }
        return uvs;
    }

    private static Dictionary<BlockType, Vector2> tileCoordinates = new Dictionary<BlockType, Vector2>()
    {
        { BlockType.Stone, new Vector2(0, 0) / 32}
    };
}
