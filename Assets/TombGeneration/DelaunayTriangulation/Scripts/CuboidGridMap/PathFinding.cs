// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class AStarAlgoCell : BaseCell
// {
//     private float _gCost = float.MaxValue;
//     private float _hCost = float.MaxValue;
//     private float _fCost = float.MaxValue; 
//     private bool _isObstacle = false;
//     private AStarAlgoCell _previousCell = null;
//     private bool _isStart = false;


//     public AStarAlgoCell(PlayfieldCell pfCell)
//      : base(pfCell.GetCellSize(), pfCell.GetGridPosition().x, 
//             pfCell.GetGridPosition().y, pfCell.GetOffset())
//     {
//         _isObstacle = pfCell.GetState() == PlayfieldCell.State.FILLED;
//     }
    

//     public float GetGCost() { return _gCost; }
//     public float GetHCost() { return _hCost; }
//     public float GetFCost() { return _fCost; }
//     public bool IsObstacle() { return _isObstacle; }

//     public void ForceGCostToBe(float gCost)
//     {
//         _gCost = gCost;
//     }

//     public void SetAsStart()
//     {
//         _isStart = true;
//     }

//     public void CalculateCost(AStarAlgoCell cellBefore, AStarAlgoCell endCell)
//     {
//         if (_previousCell == null)
//         {
//             _previousCell = cellBefore;
//         }

//         // calculate gCost how far away from the start based on
//         // the known path already taken
//         float calculatedGCost = cellBefore.GetGCost() + Vector2Int.Distance(cellBefore.GetGridPosition(), GetGridPosition());
//         if (_gCost > calculatedGCost)
//         {
//             _gCost = calculatedGCost;
//             _previousCell = cellBefore;
//         }

//         // calculate hCost is based on how far away it is from the end goal
//         _hCost = Vector2Int.Distance(GetGridPosition(), endCell.GetGridPosition());
        
//         // calculate fCost is gCost + hCost
//         _fCost = _gCost + _hCost;
//     }

//     public AStarAlgoCell GetOriginCell()
//     {
//         return _previousCell;
//     }

//     public bool IsStart()
//     {
//         return _isStart;
//     }

// }

// public class PathFinding
// {
//     public enum SolveState
//     {
//         FAILED, SUCCESS, IN_PROGRESS, NOSOLVE
//     }

//     private bool _hasInitialised = false;
//     private bool _hasStarted = false;
//     private bool _endReached = false;
//     private SolveState _currentSolveState = SolveState.NOSOLVE;

//     private Vector2Int _gridSize;
//     private AStarAlgoCell[,] _pathFindMap;
//     private AStarAlgoCell _startCell = null;
//     private AStarAlgoCell _endCell = null;
//     private AStarAlgoCell _currentCell = null;
//     private AStarAlgoCell _backtrackedCell = null; 

//     private List<AStarAlgoCell> _visitedCells = new();
//     private List<AStarAlgoCell> _potentialCells = new();
//     private List<AStarAlgoCell> _backtrackedCells = new();

//     private void ResetValues()
//     {
//         _startCell = null;
//         _endCell = null;
//         _currentCell = null;
//         _backtrackedCell = null; 

//         _visitedCells = new();
//         _potentialCells = new(); 
//         _backtrackedCells = new();
    
//         _hasInitialised = false;
//         _hasStarted = false;
//         _endReached = false;

//         _currentSolveState = SolveState.NOSOLVE;
//     }

//     public void InitialiseMap(PlayfieldCell[,] playfieldGrid, bool resetMap = false)
//     {
//         if (!_hasInitialised || resetMap)
//         {
//             ResetValues();

//             _gridSize = new Vector2Int(playfieldGrid.GetLength(1), playfieldGrid.GetLength(0));
//             _pathFindMap = new AStarAlgoCell[_gridSize.y, _gridSize.x];

