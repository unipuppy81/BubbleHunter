using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
public class StageData : ScriptableObject
{
    public int lifeCount;
    public int stageNumber;
    public int startPosX;
    public int startPosY;
    public List<int> xList = new List<int>();
    public List<int> yList = new List<int>();
}
