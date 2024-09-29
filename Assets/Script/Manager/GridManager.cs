using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager
{
    private GridCell[,] gridCell;
    public int gridWidth { get; private set; }
    public int gridHeight { get; private set; }
    public int cellSizeX { get; private set; }
    public int cellSizeY { get; private set; }


    public bool[,] visited;
    public List<GridCell> attachedCellList = new List<GridCell>();

    public GridManager(int gridSizeX, int gridSizeY, int cellSizeX, int cellSizeY)
    {
        gridWidth = gridSizeX;
        gridHeight = gridSizeY;
        this.cellSizeX = cellSizeX;
        this.cellSizeY = cellSizeY;
        gridCell = new GridCell[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                gridCell[i, j] = new GridCell(i, j, TransformGridToWorldPos(i, j), true);
            }
        }
    }

    #region ·ÎÁ÷
    public GridCell RegisterBubble(int x, int y, Bubble bubble)
    {
        gridCell[x, y].bubble = bubble;
        gridCell[x, y].pos = TransformGridToWorldPos(x, y);
        return gridCell[x, y];
    }
    private Vector3 TransformGridToWorldPos(int x, int y)
    {
        float pivotTopLeftX = (y % 2 == 0) ? 0 : cellSizeX / 2.0f;
        float positionX = pivotTopLeftX + x * cellSizeX + cellSizeX / 2.0f;
        float positionY = -y * cellSizeY - cellSizeY / 2.0f;
        return new Vector3(positionX, positionY, 0);
    }

    public GridCell GetGridCell(int x, int y)
    {
        return gridCell[x, y];
    }

    public bool CanSetGridPos(int x, int y, List<int[,]> stageData)
    {
        bool boundaryX = (x < gridWidth && x >= 0);
        bool boundaryY = (y < gridHeight && y >= 0);
        bool evenOrOdd = (y % 2) == 0 ? true : (x < gridWidth - 1);

        bool canSet = boundaryX && boundaryY && evenOrOdd;

        if (!canSet)
            return false;

        foreach (var gridArray in stageData)
        {
            for (int i = 0; i < gridArray.GetLength(0); i++)
            {
                if (gridArray[i, 0] == x && gridArray[i, 1] == y)
                    return true;
            }
        }

        return false;
    }

    public bool CanSetGridPos(int x, int y)
    {
        bool boundaryX = (x < gridWidth && x >= 0);
        bool boundaryY = (y < gridHeight && y >= 0);
        bool evenOrOdd = (y % 2) == 0 ? true : (x < gridWidth - 1);
        return boundaryX && boundaryY && evenOrOdd;
    }

    public bool IsExistBall(int x, int y)
    {
        if (CanSetGridPos(x, y))
        {
            return gridCell[x, y].bubble != null;
        }
        return false;
    }
    #endregion

    #region Å½»ö
    public List<GridCell> AddBubble(List<int[,]> stageData)
    {
        bool[,] visited = new bool[gridWidth, gridHeight];
        Queue<GridCell> queue = new Queue<GridCell>();

        int _x = (int)GameManager.Instance.startPos.x;
        int _y = (int)GameManager.Instance.startPos.y;

        GridCell newCell = new GridCell(_x, _y);
        List<GridCell> sequenceLoadGrid = new List<GridCell>();

        visited[newCell.x, newCell.y] = true;
        queue.Enqueue(gridCell[newCell.x, newCell.y]);
        sequenceLoadGrid.Add(newCell);

        foreach (var gridArray in stageData)
        {
            for (int i = 0; i < gridArray.GetLength(0); i++)
            {
                queue.Enqueue(gridCell[gridArray[i, 0], gridArray[i, 1]]);
            }
        }

        while (queue.Count > 0)
        {
            GridCell cur = queue.Dequeue();

           
                if (CanSetGridPos(cur.x, cur.y, stageData))
                {
                    sequenceLoadGrid.Add(gridCell[cur.x, cur.y]);
                }

    
        }

        return sequenceLoadGrid;
    }

    // BFS
    public List<GridCell> AddBubble2(List<int[,]> stageData)
    {
        bool[,] visited = new bool[gridWidth, gridHeight];
        Queue<GridCell> queue = new Queue<GridCell>();

        int _x = (int)GameManager.Instance.startPos.x;
        int _y = (int)GameManager.Instance.startPos.y;

        GridCell newCell = new GridCell(_x, _y);
        List<GridCell> sequenceLoadGrid = new List<GridCell>();

        visited[newCell.x, newCell.y] = true;
        queue.Enqueue(gridCell[newCell.x, newCell.y]);
        sequenceLoadGrid.Add(newCell);

        
        while (queue.Count > 0)
        {
            GridCell cur = queue.Dequeue();

            List<GridCell> pairs = new List<GridCell>();
            pairs.Add(new GridCell(cur.x, cur.y + 1));
            pairs.Add(new GridCell(cur.x, cur.y - 1));
            pairs.Add(new GridCell(cur.x + 1, cur.y));
            pairs.Add(new GridCell(cur.x - 1, cur.y));

            if (cur.y % 2 == 0)
            {
                pairs.Add(new GridCell(cur.x - 1, cur.y + 1));
                pairs.Add(new GridCell(cur.x - 1, cur.y - 1));
            }
            else
            {
                pairs.Add(new GridCell(cur.x + 1, cur.y + 1));
                pairs.Add(new GridCell(cur.x + 1, cur.y - 1));
            }

            foreach (var pair in pairs)
            {
                int dx = pair.x;
                int dy = pair.y;

                if (CanSetGridPos(dx, dy, stageData) && !visited[dx, dy])
                {
                    queue.Enqueue(gridCell[dx, dy]);
                    visited[dx, dy] = true;
                    sequenceLoadGrid.Add(gridCell[dx, dy]);
                }

            }
        }
        
        return sequenceLoadGrid;
    }

    public List<GridCell> GetNeighborSameColorBubbles(ValueManager.BubbleColor color, int x, int y)
    {
        List<GridCell> list = new List<GridCell>();
        List<GridCell> neightbors = GetNeighborCells(x, y);
        foreach (GridCell cell in neightbors)
        {
            if (IsExistBall(cell.x, cell.y) && cell.bubble.GetBubbleColor() == color)
            {
                list.Add(cell);
            }
        }
        return list;
    }

    public List<GridCell> GetNeighborCells(int x, int y)
    {
        List<GridCell> neighbors = new List<GridCell>();
        List<GridCell> pairs = new List<GridCell>();

        pairs.Add(new GridCell(x, y + 1));
        pairs.Add(new GridCell(x, y - 1));
        pairs.Add(new GridCell(x + 1, y));
        pairs.Add(new GridCell(x - 1, y));

        if (y % 2 == 0)
        {
            pairs.Add(new GridCell(x - 1, y + 1));
            pairs.Add(new GridCell(x - 1, y - 1));
        }
        else
        {
            pairs.Add(new GridCell(x + 1, y + 1));
            pairs.Add(new GridCell(x + 1, y - 1));
        }

        foreach (GridCell pair in pairs)
        {
            if (CanSetGridPos(pair.x, pair.y))
                neighbors.Add(gridCell[pair.x, pair.y]);
        }

        return neighbors;
    }

    public GridCell FindNearestGridCell(Vector3 position)
    {
        List<GridCell> listNeighbors = new List<GridCell>();

        for (int i = 0; i < gridWidth; i++)
            listNeighbors.Add(gridCell[i, 0]);

        return FindNearestGridCellInList(listNeighbors, position);
    }

    public GridCell FindNearestGridCell(GridCell cellClue, Vector3 position)
    {
        List<GridCell> listNeighbors = GetNeighborCells(cellClue.x, cellClue.y);
        return FindNearestGridCellInList(listNeighbors, position);
    }

    private GridCell FindNearestGridCellInList(List<GridCell> listNeighbors, Vector3 position)
    {
        float smallestDistance = 10000;
        GridCell nearestCell = null;
        foreach (GridCell gridCell in listNeighbors)
        {
            if (!IsExistBall(gridCell.x, gridCell.y))
            {
                float currentDistance = Vector3.Distance(gridCell.pos, position);

                if (currentDistance < smallestDistance)
                {
                    smallestDistance = currentDistance;
                    nearestCell = gridCell;
                }
            }
        }
        return nearestCell;
    }

    public void RemoveBubbleFromGridCell(GridCell cell)
    {
        gridCell[cell.x, cell.y].bubble = null;
    }

    public List<GridCell> GetListNeighborsSameColorRecursive(Bubble bubble)
    {
        List<GridCell> sameColors = new List<GridCell>();
        List<GridCell> neighbors = GetNeighborSameColorBubbles(bubble.GetBubbleColor(),
                                       bubble.GetGridPos().x, bubble.GetGridPos().y);
        GridCell mainCell = bubble.GetGridPos();
        do
        {
            List<GridCell> listTemp = new List<GridCell>();
            foreach (GridCell cell in neighbors)
            {
                List<GridCell> list = GetNeighborSameColorBubbles(mainCell.bubble.GetBubbleColor(), cell.x, cell.y);
                
                list = list.FindAll(c => !neighbors.Contains(c));
                list = list.FindAll(c => !listTemp.Contains(c));
                list = list.FindAll(c => !sameColors.Contains(c));

                if (list.Contains(mainCell))
                    list.Remove(mainCell);

                listTemp.AddRange(list);
            }
            sameColors.AddRange(neighbors);
            neighbors = listTemp;
        } while (neighbors.Count > 0);

        return sameColors;
    }
    #endregion 

    #region ¶³¾îÁö´Â ¹öºí
    public List<Bubble> GetListFallBubblesFromGrid()
    {
        List<Bubble> removedList = GetListFallBubblesFromGridRecursive();
        return removedList;
    }

    private List<Bubble> GetListFallBubblesFromGridRecursive()
    {
        List<Bubble> removedList = new List<Bubble>();
        List<GridCell> bubbleExistList = new List<GridCell>(); 
        
        attachedCellList.Clear();

        for (int j = 1; j < gridHeight; j++)
        {
            for (int i = 0; i < gridWidth; i++)
            {
                if (IsExistBall(i, j))
                    bubbleExistList.Add(gridCell[i, j]);
            }
        }

        for(int i = 0;i < gridWidth; i++)
        {
            FindConnectedArea(gridCell[i, 0]);
        }

        foreach (GridCell cell in bubbleExistList)
        {
            if (!attachedCellList.Contains(cell))
            {
                removedList.Add(cell.bubble);  
                RemoveBubbleFromGridCell(cell);
            }
        }

        return removedList;
    }

    // BFS
    private void FindConnectedArea(GridCell startCell)
    {
        Queue<GridCell> queue = new Queue<GridCell>();

        visited = new bool[gridWidth, gridHeight];

        GridCell newCell = new GridCell(startCell.x, startCell.y);
        visited[newCell.x, newCell.y] = true;

        queue.Enqueue(gridCell[newCell.x, newCell.y]);

        while(queue.Count > 0)
        {
            GridCell cur = queue.Dequeue();
            List<GridCell> pairs = new List<GridCell>();

            pairs.Add(new GridCell(cur.x, cur.y + 1)); 
            pairs.Add(new GridCell(cur.x, cur.y - 1)); 
            pairs.Add(new GridCell(cur.x + 1, cur.y)); 
            pairs.Add(new GridCell(cur.x - 1, cur.y)); 

            if (cur.y % 2 == 0)
            {
                pairs.Add(new GridCell(cur.x - 1, cur.y + 1)); 
                pairs.Add(new GridCell(cur.x - 1, cur.y - 1)); 
            }
            else
            {
                pairs.Add(new GridCell(cur.x + 1, cur.y + 1)); 
                pairs.Add(new GridCell(cur.x + 1, cur.y - 1)); 
            }

            foreach (var pair in pairs)
            {
                int dx = pair.x;
                int dy = pair.y;

                if (IsExistBall(dx, dy) && !visited[dx, dy])
                {
                    if (gridCell[dx, dy].bubble.CompareTag("Bubble"))
                    {
                        queue.Enqueue(gridCell[dx, dy]);
                        visited[dx, dy] = true;
                        attachedCellList.Add(gridCell[dx, dy]);
                    }
                }
            }

        }
    }
    #endregion
}
