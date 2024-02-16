using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FixedRoomSpawnerManager : MonoBehaviour
{
    public enum Connection
    {
        TOP, LEFT, RIGHT, BOTTOM
    }

    private class FixedRoomCell
    {
        public enum RoomCellType
        {
            FREE, FILLED, DOOR_UNSET, DOOR
        }

        private RoomCellType _Type;
        private int _associatedDoorID;

        public FixedRoomCell()
        {
            _Type = RoomCellType.FREE;
            _associatedDoorID = -1;
        }

        public bool IsFree() { return _Type == RoomCellType.FREE; }
        public bool IsDoor() { return _Type == RoomCellType.DOOR; }
        public bool IsDoorUnset() { return _Type == RoomCellType.DOOR_UNSET; }
        public bool DoesIDMatch(int id) { return _associatedDoorID == id; }

        public void SetToFill() 
        { 
            _associatedDoorID = -1;
            _Type = RoomCellType.FILLED; 
        }
        
        public void SetToDoor(int doorID, bool setDoor = false) 
        { 
            _associatedDoorID = doorID;
            if (setDoor)
            {
                _Type = RoomCellType.DOOR;
                return;
            }

            _Type = RoomCellType.DOOR_UNSET;
        }

        public int GetDoorID()
        {
            return _associatedDoorID;
        }
    }


    private class FixedRoomConfig
    {
        public Vector3Int Anchor;
        public Vector3Int Size;

        private GameObject _prefabObject;
        private GameObject _instantiatedObject;
        private Vector3 _position;
        private Transform _parent;
        private int _id;

        public FixedRoomConfig(Vector3Int anchor, Vector3Int size)
        {
            Anchor = anchor;
            Size = size;

            _prefabObject = null;
            _instantiatedObject = null;

            _id = -1;
        }

        public bool IsWithinBounds(Vector3Int gridSize)
        {
            if (Anchor.x < 0 || Anchor.x > gridSize.x ||
                Anchor.y < 0 || Anchor.y > gridSize.y ||
                Anchor.z < 0 || Anchor.z > gridSize.z)
            {
                return false;
            }

            if ((Anchor.x + Size.x) < 0 || (Anchor.x + Size.x) > gridSize.x ||
                (Anchor.y + Size.y) < 0 || (Anchor.y + Size.y) > gridSize.y ||
                (Anchor.z + Size.z) < 0 || (Anchor.z + Size.z) > gridSize.z)
            {
                return false;
            }

            return true;
        }

        public void ConfigureRoom(int id, GameObject prefab, Vector3 position, Transform parent)
        {
            if (id < 0)
            {
                throw new ArgumentException("ID is less than zero. Must be a positive interger.");
            }

            _id = id;
            _prefabObject = prefab;
            _position = position;
            _parent = parent;
        }

        public void SpawnRoom()
        {
            if (_prefabObject == null || _id < 0)
            {
                return;
            }

            if (_instantiatedObject == null)
            {
                _instantiatedObject = Instantiate(_prefabObject, _position, Quaternion.identity, _parent);
            }
        }

        public void DestroyRoom()
        {
            if (_id < 0)
            {
                return;
            }

            if (_instantiatedObject != null)
            {
                Destroy(_instantiatedObject);
            }
        }

        public Vector2 GetCentre2D(Vector3 CellSize)
        {
            Vector3 anchor3D = Vector3.Scale((Vector3)Anchor, CellSize);
            Vector3 size3D = Vector3.Scale((Vector3)Size, CellSize) / 2f;

            return new Vector2(anchor3D.x + size3D.x, anchor3D.z + size3D.z);
        }

        public Vector2 GetCentre3D(Vector3 CellSize)
        {
            Vector3 anchor3D = Vector3.Scale((Vector3)Anchor, CellSize);
            Vector3 size3D = Vector3.Scale((Vector3)Size, CellSize) / 2f;

            return anchor3D + size3D;
        }

        public int GetID()
        {
            return _id;
        }

        public bool IsEqual(FixedRoomConfig config)
        {
            if (config.GetID() < 0 || _id < 0)
            {
                return false;
            }

            return config.GetID() == _id;
        }

        public bool IsEqual(int id)
        {
            if (id < 0 || _id < 0)
            {
                return false;
            }


            return id == _id;
        }
    }

    public List<GameObject> FixedRoomPrefabs;

    [Header("Door Prefabs")]
    public GameObject OpenDoor;
    public GameObject ClosedDoor;

    private GridMap<FixedRoomCell> _fixedRoomMap;
    private List<FixedRoomData> _FixedRoomDatas = new();
    private List<FixedRoomConfig> _roomsToSpawn = new();
    private bool _isInitalised = false;
    private bool _isSpawned = false; 

    private List<Color> _idColors;
    private List<(Vector2, Vector2)> _edgeList;
    private System.Random _randomEngine = new();
    private List<GameObject> _instantiatedDoors = new();

    public void Initialise(Vector3Int gridSize, Vector3 cellSize)
    {
        if (!_isInitalised)
        {
            foreach(var frp in FixedRoomPrefabs)
            {
                if (frp == null)
                {
                    Debug.LogError("Empty Game Object");
                    continue;
                }

                if (frp.TryGetComponent<FixedRoomData>(out var fixedRoomData))
                {
                    _FixedRoomDatas.Add(fixedRoomData);
                }
                else
                {
                    Debug.LogError("GameObject of name '" + frp.name.ToString() + "' does not have the HallwayData component configured.");
                }
            }
        }


        if (_isSpawned)
        {
            foreach(var room in _roomsToSpawn)
            {
                room.DestroyRoom();
            }
            _roomsToSpawn = new();

            foreach(var door in _instantiatedDoors)
            {
                Destroy(door);
            }

            _instantiatedDoors = new();
        }

        _fixedRoomMap = new GridMap<FixedRoomCell>(gridSize, cellSize, transform.position, () => { return new FixedRoomCell(); });
        _isInitalised = true;
        _isSpawned = false;

        _idColors = new();
        _edgeList = new();

    }

    public bool AddNewRoom(Vector3Int anchor, Vector3Int size)
    {
        if (!_isInitalised)
        {
            return false;
        }
        

        // check if anchor and size is within the initialised grid
        var currentGridSize = _fixedRoomMap.GetGridSize();
        var roomConfig = new FixedRoomConfig(anchor, size);

        if (!roomConfig.IsWithinBounds(currentGridSize))
        {
            Debug.LogWarning("[FixedRoomSpawnerManager] - Anchor " + anchor.ToString() + 
                                                        ", Size " + size.ToString() + 
                                                        ", GridSize " + currentGridSize.ToString() + 
                                                        ". Is not within the bounds");
            return false;
        }

        if (!IsAreaFree(roomConfig))
        {
            Debug.LogWarning("[FixedRoomSpawnerManager] - Anchor " + anchor.ToString() + 
                                                          ", Size " + size.ToString() +
                                                          ", GridSize " + currentGridSize.ToString() + 
                                                          ". Area Not Free.");
            return false;
        }

        List<FixedRoomData> PossibleRooms = new();
        foreach(var roomData in _FixedRoomDatas)
        {
            if (roomData.IsEqual(size))
            {
                PossibleRooms.Add(roomData);
            }
        }

        if (PossibleRooms.Count == 0)
        {
            return false;
        }

        FixedRoomData selectedRoom = PossibleRooms[_randomEngine.Next(PossibleRooms.Count)];
        AddRoomToMap(selectedRoom, roomConfig);

        return true;
    }

    public void SpawnRooms()
    {
        if (!_isInitalised && _isSpawned)
        {
            return;
        }

        // Get the list of centre positions in the room with their associated ids
        // and spawn the rooms

        
        
        for(int i = 0; i < _roomsToSpawn.Count; i++)
        {
            _idColors.Add(new Color(
                (float)_randomEngine.NextDouble(), 
                (float)_randomEngine.NextDouble(), 
                (float)_randomEngine.NextDouble())
            );
        }

        // Find the minimum and maximum values of the points
        List<(int, Vector2)> listOfRoomCentresWithIDs = new();
        foreach(var rts in _roomsToSpawn)
        {
            rts.SpawnRoom();
            listOfRoomCentresWithIDs.Add(
                (rts.GetID(),
                rts.GetCentre2D(_fixedRoomMap.GetCellSize()))
            );
        }

        if (_roomsToSpawn.Count > 1)
        {
            List<(int, int)> idPairs = new();
            
            var currentGridSize = _fixedRoomMap.GetGridSize();
            Graph2D.Graph connectedRooms = new();

            for(int y = 0; y < currentGridSize.y; y++)
            {
                for(int z = 0; z < currentGridSize.z; z++)
                {
                    for(int x = 0; x < currentGridSize.x; x++)
                    {
                        // Do horizontal check
                        var currentCell = _fixedRoomMap.GetCell(x, y, z);
                        var currentCellID = currentCell.GetDoorID();

                        if (x < (currentGridSize.x - 1))
                        {
                            FixedRoomCell horizAdjCell = _fixedRoomMap.GetCell(x + 1, y, z);
                            var adjCellID = horizAdjCell.GetDoorID();

                            if (currentCellID >= 0 && adjCellID >= 0 && currentCellID != adjCellID)
                            {   
                                Graph2D.Node pointA = null;
                                Graph2D.Node pointB = null;

                                foreach(var (id, centre) in listOfRoomCentresWithIDs)
                                {
                                    if (currentCellID == id)
                                    {
                                        pointA = new(centre);
                                    }

                                    if (adjCellID == id)
                                    {
                                        pointB = new(centre);
                                    }
                                }

                                connectedRooms.AddEdge(pointA, pointB);
                            }
                        }

                        // Do vertical check
                        if (z < (currentGridSize.z - 1))
                        {
                            FixedRoomCell vertAdjCell = _fixedRoomMap.GetCell(x, y, z + 1);
                            var adjCellID = vertAdjCell.GetDoorID();

                            if (currentCellID >= 0 && adjCellID >= 0 && currentCellID != adjCellID)
                            {   
                                Graph2D.Node pointA = null;
                                Graph2D.Node pointB = null;

                                foreach(var (id, centre) in listOfRoomCentresWithIDs)
                                {
                                    if (currentCellID == id)
                                    {
                                        pointA = new(centre);
                                    }

                                    if (adjCellID == id)
                                    {
                                        pointB = new(centre);
                                    }
                                }

                                connectedRooms.AddEdge(pointA, pointB);
                            }
                        }


                    }
                }
            }

            // put it through the min tree spanning algorithm
            MinimumSpanningTree mstAlgo = new();
            
            mstAlgo.TrimGraph(connectedRooms);
            Graph2D.Graph trimmedGraph = mstAlgo.GetTrimmedGraph();

            List<(Vector2, Vector2)> trimmedGraphEdges = trimmedGraph.GetEdgeListPositions();
            List<(Vector2, Vector2)> unusedEdges = mstAlgo.RandomPickUnusedEdgePositions(0.25f);
            
            _edgeList = trimmedGraphEdges.Concat(unusedEdges).ToList();

            foreach(var (edgeA, edgeB) in _edgeList)
            {
                (int, int) idPair = (-1, -1);
                (bool, bool) idPairFound = (false, false);

                foreach(var (id, centre) in listOfRoomCentresWithIDs)
                {
                    if (centre == edgeA)
                    {
                        idPair.Item1 = id;
                        idPairFound.Item1 = true;
                    }

                    if (centre == edgeB)
                    {
                        idPair.Item2 = id;
                        idPairFound.Item2 = true;
                    }

                    if (idPairFound.Item1 && idPairFound.Item2)
                    {
                        break;
                    }
                }

                if (idPairFound.Item1 && idPairFound.Item2 &&
                    idPair.Item1 >= 0 && idPair.Item2 >= 0)
                {
                    bool addToIdList = true;
                    foreach(var p in idPairs)
                    {
                        if ((p.Item1 == idPair.Item1 && p.Item2 == idPair.Item2) ||
                            (p.Item1 == idPair.Item2 && p.Item2 == idPair.Item1))
                        {
                            addToIdList = false;
                            break;
                        }
                    }

                    if (addToIdList)
                    {
                        idPairs.Add(idPair);
                    }
                }
            }


            Dictionary<(int, int), List<(Vector3Int, Vector3Int)>> idPairToDoorPos = new();
            foreach(var i in idPairs)
            {
                idPairToDoorPos.Add(i, new List<(Vector3Int, Vector3Int)>());
            }


            // Loop through the entire grid map and determine the coordinates of the pairs and set them to being set

            for(int y = 0; y < currentGridSize.y; y++)
            {
                for(int z = 0; z < currentGridSize.z; z++)
                {
                    for(int x = 0; x < currentGridSize.x; x++)
                    {
                        // Do horizontal check
                        var currentCell = _fixedRoomMap.GetCell(x, y, z);
                        FixedRoomCell horizAdjCell = null;
                        FixedRoomCell vertAdjCell = null; 


                        if (x < (currentGridSize.x - 1))
                        {
                            horizAdjCell = _fixedRoomMap.GetCell(x + 1, y, z);
                        }

                        // Do vertical check
                        if (z < (currentGridSize.z - 1))
                        {
                            vertAdjCell = _fixedRoomMap.GetCell(x, y, z + 1);
                        }

                        foreach(var idPair in idPairs)
                        {
                            if (horizAdjCell != null)
                            {
                                if ((currentCell.GetDoorID() == idPair.Item1 && horizAdjCell.GetDoorID() == idPair.Item2) || 
                                    (currentCell.GetDoorID() == idPair.Item2 && horizAdjCell.GetDoorID() == idPair.Item1))
                                {
                                    idPairToDoorPos[idPair].Add((new Vector3Int(x, y, z), new Vector3Int(x + 1, y, z)));
                                }
                            }

                            if (vertAdjCell != null)
                            {
                                if ((currentCell.GetDoorID() == idPair.Item1 && vertAdjCell.GetDoorID() == idPair.Item2) || 
                                    (currentCell.GetDoorID() == idPair.Item2 && vertAdjCell.GetDoorID() == idPair.Item1))
                                {
                                    idPairToDoorPos[idPair].Add((new Vector3Int(x, y, z), new Vector3Int(x, y, z + 1)));
                                }
                            }
                        }
                    }
                }
            }

            foreach(var pair in idPairToDoorPos)
            {
                if (pair.Value.Count == 0)
                {
                    continue;
                }

                var (doorA, doorB) = pair.Value[_randomEngine.Next(pair.Value.Count)];

                var cellA = _fixedRoomMap.GetCell(doorA.x, doorA.y, doorA.z);
                cellA.SetToDoor(cellA.GetDoorID(), true);
                _fixedRoomMap.SetCell(doorA.x, doorA.y, doorA.z, cellA);

                var cellB = _fixedRoomMap.GetCell(doorB.x, doorB.y, doorB.z);
                cellB.SetToDoor(cellB.GetDoorID(), true);
                _fixedRoomMap.SetCell(doorB.x, doorB.y, doorB.z, cellB);
            }
        }

        _isSpawned = true;
    }

    public void DrawSubRooms(Color doorColor)
    {
        if (!_isInitalised && !_isSpawned)
        {
            return;
        }

        var currentGridSize = _fixedRoomMap.GetGridSize();
        for(int y = 0; y < currentGridSize.y; y++)
        {
            for(int z = 0; z < currentGridSize.z; z++)
            {
                for(int x = 0; x < currentGridSize.x; x++)
                {
                    var cell = _fixedRoomMap.GetCell(x, y, z);
                    if (cell.IsDoor() || cell.IsDoorUnset())
                    {
                        int doorID = cell.GetDoorID();
                        if (doorID < 0)
                        {
                            continue;
                        }

                        var doorPos = _fixedRoomMap.GetWorldPosition(x, y, z);
                        var currentCellSize = _fixedRoomMap.GetCellSize();

                        Vector3 pointA = doorPos;
                        Vector3 pointB = doorPos + new Vector3(currentCellSize.x, 0f, 0f);
                        Vector3 pointC = doorPos + new Vector3(currentCellSize.x, 0f, currentCellSize.z);
                        Vector3 pointD = doorPos + new Vector3(0f, 0f, currentCellSize.z);

                        if (cell.IsDoor())
                        {
                            Debug.DrawLine(pointA, pointB, _idColors[doorID]);
                            Debug.DrawLine(pointB, pointC, _idColors[doorID]);
                            Debug.DrawLine(pointC, pointD, _idColors[doorID]);
                            Debug.DrawLine(pointD, pointA, _idColors[doorID]);
                        }
                        else
                        {
                            Debug.DrawLine(pointA, pointC, _idColors[doorID]);
                            Debug.DrawLine(pointD, pointB, _idColors[doorID]);
                        }
                    }
                }
            }
        }

        foreach(var (pA, pB) in _edgeList)
        {
            Vector3 pA3D = new (pA.x, 0f, pA.y);
            Vector3 pB3D = new (pB.x, 0f, pB.y);
            Debug.DrawLine(pA3D, pB3D);
        }

    }

    public void DrawDebugLines(Color color)
    {
        if (!_isInitalised)
        {
            return;
        }

        _fixedRoomMap.DrawDebugLines(color);
    }

    private bool IsAreaFree(FixedRoomConfig config)
    {
        if (!_isInitalised)
        {
            return false;
        }

        var cfgAnchor = config.Anchor;
        var cfgEndAnchor = config.Anchor + config.Size;

        for(int y = cfgAnchor.y; y < cfgEndAnchor.y; y++)
        {
            for(int z = cfgAnchor.z; z < cfgEndAnchor.z; z++)
            {
                for(int x = cfgAnchor.x; x < cfgEndAnchor.x; x++)
                {
                    var cell = _fixedRoomMap.GetCell(x, y, z);
                    if (!cell.IsFree())
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void AddRoomToMap(FixedRoomData roomData, FixedRoomConfig config)
    {
        FixedRoomConfig roomConfig = config;
        Vector3Int currentAnchor = roomConfig.Anchor;
        Vector3Int currentSize = roomConfig.Size;

        var roomPosition = _fixedRoomMap.GetWorldPosition(currentAnchor.x, currentAnchor.y, currentAnchor.z);
        var roomID = _roomsToSpawn.Count;
        roomConfig.ConfigureRoom(roomID, roomData.gameObject, roomPosition, transform);

        var cfgAnchor = currentAnchor;
        var cfgEndAnchor = currentAnchor + currentSize;

        for(int y = cfgAnchor.y; y < cfgEndAnchor.y; y++)
        {
            for(int z = cfgAnchor.z; z < cfgEndAnchor.z; z++)
            {
                for(int x = cfgAnchor.x; x < cfgEndAnchor.x; x++)
                {
                    var cell = _fixedRoomMap.GetCell(x, y, z);
                    cell.SetToFill();
                    _fixedRoomMap.SetCell(x, y, z, cell);
                }
            }
        }

        var doorPos = roomData.GetDoorPositions(currentAnchor);
        foreach(var dp in doorPos)
        {
            var cell = _fixedRoomMap.GetCell(dp.x, dp.y, dp.z);
            cell.SetToDoor(roomConfig.GetID());
            _fixedRoomMap.SetCell(dp.x, dp.y, dp.z, cell);
        }

        _roomsToSpawn.Add(roomConfig);
    }

    public void SpawnDoors()
    {
        if (!_isInitalised)
        {
            return;
        }

        var currentGridSize = _fixedRoomMap.GetGridSize();
        var currentCellSize = _fixedRoomMap.GetCellSize();
        for(int y = 0; y < currentGridSize.y; y++)
        {
            for(int z = 0; z < currentGridSize.z; z++)
            {
                for(int x = 0; x < currentGridSize.x; x++)
                {
                    var cell = _fixedRoomMap.GetCell(x, y, z);

                    if (cell.IsDoor() || cell.IsDoorUnset())
                    {
                        var cellPos = _fixedRoomMap.GetWorldPosition(x, y, z);
                        var cellRot = Quaternion.identity;

                        List<(FixedRoomCell, Vector3Int)> surroundDoors = new(){
                            (_fixedRoomMap.GetCell(x + 1, y, z), new Vector3Int(x + 1, y, z)),
                            (_fixedRoomMap.GetCell(x - 1, y, z), new Vector3Int(x - 1, y, z)),
                            (_fixedRoomMap.GetCell(x, y, z + 1), new Vector3Int(x, y, z + 1)),
                            (_fixedRoomMap.GetCell(x, y, z - 1), new Vector3Int(x, y, z - 1))
                        };

                        Vector3Int currentDir = new(0, 0, 0);
                        foreach(var (cellOuter, cellPosOuter) in surroundDoors)
                        {
                            if (cellPosOuter.x < 0 || cellPosOuter.x >= currentGridSize.x ||
                                cellPosOuter.y < 0 || cellPosOuter.y >= currentGridSize.y ||
                                cellPosOuter.z < 0 || cellPosOuter.z >= currentGridSize.z)
                            {
                                currentDir = new Vector3Int(x, y, z) - cellPosOuter;
                                break;
                            }

                            if (cellOuter != null)
                            {
                                if (cellOuter.IsDoor() || cellOuter.IsDoorUnset())
                                {
                                    currentDir = new Vector3Int(x, y, z) - cellPosOuter;
                                    break;
                                }
                            }
                        }

                        if (currentDir.x == -1)
                        {
                            cellPos.z += currentCellSize.z;
                            cellRot = Quaternion.Euler(0f, 90f, 0f);
                        }
                        else if (currentDir.x == 1)
                        {
                            cellPos.x += currentCellSize.x;
                            cellRot = Quaternion.Euler(0f, 270f, 0f);
                        }
                        else if (currentDir.z == 1)
                        {
                            cellPos.x += currentCellSize.x;
                            cellPos.z += currentCellSize.z;
                            cellRot = Quaternion.Euler(0f, 180f, 0f);
                        }

                        if (cell.IsDoor())
                        {
                            _instantiatedDoors.Add(Instantiate(OpenDoor, cellPos, cellRot, transform));
                        }
                        else
                        {
                            _instantiatedDoors.Add(Instantiate(ClosedDoor, cellPos, cellRot, transform));
                        }
                    }
                }
            }
        }
    }


    public Vector3Int PickRandomDoorConnection(Connection connection)
    {
        if (!_isInitalised)
        {
            return Vector3Int.zero;
        }

        Vector3Int currentGridSize = _fixedRoomMap.GetGridSize();
        Vector3Int startPos = Vector3Int.zero;
        Vector3Int endPos = Vector3Int.zero;
        
        switch(connection)
        {
            case Connection.TOP:
                startPos = new(0, 0, currentGridSize.z - 1);
                endPos = new(currentGridSize.x, 1, currentGridSize.z);
            break;
            case Connection.LEFT:   
                startPos = new(0, 0, 0);
                endPos = new(1, 1, currentGridSize.z);
            break;
            case Connection.RIGHT:  
                startPos = new(currentGridSize.x - 1, 0, 0);
                endPos = new(currentGridSize.x, 1, currentGridSize.z);
            break;
            case Connection.BOTTOM: 
                startPos = new(0, 0, 0);
                endPos = new(currentGridSize.x, 1, 1);
            break;
        }

        bool shouldPick = true;
        List<Vector3Int> possibleDoorPos = new();
        Vector3Int chosenDoor = Vector3Int.zero;
        for(int z = startPos.z; z < endPos.z; z++)
        {
            for(int x = startPos.x; x < endPos.x; x++)
            {
                var cell = _fixedRoomMap.GetCell(x, 0, z);

                if (cell.IsDoorUnset())
                {
                    possibleDoorPos.Add(new Vector3Int(x, 0, z));
                }
                else if( cell.IsDoor())
                {   
                    shouldPick = false;
                    chosenDoor = new Vector3Int(x, 0, z);
                }
            }   
        }

        if (shouldPick)
        {
            chosenDoor = possibleDoorPos[_randomEngine.Next(possibleDoorPos.Count)];
            var cell = _fixedRoomMap.GetCell(chosenDoor.x, chosenDoor.y, chosenDoor.z);
            cell.SetToDoor(cell.GetDoorID(), true);
            _fixedRoomMap.SetCell(chosenDoor.x, chosenDoor.y, chosenDoor.z, cell);
        }

        return chosenDoor;
    }
    
}
