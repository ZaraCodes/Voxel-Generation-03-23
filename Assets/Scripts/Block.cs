using System;
using UnityEngine;

[Serializable]
public class Block
{
    private EBlockType type = EBlockType.Stone;
    public EBlockType Type
    {
        get { return type; }
        set
        {
            type = value;
            switch (type)
            {
                case EBlockType.Stone:
                    RenderType = EBlockRenderType.Solid; break;
                case EBlockType.Air:
                    RenderType = EBlockRenderType.Air; break;
                case EBlockType.Dirt:
                    RenderType = EBlockRenderType.Solid; break;
                case EBlockType.Grass:
                    RenderType = EBlockRenderType.Solid; break;
                case EBlockType.WoodPlanks:
                    RenderType = EBlockRenderType.Solid; break;
                case EBlockType.Water:
                    RenderType = EBlockRenderType.Transparent; break;
                case EBlockType.WoodLog:
                    RenderType = EBlockRenderType.Solid; break;
                case EBlockType.Leafes:
                    RenderType = EBlockRenderType.Transparent2; break;
            }
        }
    }

    public Vector3 Position { get; set; }

    public EBlockRenderType RenderType = EBlockRenderType.Solid;
}

public enum EBlockType
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

public enum EBlockRenderType
{
    Solid,
    Transparent,
    Transparent2,
    Air
}