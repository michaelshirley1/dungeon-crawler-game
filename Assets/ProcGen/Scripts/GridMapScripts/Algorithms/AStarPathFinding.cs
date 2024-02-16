using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PathFindCell
{
  private bool _IsObstacle;
  private bool _MarkedAsVisited = false;
  private PathFindCell _originCell = null;

  private Vector2Int _cellPosition;
  private float _Gcost = float.MaxValue;
  private float _Hcost = float.MaxValue;

  public PathFindCell()
  {
  }

  public void SetCellInfo(Vector2Int pos, bool IsObstacle)
  {
        _cellPosition = pos;
        _IsObstacle = IsObstacle;
  }

  public void UpdateGcost(float newCost)
  {
    if (_Gcost > newCost) _Gcost = newCost;
  }

  public void UpdateHcost(float newCost)
  {
    if (_Hcost > newCost) _Hcost = newCost;
  }

  public void MarkAsVisited()
  {
    _MarkedAsVisited = true;
  }

  public void UpdateOrigin(PathFindCell origin)
  {
    _originCell = origin;
  }

  public PathFindCell GetOrigin()
  {
    return _originCell;
  }

  public bool IsVisited()
  {
    return _MarkedAsVisited;
  }

  public bool IsCellAnObstacle()
  {
    return _IsObstacle;
  }

  public float GetGCost()
  {
    return _Gcost;
  }

  public float GetHCost()
  {
    return _Hcost;
  }

  public float GetFCost()
  {
    return _Gcost + _Hcost;
  }

  public Vector2Int GetCellPosition()
  {
    return _cellPosition;
  }
}

public class AStarPathFinding
{
    private GridMap<PathFindCell> _aStarGrid;

    public void SetMap(GridMap<ProcGenHandler.CellTestType> grid)
    {
        _aStarGrid = new(grid.GetGridSize(), grid.GetCellSize(), grid.GetWorldPosition(0, 0, 0), () => { return new PathFindCell(); });

        for (int z = 0; z < _aStarGrid.GetGridSize().z; z++)
        {
            for (int x = 0; x < _aStarGrid.GetGridSize().x; x++)
            {
                var cell = grid.GetCell(x, 0, z);
                var pathFindCell = _aStarGrid.GetCell(x, 0, z);
                pathFindCell.SetCellInfo(new(x, z), cell == ProcGenHandler.CellTestType.FREE || cell == ProcGenHandler.CellTestType.OCCUPIED);
                _aStarGrid.SetCell(x, 0, z, pathFindCell);
            }
        }
    }

