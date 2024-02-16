// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;


// public class PlayfieldCell : BaseCell
// {
//     public enum State
//     {
//         FREE, FILLED, PADDING, HALLWAY, DOOR
//     }
//     private State _currentState;

//     public PlayfieldCell(Vector2 cellSize, int x, int y, Vector2 offset) : 
//         base(cellSize, x, y, offset)
//     {
//         _currentState = State.FREE;
//     }

//     public void ToggleObstacle()
//     {
//         if (_currentState == State.FREE)
//         {
//             _currentState = State.FILLED;
//         }
//         else
//         {
//             _currentState = State.FREE;
//         }
//     }

//     public void SetObstacle(bool isObstacle)
//     {
//         if (isObstacle)
//         {
//             _currentState = State.FILLED;
//         }
//         else
//         {
//             _currentState = State.FREE;
//         }
//     }

//     public void SetPadding()
//     {
//         _currentState = State.PADDING;
//     }

//     public void SetHallway()
//     {
//         if (_currentState != State.DOOR) _currentState = State.HALLWAY;
//     }

//     public void SetDoor()
//     {
//         _currentState = State.DOOR;
//     }

//     public State GetState()
//     {
//         return _currentState;
//     }
// }


// public class CuboidGridMap : MonoBehaviour
// {
//     public Vector2 CellSize;
//     public Vector2Int GridSize;
//     public int PaddingSize;
//     public int NumberOfRoomsToSpawn;
//     public List<GameObject> RoomPrefabs;
//     public GameObject StraightHallwayPrefab;
//     public GameObject CornerHallwayPrefab;
//     public GameObject FourWayHallwayPrefab;
//     public GameObject THallwayPrefab;

//     public bool UseLevelDebugger;
//     public GameObject LevelDebuggerGameObject;

//     private CuboidGridMapDebugger _levelDebugger;
//     private PlayfieldCell[,] _gridMap;
//     private Vector3 _offset;
//     private List<CellRoomBlock> _roomBlocks;
//     private List<GameObject> _spawnedInRooms = new();

//     private bool _pathFindingInit = false;
//     private PathFinding _pathFindingAlgo = new();
//     private System.Random _rand = new();
//     private Edge2DType _registeredEdge;

//     public void InitialiseGridMap()
//     {
//         _offset = new Vector3(((float)GridSize.x * CellSize.x) / 2f, 0f, ((float)GridSize.y * CellSize.y) / 2f);
//         transform.position = -_offset;
//         _gridMap = new PlayfieldCell[GridSize.y, GridSize.x];
//         InitialiseEmptyGridMap(out _gridMap);

//         _roomBlocks = new List<CellRoomBlock>();
//         foreach (var go in RoomPrefabs)
//         {   
//             _roomBlocks.Add(go.GetComponent<CellRoomBlock>());
//         }

//         SpawnRooms();

//         _levelDebugger = LevelDebuggerGameObject.GetComponent<CuboidGridMapDebugger>();
//         if (UseLevelDebugger && _levelDebugger != null)
//         {
//             _levelDebugger.IntstantiateTestObjectsOnGrid(_gridMap, CellSize);
//         }
//         else
//         {
//             UseLevelDebugger = false;
//         }
//     }

//     private CellRoomBlock GetRoomClosestToPoint(Vector2 point)
//     {
//         CellRoomBlock closestRoom = null;
//         Vector3 point3D = new(point.x, 0f, point.y);
//         // print("\t - point3D: " + point3D.ToString());
        
//         foreach(var spawnedRoom in _spawnedInRooms)
//         {
//             CellRoomBlock room  = spawnedRoom.GetComponent<CellRoomBlock>();
//             if (closestRoom == null)
//             {
//                 closestRoom = room;
//                 continue;
//             }

//             Vector3 roomPos = room.transform.position;
//             Vector3 closestRoomPos = closestRoom.transform.position;

//             // print("     > roomPos: " + roomPos.ToString());
//             // print("     > closestRoomPos: " + closestRoomPos.ToString());

