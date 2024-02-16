// using System.Collections.Generic;
// using UnityEngine;

// public class GridMap : MonoBehaviour
// {
//     public Vector2Int MaxGridSize;
//     public Vector3 CellSize;
//     public int PaddingSize;
//     public List<GameObject> Rooms;
//     public int NumberOfRoomsToSpawn;

//     private List<GameObject> _ItemsToSpawn = new();
//     private List<GridAssetMap> _RoomData = new(); 
//     private GridCell[,] _grid;
//     private AStarAlgorithm _aStarAlgo = new();
//     System.Random _rand = new();

//     public void SpawnItems()
//     {
//         InitialiseGrid();

//         foreach (GameObject go in Rooms)
//         {
//             _RoomData.Add(go.GetComponent<GridAssetMap>());
//             print(_RoomData.Count);
//         }

//         List<(GridAssetMap, Vector2Int)> ViablePositions = new();
//         for (int y = 0; y < MaxGridSize.y; y++)
//         {
//             for (int x = 0; x < MaxGridSize.x; x++)
//             {
//                 int RandomIndex = _rand.Next(_RoomData.Count);
//                 var GridPos = new Vector2Int(x, y);
//                 if (CanPlaceAsset(_RoomData[RandomIndex], GridPos))
//                 {
//                     ViablePositions.Add((_RoomData[RandomIndex], GridPos));
//                 }
//             }
//         }

//         List<(GridAssetMap, Vector2Int)> ItemsToSpawn = new();
//         if (ViablePositions.Count < NumberOfRoomsToSpawn)
//         {
//             ItemsToSpawn = ViablePositions;
//         }
//         else
//         {   
//             for(int i = 0; i < NumberOfRoomsToSpawn; i++)
//             {
//                 int randIndex = _rand.Next(ViablePositions.Count);
//                 ItemsToSpawn.Add(ViablePositions[randIndex]);
//                 ViablePositions.RemoveAt(randIndex);
//             }
//         }

//         foreach (var its in ItemsToSpawn)
//         {
//             GameObject outputGameObject = PlaceAsset(its.Item1, its.Item2);
//             if (outputGameObject != null)
//             {
//                 _ItemsToSpawn.Add(outputGameObject);
//             }
//         }

//         _aStarAlgo.SetMap(_grid);
//     }

//     public List<Vector2> GetSpawnedObjectsPosition()
//     {
//         List<Vector2> SpawnedObjectPos = new();
//         foreach (GameObject go in _ItemsToSpawn)
//         {
//             GridAssetMap currentGam = go.GetComponent<GridAssetMap>();
//             Vector3 realWorldPos = currentGam.GetCenteredWorldPosition();
        
//             SpawnedObjectPos.Add(new Vector2(
//                 realWorldPos.x,
//                 realWorldPos.z
//             ));
//         }

//         return SpawnedObjectPos;
//     }

//     public (Vector2, Vector2) GetBoundariesFromPointList()
//     {
//         List<Vector2> points = GetSpawnedObjectsPosition();

//         Vector2 MinPosition = new(float.MaxValue, float.MaxValue);
//         Vector2 MaxPosition = new(float.MinValue, float.MinValue);

//         foreach (Vector2 p in points)
//         {
//             if (p.x < MinPosition.x)
//             {
//                 MinPosition.x = p.x;
//             }

//             if (p.y < MinPosition.y)
//             {
//                 MinPosition.y = p.y;
//             }

//             if (p.x > MaxPosition.x)
//             {
//                 MaxPosition.x = p.x;
//             }

//             if (p.y > MaxPosition.y)
//             {
//                 MaxPosition.y = p.y;
//             }
//         }

//         return (MinPosition, MaxPosition);
//     }

//     public GridAssetMap GetClosestGridmap(Vector2 position)
//     {   

//         Vector3 newPos = new(position.x, 0f, position.y);
//         float distance = float.MaxValue;
//         GridAssetMap lowestGridMap = null;
//         foreach(var gam in _RoomData)
//         {
//             if (Vector3.Distance(gam.GetCenteredWorldPosition(), newPos) < distance)
//             {
//                 lowestGridMap = gam;
//             }
//         }

