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
        Vector2Int lastAdded = new();
        List<Vector2Int> chunkOrder = new()
        {
            lastAdded
        };

        for (int i = 1; i < ViewDistance + 1; i++)
        {
            int currWidth = i * 2 + 1;

            lastAdded = new(lastAdded.x + 1, lastAdded.y);
            chunkOrder.Add(lastAdded);

            for (int j = 0; j < currWidth - 2; j++)
            {
                lastAdded.y += 1;
                chunkOrder.Add(lastAdded);
            }
            for (int j = 0; j < currWidth - 1; j++)
            {
                lastAdded.x -= 1;
                chunkOrder.Add(lastAdded);
            }
            for (int j = 0; j < currWidth - 1; j++)
            {
                lastAdded.y -= 1;
                chunkOrder.Add(lastAdded);
            }
            for (int j = 0; j < currWidth - 1; j++)
            {
                lastAdded.x += 1;
                chunkOrder.Add(lastAdded);
            }
        }
        ViewArea = chunkOrder.ToArray();
    }
}