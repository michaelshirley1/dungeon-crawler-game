using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBuilderCell
{
    public enum State
    {
        HALLWAY, FREE, FILLED, DOOR, CORNER, SPECIAL
    }

    private State _CurrentState;
    private bool _InitDone;

    public RoomBuilderCell()
    {
        _CurrentState = State.FREE;
        _InitDone = false;
    }

    public State GetState()
    {
        return _CurrentState;
    }

    public void SetToFilled()
    {
        _CurrentState = State.FILLED;
    }

    public void SetToHallway(bool isCorner = false)
    {
        if (isCorner)
        {
            _CurrentState = State.CORNER;
            return;
        }
        
        _CurrentState = State.HALLWAY;
    }

    public bool IsInit()
    {
        return _InitDone;
    }

    public void SetInit()
    {
        _InitDone = true;
    }

    public void SetAsSpecial()
    {
        _CurrentState = State.SPECIAL;
    }
}

public class DebugGrid : MonoBehaviour
{
    public Vector3Int GridSize;
    public Vector3 CellSize;
    public Vector2Int MinRoomSize;
    public Vector2Int MaxRoomSize;
    public GameObject DebugBlock;
    public GameObject RoomBuilderPrefab;

    public int SubdivideCellBy;

    private GridRoomGenerator _RoomGenerator;
    private List<GameObject> _InstantiatedRooms = new();
    private List<GridRoomBuilder> _RoomBuilders = new();
    private bool _IsDone = false; 

    private System.Random _RandomEngine = new();
    

    void Start()
    {
        _RoomGenerator = new GridRoomGenerator(GridSize, CellSize, transform.position, MinRoomSize, MaxRoomSize);
        
        while(!_IsDone)
        {
            _RoomGenerator.Step();
            _IsDone = _RoomGenerator.IsGenerationDone();
        }

        foreach(var ri in _RoomGenerator.GetRoomInfos())
        {
            var instantiatedRoom = Instantiate(RoomBuilderPrefab, ri.anchor3D, Quaternion.identity, transform);
            var debugBuilder = instantiatedRoom.GetComponent<DebugBuilder>();
            debugBuilder.GenerateRooms(ri);
            _InstantiatedRooms.Add(instantiatedRoom);
        }
    }
    
    
    void Update()
    {
        foreach(var rb in _RoomBuilders)
        {
            rb.DrawDebugLines(Color.cyan);
            rb.DrawSubRooms(Color.red, CellSize.y, 30.1f);
        }
    }
}