//             for (int y = 0; y < _gridSize.y; y++)
//             {
//                 for (int x = 0; x < _gridSize.x; x++)
//                 {
//                     _pathFindMap[y, x] = new AStarAlgoCell(playfieldGrid[y, x]);
//                 }
//             }
//         }

//         _hasInitialised = true;
//     }

//     public SolveState GetSolveState() { return _currentSolveState; }

//     public void StartFinder(Vector2Int start, Vector2Int end)
//     {
//         if (_hasStarted)
//         {
//             return;
//         }

//         if ((start.x < 0 || start.x >= _gridSize.x) &&
//             (start.y < 0 || start.y >= _gridSize.y))
//         {
//             return;
//         }
        
//         ResetValues();
//         _pathFindMap[start.y, start.x].ForceGCostToBe(0f);
//         _pathFindMap[start.y, start.x].SetAsStart();
//         _startCell = _pathFindMap[start.y, start.x];
//         _endCell = _pathFindMap[end.y, end.x];
        
//         _currentCell = _startCell;
//         AddToVisited(_currentCell, _startCell);

//         _hasStarted = true;
//         _endReached = false;

//         _currentSolveState = SolveState.IN_PROGRESS;
//     }

//     public void Step()
//     {
//         if (_hasStarted && !_endReached)
//         {
//             foreach(var nb in GetNeighbours(_currentCell))
//             {
//                 // if (nb.IsObstacle())
//                 // {
//                 //     Debug.Log("[PATHFINDER] OBS - " + _currentCell.GetGridPosition().ToString() + " - " + nb.GetGridPosition().ToString());
//                 // }
//                 // else
//                 // {
//                 //     Debug.Log("[PATHFINDER] GOOD - " + _currentCell.GetGridPosition().ToString() + " - " + nb.GetGridPosition().ToString());
//                 // }

//                 if (nb.IsObstacle() || HasCellBeenVisited(nb)) continue;

//                 var nbPos = nb.GetGridPosition();
//                 _pathFindMap[nbPos.y, nbPos.x].CalculateCost(_currentCell, _endCell);

//                 bool potentialDuplicateFound = false;
//                 for(int i = 0; i < _potentialCells.Count; i++)
//                 {
//                     if (_potentialCells[i].GetGridPosition() == _pathFindMap[nbPos.y, nbPos.x].GetGridPosition())
//                     {
//                         _pathFindMap[nbPos.y, nbPos.x] = _potentialCells[i];
//                         potentialDuplicateFound = true;
//                         break;
//                     }
//                 }

//                 if (!potentialDuplicateFound)
//                 {
//                     _potentialCells.Add(_pathFindMap[nbPos.y, nbPos.x]);
//                 }
//             }

//             float lowestFCost = float.MaxValue;
//             int numOfSimilarFCost = 1;
//             int lowestPotentialCellIndex = -1;


//             for(int i = 0; i < _potentialCells.Count; i++)
//             {
//                 if (lowestFCost > _potentialCells[i].GetFCost())
//                 {   
//                     lowestFCost = _potentialCells[i].GetFCost();
//                     lowestPotentialCellIndex = i;
//                     numOfSimilarFCost = 1;
//                 }
//                 else if(lowestFCost == _potentialCells[i].GetFCost())
//                 {
//                     numOfSimilarFCost++;
//                 }
//             }

//             float lowestHCost = float.MaxValue;
//             if (numOfSimilarFCost > 1)
//             {
//                 for(int i = 0; i < _potentialCells.Count; i++)
//                 {
//                     if (_potentialCells[i].GetFCost() == lowestFCost &&
//                         lowestHCost > _potentialCells[i].GetHCost())
//                     {
//                         lowestHCost = _potentialCells[i].GetHCost();
//                         lowestPotentialCellIndex = i;
//                     }
//                 }
//             }

//             if (_potentialCells.Count == 0)
//             {
//                 _endReached = false;
//                 _hasStarted = false;
//                 _currentSolveState = SolveState.FAILED;
//             }
            
