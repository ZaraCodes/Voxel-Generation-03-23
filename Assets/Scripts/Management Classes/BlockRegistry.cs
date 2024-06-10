using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRegistry
{
    private Dictionary<EBlockType, EBlockRenderType> BlockRenderType;

    public BlockRegistry()
    {
        BlockRenderType = new();
        AddBlock(EBlockType.Air, EBlockRenderType.Air);
        AddBlock(EBlockType.Stone, EBlockRenderType.Solid);
        AddBlock(EBlockType.Water, EBlockRenderType.Water);
        AddBlock(EBlockType.Dirt, EBlockRenderType.Solid);
        AddBlock(EBlockType.Grass, EBlockRenderType.Solid);
        AddBlock(EBlockType.WoodPlanks, EBlockRenderType.Solid);
        AddBlock(EBlockType.WoodLog, EBlockRenderType.Solid);
        AddBlock(EBlockType.Leafes, EBlockRenderType.Transparent);
        AddBlock(EBlockType.Bedrock, EBlockRenderType.Solid);
    }

    public void AddBlock(EBlockType b, EBlockRenderType t)
    {
        BlockRenderType.Add(b, t);
    }

    public EBlockRenderType GetRenderType(EBlockType b)
    {
        return BlockRenderType[b];
    }
}