//             if (Vector3.Distance(roomPos, point3D) < Vector3.Distance(closestRoomPos, point3D))
//             {
//                 closestRoom = room;
//             }
//         }

//         // print("  - closestRoom: " + closestRoom.transform.position.ToString());
//         return closestRoom;
//     }

//     public void SolvePath(Edge2DType edge)
//     {
//         RegisterPath(edge);
//         bool isSolving = true;

//         while(isSolving)
//         {
//             isSolving = SolvePathStep();
//         }
        
//         print(_pathFindingAlgo.GetBackTrackedCells().Count);
//         foreach (var bt in _pathFindingAlgo.GetBackTrackedCells())
//         {
//             var gridPos = bt.GetGridPosition();
//             _gridMap[gridPos.y, gridPos.x].SetHallway();
//         }

//         foreach (var room in _spawnedInRooms)
//         {
//             DoorHandler dh = room.GetComponent<DoorHandler>();
//             dh.CollapseAllToWall();
//         }
//     }

//     public void PlaceAllHallways()
//     {
//         for(int y = 0; y < GridSize.y; y++)
//         {
//             for(int x = 0; x < GridSize.x; x++)
//             {
//                 if (_gridMap[y, x].GetState() == PlayfieldCell.State.HALLWAY)
//                 {
//                     PlaceHallway(x, y);
//                 }
//             }
//         }
//     }

//     public void PlaceHallway(int x, int y)
//     {
//         Vector3 roomPosition = new Vector3(x * CellSize.x, 0f, y * CellSize.y) + gameObject.transform.position;
//         int numberOfConnections = 0;
        
//         Vector2Int twoWayConnectors = Vector2Int.zero;
//         Vector2Int reltwoWayConnectors = Vector2Int.zero;

        
//         if ((x + 1) >= 0 && (x + 1) < GridSize.x)
//         {
//             if (_gridMap[y, x + 1].GetState() == PlayfieldCell.State.HALLWAY || 
//                 _gridMap[y, x + 1].GetState() == PlayfieldCell.State.DOOR) 
            
//             {
//                 numberOfConnections++;
//                 twoWayConnectors.x += 1;
//                 reltwoWayConnectors.x += 1;
//             }
//         }

//         if ((x - 1) >= 0 && (x - 1) < GridSize.x)
//         {
//             if (_gridMap[y, x - 1].GetState() == PlayfieldCell.State.HALLWAY || 
//                 _gridMap[y, x - 1].GetState() == PlayfieldCell.State.DOOR) 
//             {
//                 numberOfConnections++;
//                 twoWayConnectors.x += 1;
//                 reltwoWayConnectors.x -= 1;
//             }
//         }

//         if ((y + 1) >= 0 && (y + 1) < GridSize.y)
//         {
//             if (_gridMap[y + 1, x].GetState() == PlayfieldCell.State.HALLWAY || 
//                 _gridMap[y + 1, x].GetState() == PlayfieldCell.State.DOOR) 
            
//             {
//                 numberOfConnections++;
//                 twoWayConnectors.y += 1;
//                 reltwoWayConnectors.y += 1;
//             }
//         }

//         if ((y - 1) >= 0 && (y - 1) < GridSize.y)
//         {
//             if (_gridMap[y - 1, x].GetState() == PlayfieldCell.State.HALLWAY || 
//                 _gridMap[y - 1, x].GetState() == PlayfieldCell.State.DOOR) 
            
//             {
//                 numberOfConnections++;
//                 twoWayConnectors.y += 1;
//                 reltwoWayConnectors.y -= 1;
//             }
//         }
        
//         GameObject prefabToUse = null;
//         print(numberOfConnections);
//         Quaternion prefabRotation = Quaternion.identity;
//         if (numberOfConnections == 4)
//         {
//             prefabToUse = FourWayHallwayPrefab;
//         }
//         else if (numberOfConnections == 3)
//         {
//             prefabToUse = THallwayPrefab;

