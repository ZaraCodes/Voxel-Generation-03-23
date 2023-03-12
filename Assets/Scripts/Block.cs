using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public string type = string.Empty;
    public Vector3 position = Vector3.zero;

    public (int,int,int)[] solidNeighbors;
}
