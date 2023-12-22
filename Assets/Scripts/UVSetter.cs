using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVSetter : MonoBehaviour
{
    private static float offset = 0.005f;
    public static Vector2[] GetUVs(EBlockType blockType, EBlockFace blockFace)
    {
        switch (blockType)
        {
            case EBlockType.Grass:
                return GetTopOrientedBlockUVs(blockFace, TileType.Grass, TileType.Dirt, TileType.GrassSide);
            case EBlockType.Dirt:
                return SetUVs(TileType.Dirt);
            case EBlockType.Stone:
                return SetUVs(TileType.Stone);
            case EBlockType.WoodPlanks:
                return SetUVs(TileType.WoodPlanks);
            case EBlockType.Water:
                return SetUVs(TileType.Water);
            case EBlockType.WoodLog:
                return GetTopOrientedBlockUVs(blockFace, TileType.WoodLogTop, TileType.WoodLogTop, TileType.WoodLogSide);
            case EBlockType.Leafes:
                return SetUVs(TileType.Leaves);
            case EBlockType.Bedrock:
                return SetUVs(TileType.Bedrock);
        }
        return new Vector2[0];
    }

    private static Vector2[] GetTopOrientedBlockUVs(EBlockFace blockFace, TileType topTile, TileType bottomTile, TileType sidesTile)
    {
        switch (blockFace)
        {
            case EBlockFace.Yup:
                return SetUVs(topTile);
            case EBlockFace.Ydown:
                return SetUVs(bottomTile);
            default:
                return SetUVs(sidesTile);
        }
    }

    private static Vector2[] SetUVs(TileType tileType)
    {
        Vector2[] uvs = new Vector2[4];
        uvs[0] = tileCoordinates[tileType] + new Vector2(0f + offset, 1f - offset) / 32f;
        uvs[1] = tileCoordinates[tileType] + new Vector2(0f + offset, 0f + offset) / 32f;
        uvs[2] = tileCoordinates[tileType] + new Vector2(1f - offset, 0f + offset) / 32f;
        uvs[3] = tileCoordinates[tileType] + new Vector2(1f - offset, 1f - offset) / 32f;
        return uvs;
    }


    private static Dictionary<TileType, Vector2> tileCoordinates = new()
    {
        { TileType.Stone, new Vector2(0, 0) / 32 },
        { TileType.Grass, new Vector2(1, 0) / 32 },
        { TileType.Dirt, new Vector2(2, 0) / 32 },
        { TileType.GrassSide, new Vector2(3, 0) / 32 },
        { TileType.WoodPlanks, new Vector2(4, 0) / 32 },
        { TileType.Water, new Vector2(5, 0) / 32 },
        { TileType.Bedrock, new Vector2(5, 0) / 32 },
        { TileType.WoodLogSide, new Vector2(6, 0) / 32 },
        { TileType.WoodLogTop, new Vector2(7, 0) / 32 },
        { TileType.Leaves, new Vector2(8, 0) / 32 }
    };
}

public enum TileType
{
    Stone,
    Water,
    Dirt,
    Grass,
    GrassSide,
    WoodPlanks,
    WoodLogSide,
    WoodLogTop,
    Leaves,
    Bedrock,
}