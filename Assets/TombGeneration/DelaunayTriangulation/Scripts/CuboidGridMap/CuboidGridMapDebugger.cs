// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class CuboidGridMapDebugger : MonoBehaviour
// {
//     public GameObject TestObject;
//     public bool ShowPathFinding = true;
//     public bool ShowCellStates = true;


//     private Vector2Int _gridSize;
//     private PlayfieldCell[,] _gridMap = null;

//     private PlayfieldCell _start = null;
//     private PlayfieldCell _end = null;
//     private DebugCube[,] _gridDebugCubes;


//     private List<AStarAlgoCell> _visitedCells = new();
//     private List<AStarAlgoCell> _potentialCells = new();
//     private List<AStarAlgoCell> _backtrackedCells = new();

//     void Update()
//     {
//         for(int y = 0; y < _gridSize.y; y++)
//         {
//             for (int x = 0; x < _gridSize.x; x++)
//             {
//                 _gridMap[y, x].DrawDebugLines(Color.green);
//                 _gridMap[y, x].DrawCentreLines(Color.blue);
//             }
//         }

//         if (ShowCellStates)
//         {
//             UpdateDebugCubes();
//         }

//         if (ShowPathFinding)
//         {
//             UpdatePathFindingDebugCubes();
//         }
//     }

//     private void UpdateDebugCubes()
//     {
//         if (_gridMap == null) return;

//         for(int y = 0; y < _gridSize.y; y++)
//         {
//             for (int x = 0; x < _gridSize.x; x++)
//             {
//                 PlayfieldCell.State cellState = _gridMap[y, x].GetState();
//                 _gridDebugCubes[y, x].ClearText();

//                 switch (cellState)
//                 {
//                     case PlayfieldCell.State.FREE:
//                         _gridDebugCubes[y, x].SetColor(Color.white);
//                         break;
//                     case PlayfieldCell.State.FILLED:
//                         _gridDebugCubes[y, x].SetColor(Color.black);
//                         break;
//                     case PlayfieldCell.State.PADDING:
//                         _gridDebugCubes[y, x].SetColor(Color.grey);
//                         break;
//                     case PlayfieldCell.State.HALLWAY:
//                         Color color = new(1.0f, 0.5f, 0.0f);
//                         _gridDebugCubes[y, x].SetColor(color);
//                         break;
//                     case PlayfieldCell.State.DOOR:
//                         Color doorColor = new(1.0f, 0.0f, 1.0f);
//                         _gridDebugCubes[y, x].SetColor(doorColor);
//                         break;

//                 }
                
//                 if (ShowPathFinding)
//                 {
//                     if (_start != null)
//                     {
//                         if (_start.GetGridPosition() == new Vector2Int(x, y))
//                         {
//                             _gridDebugCubes[y, x].SetColor(Color.cyan);
//                         }
//                     }

//                     if (_end != null)
//                     {
//                         if (_end.GetGridPosition() == new Vector2Int(x, y))
//                         {
//                             _gridDebugCubes[y, x].SetColor(Color.magenta);
//                         }
//                     }
//                 }
//             }
//         }
//     }

//     public void SetPaths(PlayfieldCell start, PlayfieldCell end)
//     {
//         _start = start;
//         _end = end;
//     }

//     private void UpdatePathFindingDebugCubes()
//     {
//         if (_start == null || _end == null)
//         {
//             return;
//         }

//         foreach (var v in _visitedCells)
//         {
//             var visitedCellGrid = v.GetGridPosition();
//             if (_start.GetGridPosition() != v.GetGridPosition())
//             {
//                 _gridDebugCubes[visitedCellGrid.y, visitedCellGrid.x].SetColor(Color.red);
//             }

//             _gridDebugCubes[visitedCellGrid.y, visitedCellGrid.x].UpdateScores(
//                 v.GetGCost(), v.GetHCost(), v.GetFCost()
//             );
//         }

//         foreach (var p in _potentialCells)
//         {
//             var potentialCellGrid = p.GetGridPosition();
//             if (_start.GetGridPosition() != p.GetGridPosition())
//             {
//                 _gridDebugCubes[potentialCellGrid.y, potentialCellGrid.x].SetColor(Color.green);
//             }

//             _gridDebugCubes[potentialCellGrid.y, potentialCellGrid.x].UpdateScores(
//                 p.GetGCost(), p.GetHCost(), p.GetFCost()
//             );
//         }

//         foreach (var bt in _backtrackedCells)
//         {
//             var backtrackedCellGrid = bt.GetGridPosition();
//             _gridDebugCubes[backtrackedCellGrid.y, backtrackedCellGrid.x].SetColor(Color.blue);
//             _gridDebugCubes[backtrackedCellGrid.y, backtrackedCellGrid.x].UpdateScores(
//                 bt.GetGCost(), bt.GetHCost(), bt.GetFCost()
//             );
//         }
//     }

//     public void IntstantiateTestObjectsOnGrid(PlayfieldCell[,] gridMap, Vector2 cellSize)
//     {   
//         _gridSize = new Vector2Int(gridMap.GetLength(1), gridMap.GetLength(0));
//         _gridMap = gridMap;

//         _gridDebugCubes = new DebugCube[_gridSize.y, _gridSize.x];

//         for(int y = 0; y < _gridSize.y; y++)
//         {
//             for (int x = 0; x < _gridSize.x; x++)
//             {
//                 var CellPos2D = _gridMap[y, x].GetCellPos();
//                 var CellPos = new Vector3(CellPos2D.x, 0f, CellPos2D.y);
//                 GameObject debugCube = Instantiate(TestObject, CellPos, Quaternion.identity, gameObject.transform);
            
//                 _gridDebugCubes[y, x] = debugCube.GetComponent<DebugCube>();
//                 _gridDebugCubes[y, x].ClearText();
//                 _gridDebugCubes[y, x].SetGridPos(new Vector2Int(x, y));
//             }
//         }
//     }


//     public void SetVisitedCells(List<AStarAlgoCell> cellList)
//     {
//         _visitedCells = cellList;
//     }

//     public void SetPotentialCells(List<AStarAlgoCell> cellList)
//     {
//         _potentialCells = cellList;
//     }

//     public void SetBacktrackedCells(List<AStarAlgoCell> cellList)
//     {
//         _backtrackedCells = cellList;
//     }
        
// }
