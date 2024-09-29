using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Class")]
    [SerializeField] private BubbleManager bubbleManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ControlManager inputController;
    [SerializeField] private ShootLineRender shootLineRender;
    public Score score;

    [Header("Object")]
    [SerializeField] private Boss boss;
    [SerializeField] private Shoot arrow;
    [SerializeField] private LevelProfile level;
    public List<StageData> stageData;
    public Transform ScoreGate;


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI lifeText;




    [Header("Var")]
    public int stageNumber = 10;
    public int Life = 0;

    public Vector2 startPos = Vector2.zero;
    ValueManager.GameState gameState; 
    public List<int[,]> trueGridPoint = new List<int[,]>();

    private void Start()
    {
        RegisterEventScore();
        RegisterEventTouch();
        RegisterEventWin();

        OnStartGame();
        DisplayLife();
    }

    #region 변수

    public void LifeMinus()
    {
        Life--;
        DisplayLife();
        if (Life <= 0)
        {
            OnGameover();
        }
    }
    public LevelProfile GetLevelProfile()
    {
        return level;
    }
    #endregion

    #region 게임 세팅
    public void OnStartGame()
    {
        gameState = ValueManager.GameState.Playing;
        FindStage();

        arrow.LoadDoneBubble(bubbleManager.GenerateBubbleTypeShoot(), bubbleManager.GenerateBubbleTypeShoot());
        arrow.UnBlockArrow();
        bubbleManager.InitBubbles(GetLevelProfile(), trueGridPoint);

        score = new Score();
    }

    public void FindStage()
    {
        StageData stage = stageData.FirstOrDefault(s => s.stageNumber == stageNumber);

        if (stage == null)
            return;

        trueGridPoint.Clear();
        Life = stage.lifeCount;
        int[,] gridArray = new int[stage.xList.Count, 2];

        for (int i = 0; i < stage.xList.Count; i++)
        {
            gridArray[i, 0] = stage.xList[i];
            gridArray[i, 1] = stage.yList[i];
        }

        startPos.x = stage.startPosX;
        startPos.y = stage.startPosY;

        trueGridPoint.Add(gridArray);
    }
    #endregion

    #region 게임 플레이
    public void OnShootAction()
    {
        Bubble newBullet = bubbleManager.GenerateBubbleTypeShoot();
        arrow.LoadShootBubble(newBullet);
    }

    public void SetLineColor(Color color)
    {
        shootLineRender.SetLineColor(color);
    }

    public void BossDamage()
    {
        boss.SpwanBubble();
    }
    #endregion

    #region 게임 상태
    public void OnGameover()
    {
        gameState = ValueManager.GameState.Gameover; 
        arrow.BlockArrow();
        uiManager.DisplayGameOver();
    }

    public void OnReset()
    {
        bubbleManager.ClearBubbles();
        bubbleManager.Reset();
        arrow.ClearBullets();
        arrow.ResetGunDirection();

        uiManager.DisableText();
        uiManager.UpdateScore(0);
    }

    public void OnWin()
    {
        gameState = ValueManager.GameState.Gameover; 
        arrow.BlockArrow();
        uiManager.DisplayWin();
        score.SetScore(score.GetScore() + Life * 10);
        Life = 0;
    }
    #endregion 

    #region UI

    private void DisplayScore(int score)
    {
        uiManager.UpdateScore(score);  
    }

    public void DisplayLife()
    {
        lifeText.text = Life.ToString();
    }
    #endregion 

    #region 이벤트
    private void RegisterEventTouch()
    {
        inputController.RegisterEventTouch((Vector3 position) =>
        {
            if (gameState == ValueManager.GameState.Gameover)
            {
                OnReset();   
                OnStartGame(); 
            }
        });
    }

    private void RegisterEventWin()
    {
        bubbleManager.RegisterEventClearBubble(OnWin); 
    }

    private void RegisterEventScore()
    {
        bubbleManager.RegisterEventCalculateScore(DisplayScore); 
    }

    #endregion
}