//             if (reltwoWayConnectors == new Vector2Int(0, -1))
//             {
//                 prefabRotation *= Quaternion.Euler(0f, 180f, 0f);
//                 roomPosition += new Vector3(CellSize.x, 0f, CellSize.y);
//             }
//             else if (reltwoWayConnectors == new Vector2Int(1, 0))
//             {
//                 prefabRotation *= Quaternion.Euler(0f, 90f, 0f);
//                 roomPosition += new Vector3(0f, 0f, CellSize.y);
//             }

//             else if (reltwoWayConnectors == new Vector2Int(-1, 0))
//             {
//                 prefabRotation *= Quaternion.Euler(0f, -90f, 0f);
//                 roomPosition += new Vector3(CellSize.x, 0f, 0f);
//             }

//         }
//         else if (numberOfConnections == 2)
//         {      
//             print(twoWayConnectors);
//             if (twoWayConnectors.x == 2 || twoWayConnectors.y == 2)
//             {
//                 prefabToUse = StraightHallwayPrefab;
//                 if (twoWayConnectors.y == 0)
//                 {
//                     prefabRotation *= Quaternion.Euler(0f, 90f, 0f);
//                     roomPosition += new Vector3(0f, 0f, CellSize.y);
//                 }
                
//             }

//             if (twoWayConnectors.x == 1 || twoWayConnectors.y == 1)
//             {
//                 prefabToUse = CornerHallwayPrefab;

//                 if (reltwoWayConnectors == new Vector2Int(1, 1))
//                 {
//                     prefabRotation *= Quaternion.Euler(0f, 180f, 0f);
//                     roomPosition += new Vector3(CellSize.x, 0f, CellSize.y);
//                 }

//                 else if (reltwoWayConnectors == new Vector2Int(-1, 1))
//                 {
//                     prefabRotation *= Quaternion.Euler(0f, 90f, 0f);
//                     roomPosition += new Vector3(0f, 0f, CellSize.y);
//                 }

//                 else if (reltwoWayConnectors == new Vector2Int(1, -1))
//                 {
//                     prefabRotation *= Quaternion.Euler(0f, -90f, 0f);
//                     roomPosition += new Vector3(CellSize.x, 0f, 0f);
//                 }
//             }
//         }

//         Instantiate(
//             prefabToUse, roomPosition, prefabRotation, gameObject.transform.Find("GeneratedLevel").transform
//         );
//     }
    
//     public void RegisterPath(Edge2DType edge)
//     {
//         _registeredEdge = edge;

//         // convert edge to a tile map
//         var nodePos = _registeredEdge.GetNodePositions();

//         CellRoomBlock startRoom = GetRoomClosestToPoint(nodePos.Item1);
//         Vector2 closestStartRoomPos = startRoom.GetCenteredWorldPosition2D();

//         CellRoomBlock endRoom = GetRoomClosestToPoint(nodePos.Item2);
//         Vector2 closestEndRoomPos = endRoom.GetCenteredWorldPosition2D();
//         Vector2 MidPoint = (closestStartRoomPos + closestEndRoomPos) / 2f;

//         Vector2 closestStartDoorPos = startRoom.GetClosestDoorPosition(MidPoint);
//         Vector2 closestEndDoorPos = endRoom.GetClosestDoorPosition(MidPoint);
        
//         Vector2Int startNode = new(
//             (int)((closestStartDoorPos.x + _offset.x) / CellSize.x),
//             (int)((closestStartDoorPos.y + _offset.z) / CellSize.y)
//         );
//         print("srt: " + nodePos.Item1.ToString() + ", " + closestStartDoorPos.ToString() + ", " +  startNode.ToString());

        
//         Vector2Int endNode = new(
//             (int)((closestEndDoorPos.x + _offset.x) / CellSize.x),
//             (int)((closestEndDoorPos.y + _offset.z) / CellSize.y)
//         );

//         print("end: " + nodePos.Item2.ToString() + ", " + closestEndDoorPos.ToString() + ", " + endNode.ToString());

//         _gridMap[startNode.y, startNode.x].SetDoor();
//         _gridMap[endNode.y, endNode.x].SetDoor();

//         if (!_pathFindingInit)
//         {
//             _pathFindingAlgo.InitialiseMap(_gridMap);
//         }

