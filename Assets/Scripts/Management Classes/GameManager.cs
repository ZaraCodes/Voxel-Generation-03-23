using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            instance ??= new GameManager();
            return instance;
        }
    }

    private bool isLoading;
    public bool IsLoading { get { return isLoading; } set { isLoading = value; } }

    public ThreadedChunkBuilder ChunkBuilder { get; set; }

    private GameManager()
    {
        isLoading = false;
    }
}
