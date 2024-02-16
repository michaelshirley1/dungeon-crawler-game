using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBuilder : MonoBehaviour
{
    GridRoomBuilder _roomBuilder;
    GridRoomDebugger _debugger;
    
    public GameObject _debuggerObject;
    public Vector2Int RoomSize;
    public Vector2 CellSize;
    public int SubdivideBy;

    private bool _isInit = false;


    public void GenerateRooms(GridRoomGenerator.RoomInfo roomInfo)
    {
        _roomBuilder = new(roomInfo, SubdivideBy, 3.0f);
        _debugger = _debuggerObject.GetComponent<GridRoomDebugger>();
        _debugger.InitialiseDebugMap(_roomBuilder.GetGridSize(), _roomBuilder.GetCellSize());

        foreach(var info in _roomBuilder.GetDebugCellInfo())
        {
            _debugger.SpawnDebugObject(info.Item1.x, info.Item1.y, info.Item1.z, info.Item2);
        }

        _isInit = true;
    }


    void Update()
    {
        if (_isInit)
        {
            _roomBuilder.DrawDebugLines(Color.green);
            _roomBuilder.DrawSubRooms(Color.red, 3.0f, 1.0f);
        }
    }
}