    public List<Vector2Int> Solve(Vector2Int startNode, Vector2Int endNode)
    {
        if (startNode == endNode) return new List<Vector2Int>();

        List<PathFindCell> visitedCells = new();
        List<PathFindCell> potentionCells = new();

        var cell = _aStarGrid.GetCell(startNode.x, 0, startNode.y);
        
        cell.UpdateGcost(0f);
        cell.UpdateHcost(Vector2Int.Distance(startNode, endNode));
        cell.MarkAsVisited();
        _aStarGrid.SetCell(startNode.x, 0, startNode.y, cell);

        visitedCells.Add(cell);
        PathFindCell currentNode = cell;
        bool EndHasBeenFound = false;

        int MAXLOOPS = 50000;
        int firstLoopCounter = 0;

        while (!EndHasBeenFound)
        {
            foreach (var nb in GetNeighbours(currentNode.GetCellPosition()))
            {
                if (nb.IsVisited() || nb.IsCellAnObstacle()) continue;

                if (nb.GetCellPosition() == endNode)
                {
                    EndHasBeenFound = true;
                    break;
                }
                
                float neighbourGCost = currentNode.GetGCost() + Vector2Int.Distance(currentNode.GetCellPosition(), nb.GetCellPosition());
                nb.UpdateGcost(neighbourGCost);
                nb.UpdateHcost(Vector2Int.Distance(nb.GetCellPosition(), endNode));
                nb.UpdateOrigin(currentNode);
                _aStarGrid.SetCell(nb.GetCellPosition().x, 0, nb.GetCellPosition().y, cell);

                potentionCells.Add(nb);
            }

            float LowestFValue = float.MaxValue;
            float SimilarFValueCount = 0;
            int lowestPotentialCellIndex = -1;
            for (int i = 0; i < potentionCells.Count; i++)
            {
                if (LowestFValue > potentionCells[i].GetFCost())
                {   
                    LowestFValue = potentionCells[i].GetFCost();
                    lowestPotentialCellIndex = i;
                    SimilarFValueCount = 1;
                }
                else if(LowestFValue == potentionCells[i].GetFCost())
                {
                    SimilarFValueCount++;
                }
            }

            float LowestHValue = float.MaxValue;
            if (SimilarFValueCount > 1)
            {
                for (int i = 0; i < potentionCells.Count; i++)
                {
                    if (potentionCells[i].GetFCost() == LowestFValue && 
                        LowestHValue > potentionCells[i].GetHCost())
                    {
                        LowestHValue = potentionCells[i].GetHCost();
                        lowestPotentialCellIndex = i;
                    }
                }
            }

            if (lowestPotentialCellIndex == -1)
            {
                break;
            }

            Debug.Log(lowestPotentialCellIndex);
            visitedCells.Add(potentionCells[lowestPotentialCellIndex]);
            currentNode = potentionCells[lowestPotentialCellIndex];
            potentionCells.RemoveAt(lowestPotentialCellIndex);
            currentNode.MarkAsVisited();
            _aStarGrid.SetCell(currentNode.GetCellPosition().x, 0, currentNode.GetCellPosition().y, currentNode);

            firstLoopCounter++;
            if(firstLoopCounter > MAXLOOPS)
            {
                Debug.LogWarning("LOOP 1 ENDED!!!");
                return new List<Vector2Int>();
            }
        }
        

        if (EndHasBeenFound)
        {
            bool isAtStart = false;
            int secondLoopCounter = 0;

            List<Vector2Int> PathPositions = new();
            PathFindCell originCell = currentNode;
            while (!isAtStart)
            {   
                PathPositions.Add(originCell.GetCellPosition());
                originCell = originCell.GetOrigin();

                if (originCell.GetCellPosition() == startNode)
                {
                    isAtStart = true;
                }

                secondLoopCounter++;
                if(secondLoopCounter > MAXLOOPS)
                {
                    Debug.LogWarning("LOOP 2 ENDED!!!");
                    return new List<Vector2Int>();
                }
            }

            return PathPositions;
        }

        return new List<Vector2Int>(); 
    }


    private List<PathFindCell> GetNeighbours(Vector2Int cellPos)
    {
        List<PathFindCell> ListOfNeighbours = new();
        Vector3Int currentGridSize = _aStarGrid.GetGridSize();

        if ((cellPos.x + 1) >= 0 && (cellPos.x + 1) < currentGridSize.x) ListOfNeighbours.Add(_aStarGrid.GetCell(cellPos.x + 1, 0, cellPos.y));
        if ((cellPos.x - 1) >= 0 && (cellPos.x - 1) < currentGridSize.x) ListOfNeighbours.Add(_aStarGrid.GetCell(cellPos.x - 1, 0, cellPos.y));
        if ((cellPos.y + 1) >= 0 && (cellPos.y + 1) < currentGridSize.z) ListOfNeighbours.Add(_aStarGrid.GetCell(cellPos.x, 0, cellPos.y + 1));
        if ((cellPos.y - 1) >= 0 && (cellPos.y - 1) < currentGridSize.z) ListOfNeighbours.Add(_aStarGrid.GetCell(cellPos.x, 0, cellPos.y - 1));
        

        return ListOfNeighbours;
    }

    public void DrawDebugLines()
    {
        _aStarGrid.DrawDebugLines(Color.cyan);
    }
}
