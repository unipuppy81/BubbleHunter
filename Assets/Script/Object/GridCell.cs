using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public int x;
    public int y;
    public Vector3 pos;
    public bool isValid = false;
    public Bubble bubble;

    public GridCell(int gridX, int gridY, Vector3 realPos, bool check)
    {
        x = gridX;
        y = gridY;
        pos = realPos;
        isValid = check;
    }

    public GridCell(int gridX, int gridY)
    {
        x = gridX;
        y = gridY;
    }
}

