using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVSetter : MonoBehaviour
{
    public static Vector2[] GetUVs(BlockType blockType, BlockFace blockFace)
    {
        switch (blockType)
        {
            case BlockType.Grass:
                return GetTopOrientedBlockUVs(blockFace, TileType.Grass, TileType.Dirt, TileType.GrassSide);
            case BlockType.Dirt:
                return SetUVs(TileType.Dirt);
            case BlockType.Stone:
                return SetUVs(TileType.Stone);
        }
        return new Vector2[0];
    }

    private static Vector2[] GetTopOrientedBlockUVs(BlockFace blockFace, TileType topTile, TileType bottomTile, TileType sidesTile)
    {
        switch (blockFace)
        {
            case BlockFace.Yup:
                return SetUVs(topTile);
            case BlockFace.Ydown:
                return SetUVs(bottomTile);
            default:
                return SetUVs(sidesTile);
        }
    }

    private static Vector2[] SetUVs(TileType tileType)
    {
        Vector2[] uvs = new Vector2[4];
        uvs[0] = tileCoordinates[tileType] + new Vector2(.003f, .997f) / 32f;
        uvs[1] = tileCoordinates[tileType] + new Vector2(.003f, .003f) / 32f;
        uvs[2] = tileCoordinates[tileType] + new Vector2(.997f, .003f) / 32f;
        uvs[3] = tileCoordinates[tileType] + new Vector2(.997f, .997f) / 32f;
        return uvs;
    }


    private static Dictionary<TileType, Vector2> tileCoordinates = new Dictionary<TileType, Vector2>()
    {
        { TileType.Stone, new Vector2(0, 0) / 32 },
        { TileType.Grass, new Vector2(1, 0) / 32 },
        { TileType.Dirt, new Vector2(2, 0) / 32 },
        { TileType.GrassSide, new Vector2(3, 0) / 32 },
    };
}

public enum TileType
{
    Stone,
    Dirt,
    Grass,
    GrassSide
}