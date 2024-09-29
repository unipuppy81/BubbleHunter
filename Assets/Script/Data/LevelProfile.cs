using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelProfile
{
    [SerializeField] private int life;
    [SerializeField] private int initRows;
    [SerializeField] private int colorCount;
    
    public int GetInitRow()
    {
        return Mathf.Clamp(initRows, 1, 12);
    }

    public int GetNumColor()
    {
        return Mathf.Clamp(colorCount, 2, 5);
    }
}