//         if (UseLevelDebugger)
//         {
//             _levelDebugger.SetPaths(_gridMap[startNode.y, startNode.x], _gridMap[endNode.y, endNode.x]);
//         }

//         _pathFindingAlgo.StartFinder(startNode, endNode);
//     }

//     public Vector3 GetRandomRoomPosition()
//     {
//         int randomIndex = _rand.Next(_spawnedInRooms.Count);
//         CellRoomBlock crb = _spawnedInRooms[randomIndex].GetComponent<CellRoomBlock>();
    
//         return crb.GetCenteredWorldPosition();
//     }

//     public bool SolvePathStep()
//     {
//         _pathFindingAlgo.Step();

//         if (UseLevelDebugger)
//         {
//             _levelDebugger.SetVisitedCells(_pathFindingAlgo.GetVisitedCells());
//             _levelDebugger.SetPotentialCells(_pathFindingAlgo.GetPotentialCells());
//             _levelDebugger.SetBacktrackedCells(_pathFindingAlgo.GetBackTrackedCells());
//         }
        
//         return _pathFindingAlgo.GetSolveState() == PathFinding.SolveState.IN_PROGRESS;
//     }


//     public List<Vector2> GetSpawnedObjectsPosition()
//     {
//         List<Vector2> SpawnedObjectPos = new();
//         foreach (GameObject go in _spawnedInRooms)
//         {
//             CellRoomBlock currentGam = go.GetComponent<CellRoomBlock>();
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


//     private void InitialiseEmptyGridMap(out PlayfieldCell[,] gridMap)
//     {
//         var Offset2D = new Vector2(transform.position.x, transform.position.z);
//         gridMap = new PlayfieldCell[GridSize.y, GridSize.x];

//         for(int y = 0; y < GridSize.y; y++)
//         {
//             for (int x = 0; x < GridSize.x; x++)
//             {
//                 gridMap[y, x] = new PlayfieldCell(CellSize, x, y, Offset2D);
//             }
//         }
//     }

//     public void SpawnRooms()
//     {
//         PlayfieldCell[,] intermediaryGridMap = new PlayfieldCell[GridSize.y, GridSize.x];
//         var Offset2D = new Vector2(transform.position.x, transform.position.z);
//         intermediaryGridMap = new PlayfieldCell[GridSize.y, GridSize.x];

//         for(int y = 0; y < GridSize.y; y++)
//         {
//             for (int x = 0; x < GridSize.x; x++)
//             {
//                 intermediaryGridMap[y, x] = new PlayfieldCell(CellSize, x, y, Offset2D);
//             }
//         }

//         List<(CellRoomBlock, Vector2Int)> listOfPossibleRooms = new();

//         for(int y = 0; y < GridSize.y; y++)
//         {
//             for (int x = 0; x < GridSize.x; x++)
//             {
//                 int randomIndex = _rand.Next(_roomBlocks.Count);
//                 CellRoomBlock currentBlock = _roomBlocks[randomIndex];

//                 Vector2Int roomSize = currentBlock.GetSizeInGrid(CellSize);
//                 bool canSpawn = true;
//                 if (((x - PaddingSize) < 0 || (x + roomSize.x + PaddingSize - 1) >= GridSize.x) ||
//                     ((y - PaddingSize) < 0 || (y + roomSize.y + PaddingSize - 1) >= GridSize.y))
//                 {
//                     canSpawn = false;
//                 }

//                 if (canSpawn)
//                 {
//                     for (int roomSizeY = y - PaddingSize; roomSizeY < (y + roomSize.y + PaddingSize); roomSizeY++)
//                     {
//                         for (int roomSizeX = x - PaddingSize; roomSizeX < (x + roomSize.x + PaddingSize); roomSizeX++)
//                         {
//                             if (intermediaryGridMap[roomSizeY, roomSizeX].GetState() == PlayfieldCell.State.FILLED)
//                             {
//                                 if (canSpawn) canSpawn = false;
//                             }
//                         }
//                     }
//                 }

