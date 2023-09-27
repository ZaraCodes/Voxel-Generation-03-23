using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    private BlockType type = BlockType.Stone;
    public BlockType Type
    {
        get { return type; }
        set {
            type = value;
            switch (type)
            {
                case BlockType.Stone:
                    RenderType = BlockRenderType.Solid; break;
                case BlockType.Air:
                    RenderType = BlockRenderType.Air; break;
                case BlockType.Dirt:
                    RenderType = BlockRenderType.Solid; break;
                case BlockType.Grass:
                    RenderType = BlockRenderType.Solid; break;
                case BlockType.WoodPlanks:
                    RenderType = BlockRenderType.Solid; break;
                case BlockType.Water:
                    RenderType = BlockRenderType.Transparent; break;
                case BlockType.WoodLog:
                    RenderType = BlockRenderType.Solid; break;
                case BlockType.Leafes:
                    RenderType = BlockRenderType.Transparent2; break;
            }
        }
    }

    public Vector3 position = Vector3.zero;

    public BlockRenderType RenderType = BlockRenderType.Solid;


}

public enum BlockType
{
    Air,
    Stone,
    Water,
    Dirt,
    Grass,
    WoodPlanks,
    WoodLog,
    Leafes
}

public enum BlockRenderType
{
    Solid,
    Transparent,
    Transparent2,
    Air
}