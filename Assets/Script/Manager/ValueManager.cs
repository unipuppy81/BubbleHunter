using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueManager
{
    public enum BubbleColor
    {
        None,  
        Red,   
        Blue,  
        Yellow, 
        Green // Bomb
    }

    public enum GameState
    {
        Playing,    
        Gameover   
    }

    public static int life { private get; set; }

    #region 이벤트
    public delegate void SimpleEvent();
    public delegate void SimpleEventIntegerParams(int param);

    #endregion

    #region 레이어
    public const string LAYER_BUBBLE = "Bubble";               
    public const string LAYER_SHOOTBUBBLE = "ShootBubble";          
    public const string LAYER_WALL = "Wall";              
    public const string LAYER_WALL_LINE = "WallForLine"; 
    public const string LAYER_NONE = "None";     
    #endregion
}