//         return lowestGridMap;
//     }

//     public Vector2Int ToGridPosition(Vector2 v)
//     {
//         return new(
//             (int)((v.x / CellSize.x) + (0.5f)),
//             (int)((v.y / CellSize.z) + (0.5f))
//         );
//     }

//     public List<Vector3> SolveConnections(GraphStruct2D graph)
//     {
//         List<Vector2Int> listOfPoints = new();
//          List<Vector3> points = new();
//         foreach(Edge2DType e in graph.GetEdgeList())
//         {
//             // Get the tiles that are the closest to this
//             var EdgeNodes = e.GetNodes();

//             Vector2Int startNodeDoorPos = ToGridPosition(EdgeNodes.Item1.Position);
//             Vector2Int endNodeDoorPos = ToGridPosition(EdgeNodes.Item2.Position);

//             print(startNodeDoorPos);
//             print(endNodeDoorPos);

//             listOfPoints.Add(startNodeDoorPos);
//             listOfPoints.Add(endNodeDoorPos);

//             List<Vector2Int> shortestPath = _aStarAlgo.Solve(startNodeDoorPos, endNodeDoorPos);
//             print("----");

//             // print(shortestPath.Count);
//             // print("----");
            
//             foreach (var cell in shortestPath)
//             {
//                 points.Add(new Vector3(
//                     (float)cell.x * CellSize.x,
//                     0f,
//                     (float)cell.y * CellSize.z
//                 ));

//                 _grid[cell.x, cell.y].AddHallway();
//             }
//         }

       

        
//         foreach (var p in listOfPoints)
//         {
//             points.Add(new Vector3(
//                 (float)p.x * CellSize.x,
//                 0f,
//                 (float)p.y * CellSize.z
//             ));
//         }

//         return points;
//     }


//     private void InitialiseGrid()
//     {
//         _grid = new GridCell[MaxGridSize.x, MaxGridSize.y];

//         for (int y = 0; y < MaxGridSize.y; y++)
//         {
//             for (int x = 0; x < MaxGridSize.x; x++)
//             {
//                 _grid[x,y] = new GridCell();
//             }
//         }

//     }

//     private bool CanPlaceAsset(GridAssetMap grid, Vector2Int position)
//     {
//         if (((position.x - PaddingSize) < 0 || ((position.x + grid.GridSize.x) + PaddingSize) > MaxGridSize.x) ||
//             ((position.y  - PaddingSize) < 0 || ((position.y + grid.GridSize.z) + PaddingSize) > MaxGridSize.y))
//         {
//             return false;
//         }

//         for (int y = position.y - PaddingSize; y < (position.y + grid.GridSize.z) + PaddingSize; y++)
//         {
//             for (int x = position.x - PaddingSize; x < (position.x + grid.GridSize.x) + PaddingSize; x++)
//             {
//                 if (_grid[x, y].IsObstacle())
//                 {
//                     return false;
//                 }
//             }
//         }

//         return true;
//     }

//     private GameObject PlaceAsset(GridAssetMap grid, Vector2Int position)
//     {
//         if (!CanPlaceAsset(grid, position)) return null;

//         for (int y = position.y - PaddingSize; y < (position.y + grid.GridSize.z) + PaddingSize; y++)
//         {
//             for (int x = position.x - PaddingSize; x < (position.x + grid.GridSize.x) + PaddingSize; x++)
//             {
//                 if ((y >= position.y && y < (position.y + grid.GridSize.z)) ||
//                     (x >= position.x && y < (position.y + grid.GridSize.z)))
//                 {
//                     _grid[x, y]._currentState = GridCell.State.OCCUPIED;
//                 }
//                 else
//                 {
//                     _grid[x, y]._currentState = GridCell.State.PADDING;
//                 }
//             }
//         }

//         Vector3 ObjectTransform = new(
//             (float)position.x * grid.CellSize.x, 
//             0f,
//             (float)position.y * grid.CellSize.z
//         );
//         GameObject itemToSpawn = Instantiate(grid.gameObject, ObjectTransform, grid.gameObject.transform.rotation, gameObject.transform);

//         return itemToSpawn;
//     }
    
// }
