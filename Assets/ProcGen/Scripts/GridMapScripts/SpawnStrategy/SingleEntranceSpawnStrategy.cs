using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEntranceSpawnStrategy : MonoBehaviour
{
    public struct Config
    {
        public Vector2Int Direction;
        public Vector3Int EntryPosition;
        private Vector3Int GridSize;

        public Config(Vector2Int dir, int entryPos, Vector3Int gridSize)
        {
            Direction = dir;
            GridSize = gridSize;

            var gridSize2D = new Vector2Int(gridSize.x - 1, gridSize.z - 1);
            var absDirection = new Vector2Int((int)Mathf.Abs(dir.x), (int)Mathf.Abs(dir.y));
            var absFlippedDirection = new Vector2Int((int)Mathf.Abs(dir.y), (int)Mathf.Abs(dir.x));

            var calculatedWallEntrace = Vector2Int.zero;
            
            if (dir.x == -1 || dir.y == -1)
            {
                calculatedWallEntrace = Vector2Int.Scale(gridSize2D, absDirection);
            }
            
            var calculatedEntryPoint = (absFlippedDirection * entryPos) + calculatedWallEntrace;
            EntryPosition = new Vector3Int(calculatedEntryPoint.x, 0, calculatedEntryPoint.y);
        }

        public Config(Vector2Int dir, Vector3Int entryPos, Vector3Int gridSize)
        {
            Direction = dir;
            EntryPosition = entryPos;
            GridSize = gridSize;
        }

        public readonly Vector3Int GetEntryPosition()
        {
            return EntryPosition;
        }

        public Vector3Int GetEntryPositionOpposite()
        {
            var gridSize2D = new Vector2Int(GridSize.x - 1, GridSize.z - 1);
            var absDirection = new Vector2Int((int)Mathf.Abs(Direction.x), (int)Mathf.Abs(Direction.y));
            var absFlippedDirection = new Vector2Int((int)Mathf.Abs(Direction.y), (int)Mathf.Abs(Direction.x));

            var calculatedWallEntrace = Vector2Int.zero;
            
            if (Direction.x == 1 || Direction.y == 1)
            {
                calculatedWallEntrace = Vector2Int.Scale(gridSize2D, absDirection);
            }
            
            var calculatedEntryPoint = (absFlippedDirection * new Vector2Int(EntryPosition.x, EntryPosition.z)) + calculatedWallEntrace;
            return new Vector3Int(calculatedEntryPoint.x, 0, calculatedEntryPoint.y);
        }

        public readonly bool IsWithinBounds(Vector3Int gridSize)
        {
            var entryPos = GetEntryPosition();

            return !(entryPos.x < 0 || entryPos.x >= gridSize.x ||
                     entryPos.y < 0 || entryPos.y >= gridSize.y ||
                     entryPos.z < 0 || entryPos.z >= gridSize.z);
        }

        public Vector2Int GetFlippedAbsoluteDirection()
        {
            return new Vector2Int((int)Mathf.Abs(Direction.y), (int)Mathf.Abs(Direction.x));
        }

        public Vector2Int GetAbsoluteDirection()
        {
            return new Vector2Int((int)Mathf.Abs(Direction.x), (int)Mathf.Abs(Direction.y));
        }
    }

    private class StrategyCell
    {
        public enum CellType
        {
            FREE, ROOM, HALLWAY, ENTRANCE, ROOM_ENTRANCE
        }

        private CellType _Type;
        public StrategyCell()
        {
            _Type = CellType.FREE;
        }

        public bool IsFree()    { return _Type == CellType.FREE; }
        public bool IsHallway() { return _Type == CellType.HALLWAY; }
        public bool IsRoom()    { return _Type == CellType.ROOM; }
        public bool IsEntrance() { return _Type == CellType.ENTRANCE; }
        public bool IsRoomEntrance() { return _Type == CellType.ROOM_ENTRANCE; }

        public void SetToHallway()
        {
            _Type = CellType.HALLWAY;
        }

        public void SetToEntrance()
        {
            _Type = CellType.ENTRANCE;
        }

        public void SetToRoomEntrace()
        {
            _Type = CellType.ROOM_ENTRANCE;
        }
    }

    public GameObject HallwaySpawner;
    public GameObject RoomSpawner;

    public List<GameObject> _instantiatedRoomSpawners;
    public GameObject _instantiateHallwaySpawner;
    
    private Config  _entryConfig;
    private int _minSize;
    private int _maxSize;

    private GridMap<StrategyCell> _spawnStrategyMap;
    private bool _isDebugMode = false;
    private bool _isInitialised = false;
    public bool _isCompleted = false;


    

    public void Initialise(Vector3Int gridSize, Vector3 cellSize, int minSize, int maxSize, Vector2Int dir, int entryPos)
    {
        _entryConfig = new Config(dir, entryPos, gridSize);
        if(!_entryConfig.IsWithinBounds(gridSize))
        {
            throw new ArgumentException("Configuration is incorrect. Entry Point is Out of Bounds");
        }

        if (_isCompleted)
        {
            foreach(var rs in _instantiatedRoomSpawners)
            {
                Destroy(rs);
            }

            Destroy(_instantiateHallwaySpawner);
            _instantiatedRoomSpawners = new();
        }
        
        _maxSize = maxSize;
        _minSize = minSize;

        _spawnStrategyMap = new(gridSize, cellSize, transform.position, () => { return new StrategyCell(); });

        var EntryDoorPos = _entryConfig.GetEntryPosition();
        var entryCell = _spawnStrategyMap.GetCell(EntryDoorPos.x, EntryDoorPos.y, EntryDoorPos.z);
        entryCell.SetToEntrance();
        _spawnStrategyMap.SetCell(EntryDoorPos.x, EntryDoorPos.y, EntryDoorPos.z, entryCell);

        _isInitialised = true;
        _isCompleted = false;
        
    }

    public void SetDebugMode(bool debugMode)
    {
        _isDebugMode = debugMode;
    }

    public bool Completed()
    {
        return _isCompleted;
    }


    void Update()
    {
        if (_isInitialised && !_isCompleted)
        {
            List<(Vector2Int, Vector2Int, List<FixedRoomSpawnerManager.Connection>)> roomConfig = new();
            List<(Vector3Int, Vector3Int)> hallwayEdges = new();

            // get distance of entry point to the end
            var currentGridSize = _spawnStrategyMap.GetGridSize();
            var cuurentCellPos = _entryConfig.GetEntryPosition();
            var oppositeEntryPos = _entryConfig.GetEntryPositionOpposite();
            var currentMaxBound = (int)Vector3Int.Distance(cuurentCellPos, oppositeEntryPos);

            

            while (GetRandomDistance(currentMaxBound, out int dist))
            {
                var entryDir = _entryConfig.Direction;
                var endPosition = cuurentCellPos + new Vector3Int(entryDir.x * dist, 0, entryDir.y * dist);

                var entryCell = _spawnStrategyMap.GetCell(endPosition.x, endPosition.y, endPosition.z);
                entryCell.SetToHallway();
                _spawnStrategyMap.SetCell(endPosition.x, endPosition.y, endPosition.z, entryCell);

                // To the left side
                var flippedAbsDir = _entryConfig.GetFlippedAbsoluteDirection();
                Config firstHallwayConfig =  new(_entryConfig.Direction, cuurentCellPos, currentGridSize);
                Config secondHallwayConfig = new(flippedAbsDir, endPosition, currentGridSize);
                var (roomAnchorA, roomSizeA, secondHallwayPos) = GetRoomAnchorAndSizeFromConfig(firstHallwayConfig, secondHallwayConfig);

                List<FixedRoomSpawnerManager.Connection> connectionList = GetListOfPossibleConnections(firstHallwayConfig, secondHallwayConfig);
                roomConfig.Add((roomAnchorA, roomSizeA, connectionList));

                hallwayEdges.Add((firstHallwayConfig.GetEntryPosition(), secondHallwayConfig.GetEntryPosition()));
                hallwayEdges.Add((secondHallwayConfig.GetEntryPosition(), secondHallwayPos));
                
                var secondHallwayEndCell = _spawnStrategyMap.GetCell(secondHallwayPos.x, secondHallwayPos.y, secondHallwayPos.z);
                secondHallwayEndCell.SetToHallway();
                _spawnStrategyMap.SetCell(secondHallwayPos.x, secondHallwayPos.y, secondHallwayPos.z, secondHallwayEndCell);

                cuurentCellPos = endPosition;
                cuurentCellPos += new Vector3Int(entryDir.x, 0, entryDir.y);
                currentMaxBound = (int)Vector3Int.Distance(cuurentCellPos, oppositeEntryPos);
            }



            currentGridSize = _spawnStrategyMap.GetGridSize();
            cuurentCellPos = _entryConfig.GetEntryPosition();
            oppositeEntryPos = _entryConfig.GetEntryPositionOpposite();
            currentMaxBound = (int)Vector3Int.Distance(cuurentCellPos, oppositeEntryPos);

            while (GetRandomDistance(currentMaxBound, out int dist))
            {
                var entryDir = _entryConfig.Direction;
                var endPosition = cuurentCellPos + new Vector3Int(entryDir.x * dist, 0, entryDir.y * dist);

                var entryCell = _spawnStrategyMap.GetCell(endPosition.x, endPosition.y, endPosition.z);
                entryCell.SetToHallway();
                _spawnStrategyMap.SetCell(endPosition.x, endPosition.y, endPosition.z, entryCell);

                // to the right side
                var inverseDir = _entryConfig.GetFlippedAbsoluteDirection() * -1;
                Config firstHallwayConfig =  new(_entryConfig.Direction, cuurentCellPos, currentGridSize);
                Config thirdHallwayConfig = new(inverseDir, endPosition, currentGridSize);
                var (roomAnchorB, roomSizeB, thirdHallwayPos) = GetRoomAnchorAndSizeFromConfig(firstHallwayConfig, thirdHallwayConfig);

                hallwayEdges.Add((firstHallwayConfig.GetEntryPosition(), thirdHallwayConfig.GetEntryPosition()));
                hallwayEdges.Add((thirdHallwayConfig.GetEntryPosition(), thirdHallwayPos));

                List<FixedRoomSpawnerManager.Connection> connectionList = GetListOfPossibleConnections(firstHallwayConfig, thirdHallwayConfig);
                roomConfig.Add((roomAnchorB, roomSizeB, connectionList));
                
                var thirdHallwayEndCell = _spawnStrategyMap.GetCell(thirdHallwayPos.x, thirdHallwayPos.y, thirdHallwayPos.z);
                thirdHallwayEndCell.SetToHallway();
                _spawnStrategyMap.SetCell(thirdHallwayPos.x, thirdHallwayPos.y, thirdHallwayPos.z, thirdHallwayEndCell);

                cuurentCellPos = endPosition;
                cuurentCellPos += new Vector3Int(entryDir.x, 0, entryDir.y);
                currentMaxBound = (int)Vector3Int.Distance(cuurentCellPos, oppositeEntryPos);
            }
            

            // Use bst to get room configs
            foreach(var (anchor, size, entryPoints) in roomConfig)
            {
                Vector3 roomManagerSpawnPoint = _spawnStrategyMap.GetWorldPosition(anchor.x, 0, anchor.y);
                Vector3Int roomGridSize = new(size.x, 1, size.y); 
                GameObject instantiatedSpawner = Instantiate(RoomSpawner, roomManagerSpawnPoint, Quaternion.identity, transform);

                var roomManager = instantiatedSpawner.GetComponent<FixedRoomSpawnerManager>();
                
                roomManager.Initialise(roomGridSize, _spawnStrategyMap.GetCellSize());

                BSPTree bspTree = new(size, new Vector2Int(0, 0), _minSize);
                var listOfSubRooms = bspTree.GetAllData();
                
                foreach(var lsr in listOfSubRooms)
                {
                    Vector3Int subRoomAnchor = new(lsr.GetAnchor().x, 0, lsr.GetAnchor().y);
                    Vector3Int subRoomSize = new(lsr.GetSize().x, 1, lsr.GetSize().y);

                    roomManager.AddNewRoom(subRoomAnchor, subRoomSize);
                }

                roomManager.SpawnRooms();

                foreach(var conn in entryPoints)
                {
                    var doorEntry = roomManager.PickRandomDoorConnection(conn);
                    doorEntry.x += anchor.x;
                    doorEntry.z += anchor.y;

                    var cell = _spawnStrategyMap.GetCell(doorEntry.x, doorEntry.y, doorEntry.z);
                    cell.SetToRoomEntrace();
                    _spawnStrategyMap.SetCell(doorEntry.x, doorEntry.y, doorEntry.z, cell);
                }

                roomManager.SpawnDoors();

                _instantiatedRoomSpawners.Add(instantiatedSpawner);
            }

            foreach(var (hallwayPointA, hallwayPointB) in hallwayEdges)
            {
                Vector2Int minPoint = new(
                    (int)Mathf.Min(hallwayPointA.x, hallwayPointB.x),
                    (int)Mathf.Min(hallwayPointA.z, hallwayPointB.z)
                );

                Vector2Int maxPoint = new(
                    (int)Mathf.Max(hallwayPointA.x, hallwayPointB.x),
                    (int)Mathf.Max(hallwayPointA.z, hallwayPointB.z)
                );

                for(int y = minPoint.y; y < maxPoint.y + 1; y++)
                {
                    for(int x = minPoint.x; x < maxPoint.x + 1; x++)
                    {
                        var cell = _spawnStrategyMap.GetCell(x, 0, y);
                        if (!cell.IsEntrance())
                        {
                            cell.SetToHallway();
                            _spawnStrategyMap.SetCell(x, 0, y, cell);
                        }
                    }
                }
            }


            GameObject instantiatedHallwaySpawner = Instantiate(HallwaySpawner, transform.position, Quaternion.identity, transform);
            var hallwayManager = instantiatedHallwaySpawner.GetComponent<HallwaySpawnerManager>();
            hallwayManager.Initialise(currentGridSize, _spawnStrategyMap.GetCellSize(), 0);

            for(int y = 0; y < currentGridSize.y; y++)
            {
                for(int z = 0; z < currentGridSize.z; z++)
                {
                    for(int x = 0; x < currentGridSize.x; x++)
                    {
                        var cell = _spawnStrategyMap.GetCell(x, y, z);

                        if (cell.IsHallway() || cell.IsEntrance())
                        {
                            HallwayData.Config hallwayCfg = new(false, false, false, false);
                            List<(StrategyCell, Vector3Int)> surroundDoors = new(){
                                (_spawnStrategyMap.GetCell(x + 1, y, z), new Vector3Int(x + 1, y, z)),
                                (_spawnStrategyMap.GetCell(x - 1, y, z), new Vector3Int(x - 1, y, z)),
                                (_spawnStrategyMap.GetCell(x, y, z + 1), new Vector3Int(x, y, z + 1)),
                                (_spawnStrategyMap.GetCell(x, y, z - 1), new Vector3Int(x, y, z - 1))
                            };

                            List<Vector3Int> doorDirs = new();
                            foreach(var (cellOuter, cellPosOuter) in surroundDoors)
                            {
                                if (cellPosOuter.x < 0 || cellPosOuter.x >= currentGridSize.x ||
                                    cellPosOuter.y < 0 || cellPosOuter.y >= currentGridSize.y ||
                                    cellPosOuter.z < 0 || cellPosOuter.z >= currentGridSize.z)
                                {
                                    if (cell.IsEntrance())
                                    {
                                        doorDirs.Add(new Vector3Int(x, y, z) - cellPosOuter);
                                    }
                                    continue;
                                }

                                if (cellOuter != null)
                                {
                                    if (cellOuter.IsHallway() || cellOuter.IsRoomEntrance() || cellOuter.IsEntrance())
                                    {
                                        doorDirs.Add(new Vector3Int(x, y, z) - cellPosOuter);
                                    }
                                }
                            }

                            foreach(var currentDir in doorDirs)
                            {
                                if (currentDir.x == -1)
                                {
                                    hallwayCfg.PosX = true;
                                }
                                else if (currentDir.x == 1)
                                {
                                    hallwayCfg.NegX = true;
                                }
                                else if (currentDir.z == -1)
                                {
                                    hallwayCfg.PosY = true;
                                }
                                else if (currentDir.z == 1)
                                {
                                    hallwayCfg.NegY = true;
                                }
                            }

                            hallwayManager.ConfigureHallway(x, z, hallwayCfg);
                            
                        }
                    }
                }
            }

            hallwayManager.SpawnHallways();
            _instantiateHallwaySpawner = instantiatedHallwaySpawner;
            _isCompleted = true;
        }

        DrawDebugLines();
    }

    void DrawDebugLines()
    {
        if (!_isInitialised && !_isCompleted && !_isDebugMode)
        {
            return;
        }

        var currentGridSize = _spawnStrategyMap.GetGridSize();
        for(int y = 0; y < currentGridSize.y; y++)
        {
            for(int z = 0; z < currentGridSize.z; z++)
            {
                for(int x = 0; x < currentGridSize.x; x++)
                {
                    var cell = _spawnStrategyMap.GetCell(x, y, z);
                    
                    if (cell.IsHallway())
                    {
                        _spawnStrategyMap.DrawCell(x, y, z, Color.green);
                    }

                    else if (cell.IsEntrance())
                    {
                        _spawnStrategyMap.DrawCell(x, y, z, Color.red);
                    }

                    else if (cell.IsRoomEntrance())
                    {
                         _spawnStrategyMap.DrawCell(x, y, z, Color.blue);
                    }
                    else
                    {  
                        _spawnStrategyMap.DrawCell(x, y, z, Color.gray);
                    }
                }
            }
        }
    }

    int GetMaximumAllowableDistance(int maxBound)
    {
        int selectedMaxValue = -1;

        for(int i = _maxSize; i >= _minSize; i /= 2)
        {
            if (maxBound >= i)
            {
                selectedMaxValue = i;
                break;
            }
        }

        return selectedMaxValue;
    }

    bool GetRandomDistance(int maxBound, out int distance)
    {
        distance = -1;
        if (GetMaximumAllowableDistance(maxBound) < _minSize)
        {
            return false;
        }

        List<int> possibleOptions = new();
        System.Random rand = new();
        for(int i = _maxSize; i >= _minSize; i /= 2)
        {
            if (maxBound >= i)
            {
                possibleOptions.Add(i);
            }
        }

        if (possibleOptions.Count == 0)
        {
            return false;
        }

        distance = possibleOptions[rand.Next(possibleOptions.Count)];
        return true;
    }

    (Vector2Int, Vector2Int, Vector3Int) GetRoomAnchorAndSizeFromConfig(Config firstConfig, Config secondConfig)
    {
        var firstPos = firstConfig.GetEntryPosition();
        var firstDir = firstConfig.Direction;

        var intersectionPos = secondConfig.GetEntryPosition();
        var intersectionDir = secondConfig.Direction;

        var firstDist = (int)Vector3Int.Distance(firstPos, intersectionPos);

        var maxBoundDist = (int)Vector3Int.Distance(intersectionPos, secondConfig.GetEntryPositionOpposite());
        var secondDist = GetMaximumAllowableDistance(maxBoundDist);
        var secondEndPos = intersectionPos + new Vector3Int(intersectionDir.x * secondDist, 0, intersectionDir.y * secondDist);

        var calculatedSize = (firstDist * firstConfig.GetAbsoluteDirection()) + (secondDist * secondConfig.GetAbsoluteDirection());
        var calculatedAnchor = new Vector2Int(
            (int)Mathf.Min(firstPos.x, secondEndPos.x),
            (int)Mathf.Min(firstPos.z, secondEndPos.z)
        );

        if (firstDir.x < 0 || firstDir.y < 0)
        {
            calculatedAnchor += (firstDir * -1);
        }

        if (intersectionDir.x > 0 || intersectionDir.y > 0)
        {
            calculatedAnchor += intersectionDir;
        }

        return (calculatedAnchor, calculatedSize, secondEndPos); 
    }


    List<FixedRoomSpawnerManager.Connection> GetListOfPossibleConnections(Config firstConfig, Config secondConfig)
    {
        List<FixedRoomSpawnerManager.Connection> connectionList = new();

        var firstDir = firstConfig.Direction;
        var secondDir = secondConfig.Direction;
        
        if (firstDir.x == 1)
        {
            if (secondDir.y == 1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.BOTTOM);
            }
            else if(secondDir.y == -1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.TOP);
            }

            connectionList.Add(FixedRoomSpawnerManager.Connection.RIGHT);
            if (_entryConfig.EntryPosition != firstConfig.EntryPosition)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.LEFT);
            }
        }

        if (firstDir.x == -1)
        {
            if (secondDir.y == 1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.BOTTOM);
            }
            else if(secondDir.y == -1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.TOP);
            }

            connectionList.Add(FixedRoomSpawnerManager.Connection.LEFT);
            if (_entryConfig.EntryPosition != firstConfig.EntryPosition)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.RIGHT);
            }
        }

        if (firstDir.y == 1)
        {
            if (secondDir.x == 1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.LEFT);
            }
            else if(secondDir.x == -1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.RIGHT);
            }

            connectionList.Add(FixedRoomSpawnerManager.Connection.TOP);
            if (_entryConfig.EntryPosition != firstConfig.EntryPosition)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.BOTTOM);
            }
        }

        if (firstDir.y == -1)
        {
            if (secondDir.x == 1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.LEFT);
            }
            else if(secondDir.x == -1)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.RIGHT);
            }

            connectionList.Add(FixedRoomSpawnerManager.Connection.BOTTOM);
            if (_entryConfig.EntryPosition != firstConfig.EntryPosition)
            {
                connectionList.Add(FixedRoomSpawnerManager.Connection.TOP);
            }
        }

        return connectionList;
    }

}