//                 if (canSpawn)
//                 {
//                     for (int yPos = y - PaddingSize; yPos < (y + roomSize.y + PaddingSize); yPos++)
//                     {
//                         for (int xPos = x - PaddingSize; xPos < (x + roomSize.x + PaddingSize); xPos++)
//                         {
                            
//                             intermediaryGridMap[yPos, xPos].SetObstacle(true);
//                         }
//                     }

//                     listOfPossibleRooms.Add((
//                         currentBlock, 
//                         new Vector2Int(x, y)
//                     ));
//                 }
//             }
//         }


//         List<(CellRoomBlock, Vector2Int)> roomsToSpawn = new();
//         if (listOfPossibleRooms.Count <= NumberOfRoomsToSpawn)
//         {
//             roomsToSpawn = listOfPossibleRooms;
//         }
//         else
//         {
//             for(int i = 0; i < NumberOfRoomsToSpawn; i++)
//             {
//                 int randomPossibleIndex = _rand.Next(listOfPossibleRooms.Count);
//                 roomsToSpawn.Add(listOfPossibleRooms[randomPossibleIndex]);
//                 listOfPossibleRooms.RemoveAt(randomPossibleIndex);
//             }
//         }

//         // Spawn rooms
//         foreach(var r in roomsToSpawn)
//         {
//             GameObject gm = r.Item1.gameObject;
//             Vector3 roomPosition = new Vector3(r.Item2.x * CellSize.x, 0f, r.Item2.y * CellSize.y) + gameObject.transform.position;
//             Vector2Int roomSize = r.Item1.GetSizeInGrid(CellSize);

//             for (int y = r.Item2.y - PaddingSize; y < (r.Item2.y + roomSize.y + PaddingSize); y++)
//             {
//                 for (int x = r.Item2.x - PaddingSize; x < (r.Item2.x + roomSize.x + PaddingSize); x++)
//                 {
//                     if ((y >= r.Item2.y && y < r.Item2.y + roomSize.y) && 
//                         (x >= r.Item2.x && x < r.Item2.x + roomSize.x ) )
//                     {
//                        _gridMap[y, x].SetObstacle(true);
//                     }
//                     else
//                     {
//                         _gridMap[y, x].SetPadding();
//                     }
//                 }
//             }
            
//             _spawnedInRooms.Add(Instantiate(
//                 gm, roomPosition, Quaternion.identity, gameObject.transform.Find("GeneratedLevel").transform
//             ));
//         }
//     }

//     private bool CanRoomBeSpawnedIn(Vector2Int roomSize, int yPos, int xPos)
//     {
//         if (((yPos - PaddingSize) < 0 || (yPos + roomSize.y + PaddingSize) >= GridSize.y) &&
//             ((xPos - PaddingSize) < 0 || (xPos + roomSize.x + PaddingSize) >= GridSize.x))
//         {
//             return false;
//         }

//         for (int y = yPos - PaddingSize; y < (roomSize.y + PaddingSize); y++)
//         {
//             for (int x = xPos - PaddingSize; x < (roomSize.x + PaddingSize); x++)
//             {
//                 if (_gridMap[y, x].GetState() == PlayfieldCell.State.FILLED)
//                 {
//                     return false;
//                 }
//             }
//         }

//         return true;
//     }


//     private bool CanRoomBeSpawnedInGridMap(Vector2Int roomSize, int yPos, int xPos, PlayfieldCell[,] gridMap)
//     {
//         if (((yPos - PaddingSize) < 0 || (yPos + roomSize.y + PaddingSize) >= GridSize.y) ||
//             ((xPos - PaddingSize) < 0 || (xPos + roomSize.x + PaddingSize) >= GridSize.x))
//         {
//             return false;
//         }

//         for (int y = yPos - PaddingSize; y < (yPos + roomSize.y + PaddingSize - 1); y++)
//         {
//             for (int x = xPos - PaddingSize; x < (xPos + roomSize.x + PaddingSize - 1); x++)
//             {
//                 if (gridMap[y, x].GetState() == PlayfieldCell.State.FILLED)
//                 {
//                     return false;
//                 }
//             }
//         }


//         return true;
//     }
// }
