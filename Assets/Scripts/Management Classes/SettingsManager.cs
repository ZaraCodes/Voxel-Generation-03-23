using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Manages the settings that are used in-game</summary>
public class SettingsManager
{
    private static SettingsManager instance;

    public static SettingsManager Instance
    {
        get
        {
            instance ??= new SettingsManager();
            return instance;
        }
    }

    private int viewDistance;
    public int ViewDistance { get { return viewDistance; }
        set {
            viewDistance = value;
            SetViewArea();
        }
    }

    public Vector2Int[] ViewArea { get; private set; }

    public SettingsManager()
    {

    }

    private void SetViewArea()
    {
        List<Vector2Int> chunkOrder = new();

        // add all vectors for the visible area
        for (int x = -ViewDistance + 1; x < ViewDistance; x++)
        {
            for (int z = -ViewDistance + 1; z < ViewDistance; z++)
            {
                chunkOrder.Add(new Vector2Int(x, z));
            }
        }

        // sort it
        for (int i = 0; i + 1 < chunkOrder.Count; i++)
        {
            for (int j = i + 1; j < chunkOrder.Count; j++)
            {
                if (chunkOrder[i].magnitude > chunkOrder[j].magnitude)
                {
                    (chunkOrder[i], chunkOrder[j]) = (chunkOrder[j], chunkOrder[i]);
                }
            }
        }

        ViewArea = chunkOrder.ToArray();
    }
}