using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AStarCell
{
  private bool _IsObstacle;
  private bool _MarkedAsVisited = false;
  private AStarCell _originCell = null;

  private Vector2Int _cellPosition;
  private float _Gcost = float.MaxValue;
  private float _Hcost = float.MaxValue;

  public AStarCell(Vector2Int pos, bool IsObstacle)
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

  public void UpdateOrigin(AStarCell origin)
  {
    _originCell = origin;
  }

  public AStarCell GetOrigin()
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

public class AStarAlgorithm
{
    private AStarCell[,] _aStarGrid;
    private int _Height = 0;
    private int _Width = 0;

    public void SetMap(GridCell[,] grid)
    {
        _Width = grid.GetLength(0);
        _Height = grid.GetLength(1);

        _aStarGrid = new AStarCell[_Width, _Height];

        for (int y = 0; y < _Height; y++)
        {
        for (int x = 0; x < _Width; x++)
            {
                _aStarGrid[x,y] = new AStarCell(
                    new Vector2Int(x, y),
                    grid[x, y]._currentState == GridCell.State.OCCUPIED
                );
            }
        }
    }

    public List<Vector2Int> Solve(Vector2Int startNode, Vector2Int endNode)
    {
        if (startNode == endNode) return new List<Vector2Int>();

        List<AStarCell> visitedCells = new();
        List<AStarCell> potentionCells = new();
        
        Debug.Log((startNode.x, startNode.y));
        
        _aStarGrid[startNode.x, startNode.y].UpdateGcost(0f);
        _aStarGrid[startNode.x, startNode.y].UpdateHcost(Vector2Int.Distance(startNode, endNode));
        _aStarGrid[startNode.x, startNode.y].MarkAsVisited();

        visitedCells.Add(_aStarGrid[startNode.x, startNode.y]);
        AStarCell currentNode = _aStarGrid[startNode.x, startNode.y];
        bool EndHasBeenFound = false;

        int MAXLOOPS = 50000;
        int firstLoopCounter = 0;

        while (!EndHasBeenFound)
        {
            foreach (var nb in GetNeighbours(currentNode.GetCellPosition()))
            {
                if (nb.IsVisited()) continue;

                if (nb.GetCellPosition() == endNode)
                {
                    EndHasBeenFound = true;
                    break;
                }
                
                float neighbourGCost = currentNode.GetGCost() + Vector2Int.Distance(currentNode.GetCellPosition(), nb.GetCellPosition());
                nb.UpdateGcost(neighbourGCost);
                nb.UpdateHcost(Vector2Int.Distance(nb.GetCellPosition(), endNode));
                nb.UpdateOrigin(currentNode);
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
            _aStarGrid[currentNode.GetCellPosition().x, currentNode.GetCellPosition().y] = currentNode;

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
            AStarCell originCell = currentNode;
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


    private List<AStarCell> GetNeighbours(Vector2Int cellPos)
    {
        List<AStarCell> ListOfNeighbours = new();

        if ((cellPos.x + 1) >= 0 && (cellPos.x + 1) < _Width) ListOfNeighbours.Add(_aStarGrid[cellPos.x + 1, cellPos.y]);
        if ((cellPos.x - 1) >= 0 && (cellPos.x - 1) < _Width) ListOfNeighbours.Add(_aStarGrid[cellPos.x - 1, cellPos.y]);
        if ((cellPos.y + 1) >= 0 && (cellPos.y + 1) < _Height) ListOfNeighbours.Add(_aStarGrid[cellPos.x, cellPos.y + 1]);
        if ((cellPos.y - 1) >= 0 && (cellPos.y - 1) < _Height) ListOfNeighbours.Add(_aStarGrid[cellPos.x, cellPos.y - 1]);
        

        return ListOfNeighbours;
    }
}