//             var lowestCostCell = _potentialCells[lowestPotentialCellIndex];
//             Vector2Int lowestCellPos = lowestCostCell.GetGridPosition(); 

//             bool visitedDuplicateFound = false;
//             for(int i = 0; i < _visitedCells.Count; i++)
//             {
//                 if (_visitedCells[i].GetGridPosition() == _pathFindMap[lowestCellPos.y, lowestCellPos.x].GetGridPosition())
//                 {
//                     _pathFindMap[lowestCellPos.y, lowestCellPos.x] = _visitedCells[i];
//                     visitedDuplicateFound = true;
//                     break;
//                 }
//             }

//             if (!visitedDuplicateFound)
//             {
//                 _visitedCells.Add(_pathFindMap[lowestCellPos.y, lowestCellPos.x]);
//             }

//             _potentialCells.RemoveAt(lowestPotentialCellIndex);
//             _currentCell = lowestCostCell;


//             if ((_currentCell.GetGridPosition() == _endCell.GetGridPosition()) || _currentSolveState == SolveState.FAILED)
//             {
//                 if (_currentSolveState != SolveState.FAILED)
//                 {
//                     _endReached = true;
//                 }

//                 _backtrackedCell = _currentCell;
//             }
//         }

//         if (_endReached)
//         {
//             // backtrack
//             AStarAlgoCell origin = _backtrackedCell.GetOriginCell();
//             _backtrackedCells.Add(origin);

//             if (origin.IsStart())
//             {
//                 _endReached = false;
//                 _hasStarted = false;
//                 _currentSolveState = SolveState.SUCCESS;

//             }

//             _backtrackedCell = origin;
//         }
//     }

//     public List<AStarAlgoCell> GetVisitedCells()
//     {
//         return _visitedCells;
//     }

//     public List<AStarAlgoCell> GetPotentialCells()
//     {
//         return _potentialCells;
//     }

//     public List<AStarAlgoCell> GetBackTrackedCells()
//     {
//         return _backtrackedCells;
//     }


//     public bool HasStarted()
//     {
//         return _hasStarted;
//     }

//     public bool HasInitialised()
//     {
//         return _hasInitialised;
//     }


//     private List<AStarAlgoCell> GetNeighbours(AStarAlgoCell cell)
//     {
//         List<AStarAlgoCell> listOfNeighbours = new();
//         Vector2Int cellPos = cell.GetGridPosition();

//         if ((cellPos.x + 1) >= 0 && (cellPos.x + 1) < _gridSize.x)
//             listOfNeighbours.Add(_pathFindMap[cellPos.y, cellPos.x + 1]);
        
//         if ((cellPos.x - 1) >= 0 && (cellPos.x - 1) < _gridSize.x)
//             listOfNeighbours.Add(_pathFindMap[cellPos.y, cellPos.x - 1]);
        
//         if ((cellPos.y + 1) >= 0 && (cellPos.y + 1) < _gridSize.y)
//             listOfNeighbours.Add(_pathFindMap[cellPos.y + 1, cellPos.x]);
        
//         if ((cellPos.y - 1) >= 0 && (cellPos.y - 1) < _gridSize.y)
//             listOfNeighbours.Add(_pathFindMap[cellPos.y - 1, cellPos.x]);

//         return listOfNeighbours;
//     }

//     private bool HasCellBeenVisited(AStarAlgoCell cell)
//     {
//         foreach(var v in _visitedCells)
//         {
//             if (v.GetGridPosition() == cell.GetGridPosition())
//             {
//                 return true;
//             }
//         }

//         return false;
//     }

//     private void AddToVisited(AStarAlgoCell cell, AStarAlgoCell precidingCell)
//     {
//         Vector2Int currentCellPos = cell.GetGridPosition(); 
//         _pathFindMap[currentCellPos.y, currentCellPos.x].CalculateCost(precidingCell, _endCell);
//         _visitedCells.Add(_pathFindMap[currentCellPos.y, currentCellPos.x]);
//     }
// }
