using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BubbleManager : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab; 
    [SerializeField] private Transform pivotGrid;

    public delegate void ClearEvent();
    public delegate void ScoreEvent(int s);
    public event ClearEvent clearBallEvent;
    public event ScoreEvent scoreEvent;  

    private List<int[,]> sData = new List<int[,]>();
    private List<GridCell> maxSizeGrid = new List<GridCell>();

    private Color a = new Color(Color.white.r, Color.white.g, Color.white.b, 0);
    private Vector3 originalPosition;  
    private int size = 0;
    private int numberOfInitRow;        
    private int numberOfBubbleColor;     
    [HideInInspector] public int noBubblesSameColor = 0;

    GridManager gridManager;
    private void Start()
    {
        originalPosition = pivotGrid.localPosition;                         
    }

    #region 버블 생성 & 제거 (시작, 발사)

    public void InitBubbles(LevelProfile level, List<int[,]> stageData)
    {
        if (gridManager == null)
        {
            gridManager = new GridManager(11, 10, 85, 85); 
            numberOfInitRow = level.GetInitRow();       
            numberOfBubbleColor = level.GetNumColor();
            sData = stageData;
        }

        for (int i = 0; i < gridManager.gridWidth; i++)
        {
            for (int j = 0; j < numberOfInitRow; j++)
            {
                if (gridManager.CanSetGridPos(i, j, stageData))
                {
                    Bubble bubble = InstantiateNewBubble(RandomBubbleColor(numberOfBubbleColor + 1));
                    SetBubbleToGrid(bubble, i, j);
                    bubble.FixPos();
                }
            }
        }
    }

    private Bubble InstantiateNewBubble(ValueManager.BubbleColor color)
    {
        Bubble bubble = ObjectPooling.Instance.GetPooledBubble();
        bubble.Init(this);
        bubble.SetBubbleColor(color);

        GameObject go = bubble.gameObject;
        go.transform.parent = pivotGrid;            
        go.transform.localScale = Vector3.one;      
        go.transform.localPosition = new Vector3(-1000, 0, 0); 
        go.SetActive(true);

        return bubble;
    }

    public Bubble GenerateBubbleTypeShoot()
    {
        ValueManager.BubbleColor randomColor = (ValueManager.BubbleColor)Random.Range(1, numberOfBubbleColor + 1);
        Bubble bubble = InstantiateNewBubble(randomColor); 
        bubble.tag = ValueManager.LAYER_SHOOTBUBBLE;             
        bubble.ChangeLayer(ValueManager.LAYER_SHOOTBUBBLE);     
        return bubble;
    }

    public Bubble GenerateSpecialBubbleTypeShoot()
    {
        ValueManager.BubbleColor greenColor = ValueManager.BubbleColor.Green;
        Bubble bubble = InstantiateNewBubble(greenColor); 
        bubble.tag = ValueManager.LAYER_SHOOTBUBBLE;             
        bubble.ChangeLayer(ValueManager.LAYER_SHOOTBUBBLE);       
        return bubble;
    }

    private ValueManager.BubbleColor RandomBubbleColor(int num)
    {
        return (ValueManager.BubbleColor)Random.Range(1, num); 
    }

    private void RemoveBubbleFromGame(Bubble ball)
    {
        if (ball != null)
            ball.RemoveBubble();
    }
    #endregion

    #region 그리드 할당 & 제거
    private void SetBubbleToGrid(Bubble ball, int x, int y)
    {
        GridCell grid = gridManager.RegisterBubble(x, y, ball);

        ball.SetGridPos(grid);                 
        ball.transform.localPosition = ball.GetGridPos().pos;
        ball.name = "Bubble_" + x.ToString() + y.ToString();
    }

    public void SetShootBubbleToGrid(Bubble bubble)
    {
        bubble.transform.parent = pivotGrid;            
        bubble.transform.localScale = Vector3.one;     
        GridCell nearestCell = gridManager.FindNearestGridCell(bubble.transform.localPosition);
        SetBubbleToGrid(bubble, nearestCell.x, nearestCell.y);
    }

    public void SetShootBubbleToGrid(Bubble bullet, GridCell gridCellClue)
    {
        bullet.transform.parent = pivotGrid;            
        bullet.transform.localScale = Vector3.one;     
        GridCell nearestCell = gridManager.FindNearestGridCell(gridCellClue, bullet.transform.localPosition);
        SetBubbleToGrid(bullet, nearestCell.x, nearestCell.y);
    }

    public void RemoveBubbleFromGrid(GridCell cell)
    {
        gridManager.RemoveBubbleFromGridCell(cell); 
    }

    private int RemoveFallBubble()
    {
        List<Bubble> listUnHoldBalls = gridManager.GetListFallBubblesFromGrid();
        foreach (Bubble b in listUnHoldBalls)
        {
            b.EffectFallingBubble();
        }

        return listUnHoldBalls.Count;
    }
    #endregion

    #region 버블 폭발
    public void ExplodeGreenBubble(Bubble shootBubble)
    {
        List<GridCell> listNeighbor = gridManager.GetNeighborCells(shootBubble.GetGridPos().x, shootBubble.GetGridPos().y);
        listNeighbor.Add(shootBubble.GetGridPos());
        foreach (GridCell cell in listNeighbor)
        {
            if (cell.bubble != null)
            {
                cell.bubble.EffectExplodeBubble();
                RemoveBubbleFromGrid(cell);
            }
        }

        int noBallFallingDown = RemoveFallBubble();

        ScoreCalSameColor();
        GameManager.Instance.BossDamage();
    }

    public void ExplodeSameColorBubble(Bubble bubble)
    {
        if (CheckAndExplodeSameColorBubbles(bubble))
        {
            CheckClearBalls();
        }
    }

    public bool CheckAndExplodeSameColorBubbles(Bubble shootBubble)
    {
        List<GridCell> listSameColors = gridManager.GetListNeighborsSameColorRecursive(shootBubble);
        bool isExploded = listSameColors.Count >= 2;

        if (isExploded)
        {
            listSameColors.Add(shootBubble.GetGridPos());
            noBubblesSameColor = listSameColors.Count;
            foreach (GridCell cell in listSameColors)
            {
                cell.bubble.EffectExplodeBubble();
                RemoveBubbleFromGrid(cell);
            }

            int noBallFallingDown = RemoveFallBubble();
            ScoreCalSameColor();
            GameManager.Instance.BossDamage();
        }

        return isExploded;
    }
    #endregion

    #region 버블 생성 (재생성)
    public void SetBubbleMaxColor(List<GridCell> maxSizeGrid, int size)
    {
        ValueManager.BubbleColor bColor;
        Color cColor = new Color();

        int pivotIndex = maxSizeGrid.Count - 1 - size;
        int lastIndex = maxSizeGrid.Count - 1;

        for (int i = pivotIndex; i > 0; i--)
        {
            cColor = maxSizeGrid[i].bubble.sprite.color;
            maxSizeGrid[lastIndex].bubble.sprite.color = cColor;

            bColor = maxSizeGrid[i].bubble.GetBubbleColor();
            maxSizeGrid[lastIndex].bubble.SetBubbleColor(bColor);

            lastIndex--;
        }

        for (int i = 0; i < lastIndex; i++)
        {
            bColor = RandomBubbleColor(numberOfBubbleColor + 1);
            GridCell c = gridManager.GetGridCell(maxSizeGrid[i].x, maxSizeGrid[i].y);
            c.bubble.SetBubbleColor(bColor);
        }
    }

    public void SetBubbleMax(List<GridCell> maxSizeGrid)
    {
        int countBubble = 0;
        for (int i = maxSizeGrid.Count; i > 0; i--)
        {
            if (gridManager.CanSetGridPos(maxSizeGrid[i - 1].x, maxSizeGrid[i - 1].y, sData) && !gridManager.IsExistBall(maxSizeGrid[i - 1].x, maxSizeGrid[i - 1].y))
            {
                Bubble bubble = InstantiateNewBubble(RandomBubbleColor(numberOfBubbleColor + 1));
                bubble.sprite.color = a;

                SetBubbleToGrid(bubble, maxSizeGrid[i - 1].x, maxSizeGrid[i - 1].y);
                bubble.FixPos();

                countBubble++;
            }
        }

        size = countBubble;

        if (!CheckBubble(maxSizeGrid))
            return;
    }

    private bool CheckBubble(List<GridCell> maxSizeGrid)
    {
        foreach (GridCell c in maxSizeGrid)
        {
            if (c.bubble == null)
                return false;
        }

        return true;
    }
    public void SetNewBubble()
    {
        StartCoroutine(SetNewBubble_Cor());
    }

    IEnumerator SetNewBubble_Cor()
    {
        yield return new WaitForSeconds(0.5f);

        maxSizeGrid = gridManager.AddBubble(sData);
        SetBubbleMax(maxSizeGrid);

        yield return new WaitForSeconds(0.1f);

        SetBubbleMaxColor(maxSizeGrid, size);
    }
    #endregion

    #region 스코어 & 이벤트
    public void ScoreCalSameColor()
    {
        if (scoreEvent != null)
        {
            int calScore = GameManager.Instance.score.CalculateScore(noBubblesSameColor);
            GameManager.Instance.score.SetScore(GameManager.Instance.score.GetScore() + calScore);
            scoreEvent(GameManager.Instance.score.GetScore());
        }
    }

    public void ScoreCalDrop(int point)
    {
        int calScore = GameManager.Instance.score.CalculateScoreFallingDown(point);
        GameManager.Instance.score.SetScore(GameManager.Instance.score.GetScore() + calScore);
        scoreEvent(GameManager.Instance.score.GetScore());
    }

    public void ClearBubbles()
    {
        for (int i = 0; i < gridManager.gridWidth; i++)
        {
            for (int j = 0; j < gridManager.gridHeight; j++)
            {
                RemoveBubbleFromGame(gridManager.GetGridCell(i, j).bubble);
                RemoveBubbleFromGrid(gridManager.GetGridCell(i, j));
            }
        }
    }

    private int CountRemainBubbles()
    {
        int count = 0;
        for (int i = 0; i < gridManager.gridWidth; i++)
        {
            for (int j = 0; j < gridManager.gridHeight; j++)
            {
                if (gridManager.IsExistBall(i, j))
                    count++;
            }
        }
        return count;  
    }

    private void CheckClearBalls()
    {
        if (CountRemainBubbles() == 0)
        {
            if (clearBallEvent != null)
                clearBallEvent(); 
        }
    }

    // Event
    public void RegisterEventClearBubble(ClearEvent e)
    {
        clearBallEvent = e;
    }
    public void RegisterEventCalculateScore(ScoreEvent e)
    {
        scoreEvent = e;
    }

    public void Reset()
    {
        pivotGrid.localPosition = originalPosition;
        GameManager.Instance.score.SetScore(0);
    }
    #endregion
}
