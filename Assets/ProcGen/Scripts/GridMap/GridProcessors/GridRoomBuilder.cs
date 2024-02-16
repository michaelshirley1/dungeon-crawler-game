using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridRoomBuilder
{
    public class HallwayCellInfo
    {
        public enum HallwayType
        {
            NONE, MAIN_ENTRY, STRAIGHT, THREE_WAY, FOUR_WAY, CORNER  
        }

        private HallwayType _hallwayType;

        public HallwayCellInfo()
        {
            _hallwayType = HallwayType.NONE;
        }

        public void SetAsMainEntry()
        {
            _hallwayType = HallwayType.MAIN_ENTRY;
        }

        public void SetAsStraight()
        {
            _hallwayType = HallwayType.STRAIGHT;
        }

        public void SetAsFourWay()
        {
            _hallwayType = HallwayType.FOUR_WAY;
        }

        public void SetAsThreeWay()
        {
            _hallwayType = HallwayType.THREE_WAY;
        }

        public void SetAsCorner()
        {
            _hallwayType = HallwayType.CORNER;
        }

        public HallwayType GetHallwayType()
        {
            return _hallwayType;
        }
    }
    
    private int _subdivideBy;
    private Vector3Int _gridSize;
    private Vector3 _cellSize;
    private Vector3 _origin;
    private Vector3Int _mainEntryPoint;
    private Vector2Int _mainEntryPointDir;

    private List<(Vector3Int, Vector3Int)> _RoomBoundingInfo;
    private List<(Vector3Int, Vector3Int)> _BSPSplitRooms = new();

    private GridMap<HallwayCellInfo> _hallwayGridMap;

    public GridRoomBuilder(GridRoomGenerator.RoomInfo roominfo, int subdivideBy, float roomHeight)
    {
        _subdivideBy = subdivideBy;

        _gridSize = new Vector3Int(roominfo.size.x * subdivideBy, 1, roominfo.size.y * subdivideBy);
        _cellSize = new Vector3(roominfo.cellSize.x / subdivideBy, roomHeight, roominfo.cellSize.y / subdivideBy);

        Debug.Log(roominfo.size.ToString() + ", " + roominfo.cellSize.ToString());
        _origin = roominfo.anchor3D;
        
        _hallwayGridMap = new GridMap<HallwayCellInfo>(_gridSize, _cellSize, roominfo.anchor3D, () => { return new HallwayCellInfo(); });
        
        _mainEntryPoint = GetAdjustedEntryPoint(roominfo);
        _mainEntryPointDir = roominfo.GetEntryPointDirection();

        InitialiseGridMap(_mainEntryPoint);

        // calculate the first hallway depth
        var (startHallwayPoint, endHallwayPoint) = GetHallwayCoords(_mainEntryPoint, _mainEntryPointDir);
        AddHallway(startHallwayPoint, endHallwayPoint, _mainEntryPointDir);
        
        // calculate the middle points of the hallways
        int hallwayDistance = (int)Vector3Int.Distance(startHallwayPoint, endHallwayPoint);
        _RoomBoundingInfo = new();
        Vector3Int mainEntryPointDir3D = new(_mainEntryPointDir.x, 0, _mainEntryPointDir.y);
        Vector2Int mainEntryPointDirSwappedAbs = new (Mathf.Abs(_mainEntryPointDir.y), Mathf.Abs(_mainEntryPointDir.x));

        int maxSize = 12;
        int minSize = 3;
        
        Vector3Int currentHallwayPoint = startHallwayPoint;
        Vector3Int currentHallwayPointSubRoomLeft = startHallwayPoint;
        Vector3Int currentHallwayPointSubRoomRight = startHallwayPoint;

        int currentHallwayDistance = (int)Vector3Int.Distance(currentHallwayPoint, endHallwayPoint);
        System.Random rand = new();

        while (currentHallwayDistance > (minSize * 2))
        {
            List<int> PossibleRoomWidths = new();
            for(int i = maxSize; i >= minSize; i /= 2)
            {
                if (i < currentHallwayDistance)
                {
                    PossibleRoomWidths.Add(i);
                }
            }

            if (PossibleRoomWidths.Count == 0)
            {
                break;
            }

            int randomRoomWidth = PossibleRoomWidths[rand.Next(PossibleRoomWidths.Count)];
            Vector3Int intersectionPoint = (mainEntryPointDir3D * randomRoomWidth) + currentHallwayPoint;

            var cell = _hallwayGridMap.GetCell(intersectionPoint.x, intersectionPoint.y, intersectionPoint.z);
            cell.SetAsFourWay();
            _hallwayGridMap.SetCell(intersectionPoint.x, intersectionPoint.y, intersectionPoint.z, cell);

            double choice = rand.NextDouble();

            if (choice < 1.0 / 3.0 || choice <= 2.0 / 3.0)
            {
                var (splitHallwayStart, splitHallwayEnd) = GetHallwayCoords(intersectionPoint, mainEntryPointDirSwappedAbs);

                Vector3Int hallwayDirRight3D = new(mainEntryPointDirSwappedAbs.x, 0, mainEntryPointDirSwappedAbs.y);
                var roomBoundsAndHallwaySize = GetSubRoomInfo(
                    24, 3, currentHallwayPointSubRoomRight, mainEntryPointDir3D, 
                    splitHallwayStart, splitHallwayEnd, hallwayDirRight3D
                );
                
                _RoomBoundingInfo.Add((roomBoundsAndHallwaySize.Item1, roomBoundsAndHallwaySize.Item2));
                AddHallway(splitHallwayStart, roomBoundsAndHallwaySize.Item3, mainEntryPointDirSwappedAbs);
            }

            if (choice >= 1.0 / 3.0)
            {
                var (splitHallwayStart, splitHallwayEnd) = GetHallwayCoords(intersectionPoint, mainEntryPointDirSwappedAbs * -1);

                Vector3Int hallwayDirLeft3D = new(mainEntryPointDirSwappedAbs.x * -1, 0, mainEntryPointDirSwappedAbs.y * -1);
                var roomBoundsAndHallwaySize = GetSubRoomInfo(
                    24, 3, currentHallwayPointSubRoomLeft, mainEntryPointDir3D, 
                    splitHallwayStart, splitHallwayEnd, hallwayDirLeft3D
                );

                _RoomBoundingInfo.Add((roomBoundsAndHallwaySize.Item1, roomBoundsAndHallwaySize.Item2));
                AddHallway(splitHallwayStart, roomBoundsAndHallwaySize.Item3, mainEntryPointDirSwappedAbs * -1);
            }
            
            currentHallwayPoint = intersectionPoint + mainEntryPointDir3D;

            if (choice < 1.0 / 3.0 || choice <= 2.0 / 3.0)
            {
                currentHallwayPointSubRoomRight = currentHallwayPoint;
            }

            if (choice >= 1.0 / 3.0)
            {
                currentHallwayPointSubRoomLeft = currentHallwayPoint;
            }

            currentHallwayDistance = (int)Vector3Int.Distance(currentHallwayPoint, endHallwayPoint);

            if (currentHallwayDistance < maxSize)
            {
                break;
            }
        }

        if (currentHallwayDistance > 3)
        {
            var (splitHallwayStartRight, splitHallwayEndRight) = GetHallwayCoords(endHallwayPoint, mainEntryPointDirSwappedAbs);
            Vector3Int hallwayDirRight3DEnd = new(mainEntryPointDirSwappedAbs.x, 0, mainEntryPointDirSwappedAbs.y);
            var roomBoundsAndHallwaySizeRight = GetSubRoomInfo(
                24, 3, currentHallwayPointSubRoomRight, mainEntryPointDir3D, 
                splitHallwayStartRight, splitHallwayEndRight, hallwayDirRight3DEnd,
                true
            );
            _RoomBoundingInfo.Add((roomBoundsAndHallwaySizeRight.Item1, roomBoundsAndHallwaySizeRight.Item2));

            var (splitHallwayStartLeft, splitHallwayEndLeft) = GetHallwayCoords(endHallwayPoint, mainEntryPointDirSwappedAbs * -1);
            Vector3Int hallwayDirLeft3DEnd = new(mainEntryPointDirSwappedAbs.x * -1, 0, mainEntryPointDirSwappedAbs.y * -1);
            var roomBoundsAndHallwaySizeLeft = GetSubRoomInfo(
                24, 3, currentHallwayPointSubRoomLeft, mainEntryPointDir3D, 
                splitHallwayStartLeft, splitHallwayEndLeft, hallwayDirLeft3DEnd,
                true
            );

            _RoomBoundingInfo.Add((roomBoundsAndHallwaySizeLeft.Item1, roomBoundsAndHallwaySizeLeft.Item2));
        }

        foreach(var (anchor, size) in _RoomBoundingInfo)
        {
            Vector2Int anchor2D = new(anchor.x, anchor.z);
            Vector2Int size2D = new(size.x, size.z);
            Debug.Log("RBI: " + anchor2D.ToString() + ", " + size2D.ToString());
            BSPTree bsptree = new(size2D, anchor2D, 3);
            var listOfRooms = bsptree.GetAllData();

            foreach (var r in listOfRooms)
            {
                Vector3Int newAnchor = new(r.GetAnchor().x, anchor.y, r.GetAnchor().y);
                Vector3Int newSize = new(r.GetSize().x, size.y, r.GetSize().y);

                _BSPSplitRooms.Add((newAnchor, newSize));

                var cellA = _hallwayGridMap.GetCell(r.GetAnchor().x, 0, r.GetAnchor().y);
                cellA.SetAsCorner();
                _hallwayGridMap.SetCell(r.GetAnchor().x, 0, r.GetAnchor().y, cellA);

                var cellB = _hallwayGridMap.GetCell(r.GetAnchor().x + r.GetSize().x - 1, 0, r.GetAnchor().y + r.GetSize().y - 1);
                cellB.SetAsThreeWay();
                _hallwayGridMap.SetCell(r.GetAnchor().x + r.GetSize().x - 1, 0, r.GetAnchor().y + r.GetSize().y - 1, cellB);
            }
        }
    }

    public Vector3Int GetGridSize()
    {
        return _gridSize;
    }

    public Vector3 GetCellSize()
    {
        return _cellSize;
    }

    public void DrawDebugLines(Color color)
    {
        _hallwayGridMap.DrawDebugLines(color);
    }

    private void InitialiseGridMap(Vector3Int mainEntryPoint)
    {
        var cell = _hallwayGridMap.GetCell(mainEntryPoint.x, mainEntryPoint.y, mainEntryPoint.z);
        cell.SetAsMainEntry();
        _hallwayGridMap.SetCell(mainEntryPoint.x, mainEntryPoint.y, mainEntryPoint.z, cell);
    }


    private Vector3Int GetAdjustedEntryPoint(GridRoomGenerator.RoomInfo roomInfo)
    {
        Vector3Int roomEntryPoint = new(roomInfo.entryPoint.x * _subdivideBy, 0, roomInfo.entryPoint.y * _subdivideBy);
        Vector2Int roomDir = roomInfo.GetEntryPointDirection();
        Vector2Int scaledRoomDir = roomDir * _subdivideBy;

        // The direction is positive
        if (roomDir.x > 0 || roomDir.y > 0)
        {
            roomEntryPoint += new Vector3Int(scaledRoomDir.x, 0, scaledRoomDir.y);
        }
        // The direction is negative
        else if (roomDir.x < 0 || roomDir.y < 0)
        {
            roomEntryPoint += new Vector3Int(roomDir.x, 0, roomDir.y);
        }


        int centeredPos = _subdivideBy / 2;
        roomEntryPoint += new Vector3Int(Mathf.Abs(roomDir.y) * centeredPos, 0, Mathf.Abs(roomDir.x) * centeredPos);
               
        return roomEntryPoint;
    }


    public List<(Vector3Int, Color)> GetDebugCellInfo()
    {
        List<(Vector3Int, Color)> debugObjectList = new();

        for(int y = 0; y < _gridSize.y; y++)
        {
            for(int z = 0; z < _gridSize.z; z++)
            {
                for(int x = 0; x < _gridSize.x; x++)
                {
                    var cell = _hallwayGridMap.GetCell(x, y, z);
                    Color currentColor = Color.white;

                    if (cell.GetHallwayType() == HallwayCellInfo.HallwayType.NONE)
                    {
                        continue;
                    }

                    switch(cell.GetHallwayType())
                    {
                        case HallwayCellInfo.HallwayType.MAIN_ENTRY:    currentColor = Color.red;     break;
                        case HallwayCellInfo.HallwayType.STRAIGHT:      currentColor = Color.blue;    break;
                        case HallwayCellInfo.HallwayType.THREE_WAY:     currentColor = Color.green;   break;
                        case HallwayCellInfo.HallwayType.FOUR_WAY:      currentColor = Color.cyan;    break;
                        case HallwayCellInfo.HallwayType.CORNER:        currentColor = Color.magenta; break;
                    }

                    debugObjectList.Add((new Vector3Int(x, y, z), currentColor));
                }
            }
        }

        return debugObjectList;
    } 

    private (Vector3Int, Vector3Int) GetHallwayCoords(Vector3Int startPoint, Vector2Int hallwayDir)
    {
        Vector2Int startPoint2D = new(startPoint.x, startPoint.z);
        Vector2Int gridSize2D = new(_gridSize.x, _gridSize.z);
        Vector2Int endPoint2D = Vector2Int.Scale(startPoint2D, new Vector2Int(Mathf.Abs(hallwayDir.y), Mathf.Abs(hallwayDir.x)));

        if (hallwayDir.x > 0 || hallwayDir.y > 0)
        {
            endPoint2D += Vector2Int.Scale(gridSize2D, hallwayDir);
        }

        Vector3Int updatedStartPoint = startPoint + new Vector3Int(hallwayDir.x, 0, hallwayDir.y);
        Vector3Int updatedEndPoint = new (endPoint2D.x, startPoint.y, endPoint2D.y);

        return (updatedStartPoint, updatedEndPoint);
    }

    private void AddHallway(Vector3Int startPoint, Vector3Int endPoint, Vector2Int hallwayDir)
    {
        Vector3Int hallwayOffset = new(Mathf.Abs(hallwayDir.y), 0, Mathf.Abs(hallwayDir.x));
        Vector2Int hallwayEndOffset = Vector2Int.zero;
        if (endPoint.x == 0)
        {
            hallwayEndOffset.x = -1;
        }

        if (endPoint.z == 0)
        {
            hallwayEndOffset.y = -1;
        }

        for(int y = startPoint.z; y != endPoint.z + hallwayEndOffset.y + hallwayOffset.z; y += hallwayDir.y + hallwayOffset.z)
        {
            for (int x = startPoint.x; x != endPoint.x + hallwayEndOffset.x + hallwayOffset.x; x += hallwayDir.x + hallwayOffset.x)
            {
                var cell = _hallwayGridMap.GetCell(x, 0, y);
                cell.SetAsStraight();
                _hallwayGridMap.SetCell(x, 0, y, cell);
            }
        }
    }

    public void DrawSubRooms(Color color, float height, float yOffset)
    {
        foreach(var (anchor, size) in _BSPSplitRooms)
        {
            var boundAPos = _hallwayGridMap.GetWorldPosition(anchor.x, anchor.y, anchor.z);
            var boundBPos = _hallwayGridMap.GetWorldPosition(anchor.x + size.x, anchor.y + size.y, anchor.z + size.z);

            Vector3 subRoomSize = new(
                Mathf.Abs(boundAPos.x - boundBPos.x),
                height,
                Mathf.Abs(boundAPos.z - boundBPos.z)
            );

            Vector3 subRoomAnchor = new(
                Mathf.Min(boundAPos.x, boundBPos.x),
                yOffset,
                Mathf.Min(boundAPos.z, boundBPos.z)
            );

            Vector3 pointA = new (subRoomAnchor.x, subRoomAnchor.y, subRoomAnchor.z);
            Vector3 pointB = new (subRoomAnchor.x + subRoomSize.x, subRoomAnchor.y, subRoomAnchor.z);
            Vector3 pointC = new (subRoomAnchor.x + subRoomSize.x, subRoomAnchor.y, subRoomAnchor.z + subRoomSize.z);
            Vector3 pointD = new (subRoomAnchor.x, subRoomAnchor.y, subRoomAnchor.z + subRoomSize.z);

            Debug.DrawLine(pointA, pointB, color);
            Debug.DrawLine(pointB, pointC, color);
            Debug.DrawLine(pointC, pointD, color);
            Debug.DrawLine(pointD, pointA, color);
        }
    }

    private (Vector3Int, Vector3Int, Vector3Int) GetSubRoomInfo(
        int maxSize, int minSize, Vector3Int hallwayStartPoint, Vector3Int hallwayDir,
        Vector3Int hallwaySplitStartPoint, Vector3Int hallwaySplitEndPoint, Vector3Int hallwaySplitDir,
        bool reverseDir = false)
    {
        var updatedStart = hallwayStartPoint;
        Vector3Int reversedUpdatedEnd = Vector3Int.zero; 
        var hallwaySubDistanceFromStart = (int)Vector3Int.Distance(hallwaySplitStartPoint, updatedStart);
        for(int i = maxSize; i >= minSize; i /= 2)
        {
            if (i <= hallwaySubDistanceFromStart)
            {
                if(reverseDir)
                {
                    reversedUpdatedEnd = (hallwayDir * (hallwaySubDistanceFromStart - i));
                }
                else
                {
                    updatedStart += (hallwayDir * (hallwaySubDistanceFromStart - i));
                }
                break;
            }
        }

        Vector3Int boundA = updatedStart + hallwaySplitDir;
        
        var updatedEnd = hallwaySplitEndPoint;        
        if (updatedEnd.z == _gridSize.z)
        {
            updatedEnd.z -= 1;
        }
        
        if (updatedEnd.x == _gridSize.x)
        {
            updatedEnd.x -= 1;
        }

        var hallwaySubDistanceFromSplit = (int)Vector3Int.Distance(hallwaySplitStartPoint, updatedEnd);
        for(int i = maxSize; i >= minSize; i /= 2)
        {
            if (i <= hallwaySubDistanceFromSplit)
            {
                updatedEnd += (-hallwaySplitDir * (hallwaySubDistanceFromSplit - i));

                if (reverseDir)
                {
                    updatedEnd -= reversedUpdatedEnd;
                }
                break;
            }
        }

        Vector3Int boundB = updatedEnd - hallwayDir;

        Vector3Int subRoomSize = new(
            (int)Mathf.Abs(boundA.x - boundB.x),
            0,
            (int)Mathf.Abs(boundA.z - boundB.z)
        );

        Vector3Int subRoomAnchor = new(
            (int)Mathf.Min(boundA.x, boundB.x),
            (int)Mathf.Min(boundA.y, boundB.y),
            (int)Mathf.Min(boundA.z, boundB.z)
        );

        subRoomSize += new Vector3Int(Mathf.Abs(hallwayDir.x), Mathf.Abs(hallwayDir.y), Mathf.Abs(hallwayDir.z));
        Debug.Log(subRoomAnchor.ToString() + ", " + hallwaySplitDir.ToString());
        if (hallwaySplitDir.x < 0 || hallwaySplitDir.z < 0)
        {
            subRoomAnchor -= hallwaySplitDir;
        }
        Debug.Log(subRoomAnchor);


        return (subRoomAnchor, subRoomSize, updatedEnd);
    }
}
