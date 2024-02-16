using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GridRoomGenerator
{
    // Information contained per cell in the grid
    public class CellInfo
    {
        public enum State
        {
            FREE, FILL, ENTRY, HALLWAY
        }

        private State _currentState;
        private bool _debugInitialised;

        public CellInfo()
        {
            _currentState = State.FREE;
            _debugInitialised = false;
        }

        public State GetState()
        {
            return _currentState;
        }

        public void SetToFill()
        {
            _currentState = State.FILL;
        }

        public void SetToEntry()
        {
            _currentState = State.ENTRY;
        }

        public void DebugInit()
        {
            _debugInitialised = true;
        }

        public bool IsDebugInit()
        {
            return _debugInitialised;
        }
    }

    // Information contained per room generated
    public class RoomInfo
    {   
        // remove this and use an acutal 3d real world position
        public Vector2Int anchor;

        public Vector3 anchor3D;
        public Vector2Int size;
        public Vector2 cellSize;
        public Vector2Int entryPoint;

        public RoomInfo(Vector2Int anchor, Vector3 anchor3D, Vector2Int size, Vector2 cellSize)
        {
            this.anchor = anchor;
            this.size = size;
            this.anchor3D = anchor3D;
            this.cellSize = cellSize;

            System.Random rand = new();
            int randomX = rand.Next(2, size.x - 2);
            int randomY = rand.Next(2, size.y - 2);
            int randomFlip = rand.Next(0, 2);
            
            
            entryPoint = new();

            Vector2Int entryPointVal = new();
            if (rand.NextDouble() >= 0.5)
            {
                entryPointVal.x = randomX;
                entryPointVal.y = randomFlip * (size.y - 1);
            }
            else
            {
                entryPointVal.x = randomFlip * (size.x - 1);
                entryPointVal.y = randomY;
            }


            entryPoint = entryPointVal;
        }

        public Vector2Int GetEntryPointDirection()
        {
            Vector2Int direction = new();

            if (entryPoint.x == 0)
            {
                direction.x = 1;
            }
            else if (entryPoint.x >= (size.x - 1))
            {
                direction.x = -1;
            }
            else
            {
                direction.x = 0;
            }

            if (entryPoint.y == 0)
            {
                direction.y = 1;
            }
            else if(entryPoint.y >= (size.y - 1))
            {
                direction.y = -1;
            }
            else
            {
                direction.y = 0;
            }

            return direction;

        }

    }

    private Vector2Int _MinRoomSize;
    private Vector2Int _MaxRoomSize;
    private GridMap<CellInfo> _GridMap;
    private int _Padding;

    private List<Vector2Int> _AnchorPoints = new();
    private List<RoomInfo> _RoomInfos = new();
    private Vector2Int _CurrentAnchorPoint;
    private int _CurrentWidth;
    private System.Random _RandomEngine = new();

    private bool _isDone;
    
    public GridRoomGenerator(Vector3Int gridSize, Vector3 cellSize, Vector3 origin,
                             Vector2Int minRoomSize, Vector2Int maxRoomSize)
    {
        _GridMap = new GridMap<CellInfo>(gridSize, cellSize, origin, () => { return new CellInfo(); });

        _MinRoomSize = minRoomSize;
        _MaxRoomSize = maxRoomSize;
        _Padding = 1;

        Reset();
    }


    public void Reset()
    {
        InitialiseGridMap();
        _CurrentWidth = _RandomEngine.Next(_MinRoomSize.x, _MaxRoomSize.x) + _Padding;
        _isDone = false;
        _AnchorPoints = new();
        _RoomInfos = new();

        _CurrentAnchorPoint = new Vector2Int(_Padding, _Padding);
        _AnchorPoints.Add(_CurrentAnchorPoint);
    }

    public Vector3 GetOriginFromInfo(RoomInfo roomInfo)
    {
        Vector2Int roomAnchor = roomInfo.anchor;
        return _GridMap.GetWorldPosition(roomAnchor.x, 0, roomAnchor.y);
    }


    public List<RoomInfo> GetRoomInfos()
    {
        return _RoomInfos;
    }

    private Vector2Int GetNextAnchorPoint()
    {
        int lowestXIndex = -1;
        Vector2Int anchorPoint = new(int.MaxValue, int.MaxValue);

        for(int i = 0; i < _AnchorPoints.Count; i++)
        {
            if (anchorPoint.x > _AnchorPoints[i].x)
            {
                anchorPoint = _AnchorPoints[i];
                lowestXIndex = i;
            }
        }

        
        if (lowestXIndex >= 0)
        {
            _AnchorPoints.RemoveAt(lowestXIndex);
        }

        return anchorPoint;
    }


    public bool Step()
    {
        if (_isDone)
        {
            return _isDone;
        }

        Vector3Int GridSize = _GridMap.GetGridSize();

        _CurrentAnchorPoint = GetNextAnchorPoint();
        if ((_MaxRoomSize.x + _CurrentAnchorPoint.x + _Padding) > GridSize.x)
        {
            _CurrentWidth = GridSize.x - _CurrentAnchorPoint.x;
        }

        if (_CurrentWidth >= _MinRoomSize.x)
        {
            if (RoomGenerateYStep())
            {
                _CurrentWidth = _RandomEngine.Next(_MinRoomSize.x, _MaxRoomSize.x) + _Padding;
            }
        }

        if (_AnchorPoints.Count == 0)
        {
            _isDone = true;
        }

        return _isDone;
    }

    public bool IsGenerationDone()
    {
        return _isDone;
    }
    
    public void DrawDebugLines(Color color)
    {
        _GridMap.DrawDebugLines(color);
    }
    
    private void InitialiseGridMap()
    {
        var GridSize = _GridMap.GetGridSize();

        for(int y = 0; y < GridSize.y; y++)
        {
            for(int z = 0; z < GridSize.z; z++)
            {
                for(int x = 0; x < GridSize.x; x++)
                {
                    _GridMap.SetCell(x, y, z, new CellInfo());
                }
            }
        }
    }


    private bool RoomGenerateYStep()
    {
        bool ReachedEndOfY = false;
        Vector3Int GridSize = _GridMap.GetGridSize();

        Vector2Int RoomSize = Vector2Int.zero;
        RoomSize.y = GridSize.y;
        RoomSize.x = _CurrentWidth;

        if (_CurrentAnchorPoint.y + _MaxRoomSize.y + _Padding > GridSize.z)
        {
            RoomSize.y = GridSize.z - _CurrentAnchorPoint.y;
            ReachedEndOfY = true;
        }
        else
        {
            RoomSize.y = _RandomEngine.Next(_MinRoomSize.y, _MaxRoomSize.y) + _Padding;
        }
        
        if (!ReachedEndOfY)
        {
            _AnchorPoints.Add(new Vector2Int(
                _CurrentAnchorPoint.x,
                _CurrentAnchorPoint.y + RoomSize.y
            ));

            if (_CurrentAnchorPoint.y == _Padding)
            {
                _AnchorPoints.Add(new Vector2Int(
                    _CurrentAnchorPoint.x + _CurrentWidth,
                    _CurrentAnchorPoint.y
                ));
            }
        }

        if (RoomSize.y >= _MinRoomSize.y)
        {
            RoomInfo roomInfo = new(
                _CurrentAnchorPoint, 
                _GridMap.GetWorldPosition(_CurrentAnchorPoint.x, 0, _CurrentAnchorPoint.y),
                RoomSize - new Vector2Int(_Padding, _Padding),
                new Vector2(_GridMap.GetCellSize().x, _GridMap.GetCellSize().z));
            GenerateRoom(roomInfo);
            _RoomInfos.Add(roomInfo);
        }

        return ReachedEndOfY;
    }


    private void GenerateRoom(RoomInfo roomInfo)
    {
        var GridSize = _GridMap.GetGridSize();

        for(int y = 0; y < GridSize.y; y++)
        {
            for (int z = roomInfo.anchor.y; z < roomInfo.anchor.y + roomInfo.size.y; z++)
            {
                for(int x = roomInfo.anchor.x; x < roomInfo.anchor.x + roomInfo.size.x; x++)
                {
                    Vector2Int currentXZPos = new(x, z);
                    if ((roomInfo.entryPoint + roomInfo.anchor) == currentXZPos)
                    {
                        CellInfo roomCell = new();
                        roomCell.SetToEntry();
                        _GridMap.SetCell(x, y, z, roomCell);
                    }

                    CellInfo currentCellInfo = _GridMap.GetCell(x, y, z);
                    if (currentCellInfo.GetState() != CellInfo.State.ENTRY)
                    {
                        CellInfo roomCell = new();
                        roomCell.SetToFill();
                        _GridMap.SetCell(x, y, z, roomCell);
                    }
                }
            }
        }
    }
}
