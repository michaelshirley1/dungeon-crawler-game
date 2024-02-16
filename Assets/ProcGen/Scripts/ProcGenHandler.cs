using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ProcGenHandler : MonoBehaviour
{
    public enum CellTestType
    {
        HALLWAY, OCCUPIED, FREE, TRUE_HALLWAY
    }

    public class SectionInfo
    {
        public enum Side { TOP, LEFT, RIGHT, BOTTOM };

        private Dictionary<Side, List<int>> _possibleDoorPlacements;
        private Vector3 _SectionPosition;
        private Vector3Int _UnsubdividedGridSize;
        private Vector3 _UnsubdividedCellSize;
        private int _SubdivideBy;
        private int _unsubdiviedDoorPos;

        private bool _isChosen;
        private Side _currentSide;
        private int _currentDoorPos;
        private Vector3Int _anchorPos; 

        public SectionInfo(Vector3 position, Vector3Int anchorPos, Vector3Int gridSize, Vector3 cellSize, int subdivideBy = 1)
        {
            _SectionPosition = position;
            _UnsubdividedGridSize = gridSize;
            _UnsubdividedCellSize = cellSize;
            _anchorPos = anchorPos;
            _SubdivideBy = subdivideBy;

            Debug.Log((position, anchorPos, gridSize, cellSize));

            _possibleDoorPlacements = new()
            {
                { Side.TOP, new() },
                { Side.LEFT, new() },
                { Side.RIGHT, new() },
                { Side.BOTTOM, new() }
            };

            _isChosen = false;
        }

        public void AddDoorPos(Side side, int pos)
        {
            bool shouldAdd = true;
            foreach(var doorPos in _possibleDoorPlacements[side])
            {
                if (doorPos == pos)
                {
                    shouldAdd = false;
                    break;
                }
            }

            if (shouldAdd)
            {
                _possibleDoorPlacements[side].Add(pos);
            }
        }

        public (Vector2Int, int) GetDoorConfig()
        {
            if (!_isChosen)
            {
                List<Side> possibleSides = new();
                foreach(var pdp in _possibleDoorPlacements)
                {
                    if (pdp.Value.Count > 0)
                    {
                        possibleSides.Add(pdp.Key);
                    }
                }

                if (possibleSides.Count == 0)
                {
                    return (new Vector2Int(0, 0), -1);
                }

                System.Random rand = new();
                _currentSide = possibleSides[rand.Next(possibleSides.Count)];
                _unsubdiviedDoorPos = _possibleDoorPlacements[_currentSide][rand.Next(_possibleDoorPlacements[_currentSide].Count)];
                _currentDoorPos = (_unsubdiviedDoorPos * _SubdivideBy) + (int)Mathf.Floor((float)_SubdivideBy / 2f);

                _isChosen = true;
            }

            return (GetDirectionFromSide(_currentSide), _currentDoorPos);
        }

        public Vector2Int GetDirectionFromSide(Side side)
        {
            Vector2Int dir = new(0, 0);
            switch(side)
            {
                case Side.TOP:      dir = new(0, -1); break;
                case Side.LEFT:     dir = new(1, 0); break;
                case Side.RIGHT:    dir = new(-1, 0); break;
                case Side.BOTTOM:   dir = new(0, 1); break;
            }

            return dir;
        }

        public Vector3Int GetSectionGridSize()
        {
            var currentDir = GetDirectionFromSide(_currentSide);
            Vector3Int roomSize = _UnsubdividedGridSize;

            roomSize.x -= (int)Mathf.Abs(currentDir.x);
            roomSize.z -= (int)Mathf.Abs(currentDir.y);

            roomSize.x *= _SubdivideBy;
            roomSize.z *= _SubdivideBy;

            return roomSize;
        }

        public Vector3 GetSectionCellSize()
        {
            Vector3 roomCellSize = _UnsubdividedCellSize;

            roomCellSize.x /= (float)_SubdivideBy;
            roomCellSize.z /= (float)_SubdivideBy;

            return roomCellSize;
        } 

        public Vector3 GetSectionAnchorPosition()
        {
            var currentDir = GetDirectionFromSide(_currentSide);
            Vector3 roomAnchor = _SectionPosition;

            if (currentDir.x > 0 || currentDir.y > 0)
            {
                roomAnchor.x += _UnsubdividedCellSize.x * Mathf.Abs(currentDir.x);
                roomAnchor.z += _UnsubdividedCellSize.z * Mathf.Abs(currentDir.y);
            }

            return roomAnchor;
        }

        public (Vector3Int, Vector3Int) GetUnsubdividedData()
        {
            return (_anchorPos, _UnsubdividedGridSize);
        }

        public Vector3Int GetEntryPosition()
        {
            var dir = GetDirectionFromSide(_currentSide);

            Vector2Int AbsDirection = new(Math.Abs(dir.x), Math.Abs(dir.y));
            Vector2Int flippedAbsDirection = new(Math.Abs(dir.y), Math.Abs(dir.x));

            var currentSize = _UnsubdividedGridSize;
            Vector2Int currentSize2D = new(currentSize.x - 1, currentSize.z - 1);
            
            Vector2Int relativeEntryPos = flippedAbsDirection * _unsubdiviedDoorPos;
            if (dir.x < 0 || dir.y < 0)
            {
                relativeEntryPos += Vector2Int.Scale(currentSize2D, AbsDirection);
            }

            return new Vector3Int(relativeEntryPos.x, 0, relativeEntryPos.y) + _anchorPos;
        }
    }

    public bool InDebugMode = false;
    public TextMeshPro debugText; 

    [Header("Spawners")]
    public GameObject SpawnStrategyPrefab;
    public GameObject HallwaySpawner;
    
    [Header("Grid Map Settings")]
    public Vector3 CellSize; 

    [Header("Grid Size")]
    public Vector2Int MinGridSize;
    public Vector2Int MaxGridSize;

    [Header("Section Size")]
    public Vector2Int MinSectionSize;
    public Vector2Int MaxSectionSize;

    private GridMap<CellTestType> _gridMap; 
    private System.Random _randEngine = new();
    private Vector3Int _gridSize;

    private List<GameObject> _instantiatedSections = new();
    private List<SectionInfo> _sectionInfoList = new();
    private AStarPathFinding _pf = new();

    private Graph2D.Graph _trimmedGraph = new();
    private GameObject _instantiateHallwaySpawner;

    public void GenerateRooms()
    {
        _gridSize = new(
            _randEngine.Next(MinGridSize.x, MaxGridSize.x),
            1,
            _randEngine.Next(MinGridSize.y, MaxGridSize.y)
        );
        _sectionInfoList = new();

        var midPointGrid = ((Vector3)_gridSize / 2f);
        transform.position = -Vector3.Scale(midPointGrid, CellSize);

        if (_instantiatedSections.Count > 0)
        {
            foreach(var section in _instantiatedSections)
            {
                Destroy(section);
            }

            Destroy(_instantiateHallwaySpawner);

            _instantiateHallwaySpawner = new();
            _instantiatedSections = new();
        }

        _gridMap = new(_gridSize, CellSize, transform.position, () => { return CellTestType.FREE; });
        
        // Generate the rooms
        for(int z = 0; z < _gridSize.z; z++)
        {
            for(int x = 0; x < _gridSize.x; x++)
            {
                Vector2Int currentAnchor = new(x, z);
                Vector2Int maxCurrentSize = GetMaxFreeSpace(currentAnchor);

                if (maxCurrentSize.x < MinSectionSize.x || maxCurrentSize.y < MinSectionSize.y)
                {
                    continue;
                }

                Vector2Int currentSize = new(
                    _randEngine.Next(MinSectionSize.x, (int)Mathf.Min(maxCurrentSize.x, MaxSectionSize.x)),
                    _randEngine.Next(MinSectionSize.y, (int)Mathf.Min(maxCurrentSize.y, MaxSectionSize.y))
               );

                if (PlaceRoom(currentAnchor, currentSize))
                {
                    Vector3Int currentAnchor3D = new(currentAnchor.x + 1, 0, currentAnchor.y + 1);
                    Vector3 SectionPosition = _gridMap.GetWorldPosition(currentAnchor3D.x, currentAnchor3D.y, currentAnchor3D.z);
                    Vector3Int sectionSize = new((currentSize.x - 2), 1, (currentSize.y - 2));

                    SectionInfo info = new(SectionPosition, currentAnchor3D, sectionSize, CellSize, 3);
                    _sectionInfoList.Add(info);
                }
            }
        }

        print("----------------------");

        // Filter out hallways that are not good
        CellTestType[] invalidLine = new CellTestType[4]{
            CellTestType.OCCUPIED, CellTestType.HALLWAY, CellTestType.HALLWAY, CellTestType.OCCUPIED
        };


        for(int z = 0; z < _gridSize.z; z++)
        {
            for(int x = 0; x < _gridSize.x; x++)
            {
                // x direction
                if (_gridSize.x - x > 3)
                {
                    bool isInvalid = true;
                    for(int i = 0; i < invalidLine.Length; i++)
                    {
                        if (invalidLine[i] != _gridMap.GetCell(x + i, 0, z))
                        {
                            isInvalid = false;
                            break;
                        }
                    }

                    if (isInvalid)
                    {
                        _gridMap.SetCell(x + 2, 0, z, CellTestType.FREE);
                    }
                }

                if (_gridSize.z - z > 3)
                {
                    bool isInvalid = true;
                    for(int i = 0; i < invalidLine.Length; i++)
                    {
                        if (invalidLine[i] != _gridMap.GetCell(x, 0, z + i))
                        {
                            isInvalid = false;
                            break;
                        }
                    }

                    if (isInvalid)
                    {
                        _gridMap.SetCell(x, 0, z + 2, CellTestType.FREE);
                    }
                }
            }
        }

        var sideList = SectionInfo.Side.GetValues(typeof(SectionInfo.Side)).Cast<SectionInfo.Side>();
        List<Vector2Int> entryPoints = new();

        foreach(var sil in _sectionInfoList)
        {
            var (anchor, size) = sil.GetUnsubdividedData();
            print((anchor, size));
            
            foreach(var s in sideList)
            {
                var dir = sil.GetDirectionFromSide(s);
                if (s == SectionInfo.Side.TOP || s == SectionInfo.Side.BOTTOM)
                {
                    for(int i = anchor.x + 2; i < (anchor.x + size.x) - 2; i++)
                    {
                        if (s == SectionInfo.Side.TOP)
                        {
                            var cell = _gridMap.GetCell(i, 0, anchor.z + size.z);
                            if (cell == CellTestType.HALLWAY)
                            {
                                sil.AddDoorPos(s, i - anchor.x);
                            }
                        }
                        else
                        {
                            var cell = _gridMap.GetCell(i, 0, anchor.z - 1);
                            if (cell == CellTestType.HALLWAY)
                            {
                                sil.AddDoorPos(s, i - anchor.x);
                            }
                        }
                    }
                }

                if (s == SectionInfo.Side.LEFT || s == SectionInfo.Side.RIGHT)
                {
                    for(int i = anchor.z + 2; i < (anchor.z + size.z) - 2; i++)
                    {
                        if (s == SectionInfo.Side.RIGHT)
                        {
                            var cell = _gridMap.GetCell(anchor.x + size.x, 0, i);
                            if (cell == CellTestType.HALLWAY)
                            {
                                sil.AddDoorPos(s, i - anchor.z);
                            }
                        }
                        else
                        {
                            var cell = _gridMap.GetCell(anchor.x - 1, 0, i);
                            if (cell == CellTestType.HALLWAY)
                            {
                                sil.AddDoorPos(s, i - anchor.z);
                            }
                        }
                    }
                }
            }

            var (sectionDir, sectionDoorPos) = sil.GetDoorConfig();
            var sectionAnchor = sil.GetSectionAnchorPosition();
            var sectionSize = sil.GetSectionGridSize();
            var sectionCellSize = sil.GetSectionCellSize();

            print((sectionAnchor, sectionSize, sectionCellSize, sectionDir, sectionDoorPos));
            var instancedRoom = Instantiate(SpawnStrategyPrefab, sectionAnchor, Quaternion.identity, transform);
            var spawnStrategy = instancedRoom.GetComponent<SingleEntranceSpawnStrategy>();

            spawnStrategy.Initialise(sectionSize, sectionCellSize, 3, 24, sectionDir, sectionDoorPos);
            _instantiatedSections.Add(instancedRoom);

            var entryPos = sil.GetEntryPosition();

            var currCell = _gridMap.GetCell(entryPos.x, entryPos.y, entryPos.z);
            currCell = CellTestType.TRUE_HALLWAY;
            _gridMap.SetCell(entryPos.x, entryPos.y, entryPos.z, currCell);

            entryPoints.Add(new(entryPos.x, entryPos.z));
        }
        
        var currentGridSize = _gridMap.GetGridSize();
        GameObject instantiatedHallwaySpawner = Instantiate(HallwaySpawner, transform.position, Quaternion.identity, transform);
        var hallwayManager = instantiatedHallwaySpawner.GetComponent<HallwaySpawnerManager>();
        hallwayManager.Initialise(currentGridSize, _gridMap.GetCellSize(), 0);

        for(int y = 0; y < currentGridSize.y; y++)
        {
            for(int z = 0; z < currentGridSize.z; z++)
            {
                for(int x = 0; x < currentGridSize.x; x++)
                {
                    var cell = _gridMap.GetCell(x, y, z);

                    if (cell == CellTestType.HALLWAY || cell == CellTestType.TRUE_HALLWAY)
                    {
                        HallwayData.Config hallwayCfg = new(false, false, false, false);
                        List<(CellTestType, Vector3Int)> surroundDoors = new(){
                            (_gridMap.GetCell(x + 1, y, z), new Vector3Int(x + 1, y, z)),
                            (_gridMap.GetCell(x - 1, y, z), new Vector3Int(x - 1, y, z)),
                            (_gridMap.GetCell(x, y, z + 1), new Vector3Int(x, y, z + 1)),
                            (_gridMap.GetCell(x, y, z - 1), new Vector3Int(x, y, z - 1))
                        };

                        List<Vector3Int> doorDirs = new();
                        foreach(var (cellOuter, cellPosOuter) in surroundDoors)
                        {
                            if (cellPosOuter.x < 0 || cellPosOuter.x >= currentGridSize.x ||
                                cellPosOuter.y < 0 || cellPosOuter.y >= currentGridSize.y ||
                                cellPosOuter.z < 0 || cellPosOuter.z >= currentGridSize.z)
                            {
                                if (cell == CellTestType.TRUE_HALLWAY)
                                {
                                    doorDirs.Add(new Vector3Int(x, y, z) - cellPosOuter);
                                }
                                continue;
                            }

                            if (cellOuter == CellTestType.HALLWAY || cellOuter == CellTestType.TRUE_HALLWAY)
                            {
                                doorDirs.Add(new Vector3Int(x, y, z) - cellPosOuter);
                            }
                        }

                        foreach(var currentDir in doorDirs)
                        {
                            if (currentDir.x == -1)
                            {
                                if (cell == CellTestType.TRUE_HALLWAY)
                                {
                                    hallwayCfg.NegX = true;    
                                }
                                hallwayCfg.PosX = true;
                            }
                            else if (currentDir.x == 1)
                            {
                                if (cell == CellTestType.TRUE_HALLWAY)
                                {
                                    hallwayCfg.PosX = true;    
                                }
                                hallwayCfg.NegX = true;
                            }
                            else if (currentDir.z == -1)
                            {
                                if (cell == CellTestType.TRUE_HALLWAY)
                                {
                                    hallwayCfg.NegY = true;    
                                }
                                hallwayCfg.PosY = true;
                            }
                            else if (currentDir.z == 1)
                            {
                                if (cell == CellTestType.TRUE_HALLWAY)
                                {
                                    hallwayCfg.PosY = true;    
                                }
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

        // Vector2 minPoint = new(float.MaxValue, float.MaxValue);
        // Vector2 maxPoint = new(float.MinValue, float.MinValue);
        // List<Vector2> points = new();
        // var currentCellSize = _gridMap.GetCellSize(); 
        // foreach(var e in entryPoints)
        // {
        //     var entryPointWorldPos = _gridMap.GetWorldPosition(e.x, 0, e.y);
        //     var centeredPos = new Vector2(entryPointWorldPos.x, entryPointWorldPos.z);
        //     points.Add(centeredPos);

        //     minPoint = new Vector2(
        //         Mathf.Min(minPoint.x, centeredPos.x),
        //         Mathf.Min(minPoint.y, centeredPos.y)
        //     );

        //     maxPoint = new Vector2(
        //         Mathf.Max(maxPoint.x, centeredPos.x),
        //         Mathf.Max(maxPoint.y, centeredPos.y)
        //     );
        // }

        // DelaunayTriangulation dTri = new(minPoint, maxPoint);
        // foreach(var p in points)
        // {
        //     dTri.AddPoint(p);
        // }

        // MinimumSpanningTree mst = new();
        // mst.TrimGraph(dTri.GetTriangulationStruct());
        // _trimmedGraph = mst.GetTrimmedGraph();

        
        
        // foreach(var (startPos, endPos) in _trimmedGraph.GetEdgeListPositions3D())
        // {
        //     _pf.SetMap(_gridMap);
        //     var startNode = _gridMap.GetCellPositionFromWorld(startPos);
        //     var endNode = _gridMap.GetCellPositionFromWorld(endPos);
        //     print((startNode, endNode));

        //     var startNode2D = new Vector2Int((int)startNode.x, (int)startNode.z);
        //     var endNode2D = new Vector2Int((int)endNode.x, (int)endNode.z);

        //     foreach(var p in _pf.Solve(startNode2D, endNode2D))
        //     {
        //         var cell = _gridMap.GetCell(p.x, 0, p.y);
        //         cell = CellTestType.TRUE_HALLWAY;
        //         _gridMap.SetCell(p.x, 0, p.y, cell);
        //     }
        // }
    }

    bool PlaceRoom(Vector2Int anchor, Vector2Int size)
    {
        if (!CheckIfFree(anchor, size)) return false;

        for(int y = anchor.y; y < anchor.y + size.y; y++)
        {
            for(int x = anchor.x; x < anchor.x + size.x; x++)
            {
                var cell = _gridMap.GetCell(x, 0, y);

                if (x == anchor.x || y == anchor.y || y == anchor.y + size.y - 1 || x == anchor.x + size.x - 1)
                {
                    cell = CellTestType.HALLWAY;
                }
                else
                {
                    cell = CellTestType.OCCUPIED;
                }
                
                _gridMap.SetCell(x, 0, y, cell);
            }
        }

        return true;
    }


    bool CheckIfFree(Vector2Int anchor, Vector2Int size)
    {
        for(int y = anchor.y; y < anchor.y + size.y; y++)
        {
            for(int x = anchor.x; x < anchor.x + size.x; x++)
            {
                var cell = _gridMap.GetCell(x, 0, y);

                if (cell == CellTestType.OCCUPIED)
                {
                    return false;
                }
            }
        }

        return true;
    }

    Vector2Int GetMaxFreeSpace(Vector2Int anchor)
    {
        Vector2Int maxAllowableSize = new(
            _gridSize.x - anchor.x,
            _gridSize.z - anchor.y
        );

        for(int y = anchor.y; y < _gridSize.z - 1; y++)
        {
            for(int x = anchor.y; x < _gridSize.y; x++)
            {
                var cell = _gridMap.GetCell(x, 0, y);
                if (cell == CellTestType.OCCUPIED)
                {
                    if (maxAllowableSize.x > x)
                    {
                        maxAllowableSize.x = x;
                    }
                }

                var cellAbove = _gridMap.GetCell(x, 0, y + 1);
                if (cellAbove == CellTestType.OCCUPIED)
                {
                    if (maxAllowableSize.y > y)
                    {
                        maxAllowableSize.y = y;
                    }
                }
                
            }
        }

        return maxAllowableSize;
    }

    void DrawDebugLines()
    {
        for(int z = 0; z < _gridSize.z; z++)
        {
            for(int x = 0; x < _gridSize.x; x++)
            {
                Color currentColor = Color.clear;
                var cell = _gridMap.GetCell(x, 0, z);

                switch(cell)
                {
                    case CellTestType.OCCUPIED: currentColor = Color.green; break;
                    case CellTestType.HALLWAY: currentColor = Color.blue; break;
                    case CellTestType.TRUE_HALLWAY: currentColor = Color.red; break;
                }

                _gridMap.DrawCell(x, 0, z, currentColor);
            }
        }
    }

}
