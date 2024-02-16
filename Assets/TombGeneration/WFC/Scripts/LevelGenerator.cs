using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public List<GameObject> RoomChunksPrefabs;
    public GameObject EdgeRoomChunk;
    public Vector2Int GridSize;
    public Vector2 CellSize;

    private RoomLevelCell[,] _roomLevelCells;

    private int XValue = 1;
    private int YValue = 1;
    private double Duration = 0.0;

    void Start()
    {
        _roomLevelCells = new RoomLevelCell[GridSize.x, GridSize.y];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {   
                RoomLevelCell cell = new();
                
                for (int i = 0; i < RoomChunksPrefabs.Count; i++)
                {
                    RoomChunks roomChunk = RoomChunksPrefabs[i].GetComponent<RoomChunks>();
                    cell.AddChunkSockets(i, roomChunk);
                }

                RoomChunks edgeRoomChunk = EdgeRoomChunk.GetComponent<RoomChunks>();
                cell.AddChunkSockets(RoomChunksPrefabs.Count, edgeRoomChunk);

                if (x == 0 || x == (GridSize.x - 1) || y == 0 || y == (GridSize.y - 1))
                {
                    cell.CollapseToASpecific(RoomChunksPrefabs.Count, edgeRoomChunk);
                }

                _roomLevelCells[x, y] = cell;
            }
        }

        for (int x = 1; x < (GridSize.x - 1); x++)
        {
            for (int y = 1; y < (GridSize.y - 1); y++)
            {
                AddBlock(x, y);
            }
        }
    }

    void Update()
    {
       
    }
    

    private void AddBlock(int x, int y)
    {
        // Update this cell by looking at any collapsed cell within its surroundings
        if (_roomLevelCells[x - 1, y].IsCollapsed())
        {
            var collapsedCell = _roomLevelCells[x - 1, y].GetCollapsedCell();
            _roomLevelCells[x, y].UpdateChunkList(collapsedCell.Item2, ChunkSockets.Face.NegX);
        }

        if (_roomLevelCells[x, y - 1].IsCollapsed())
        {
            var collapsedCell = _roomLevelCells[x - 1, y].GetCollapsedCell();
            _roomLevelCells[x, y].UpdateChunkList(collapsedCell.Item2, ChunkSockets.Face.NegZ);
        }

        if (_roomLevelCells[x + 1, y].IsCollapsed())
        {
            var collapsedCell = _roomLevelCells[x - 1, y].GetCollapsedCell();
            _roomLevelCells[x, y].UpdateChunkList(collapsedCell.Item2, ChunkSockets.Face.PosX);
        }

        if (_roomLevelCells[x, y + 1].IsCollapsed())
        {
            var collapsedCell = _roomLevelCells[x - 1, y].GetCollapsedCell();
            _roomLevelCells[x, y].UpdateChunkList(collapsedCell.Item2, ChunkSockets.Face.PosZ);
        }

        var collapsedCellInfo = _roomLevelCells[x, y].CollapseCell();
        GameObject collapsedCellPrefab;
        if (collapsedCellInfo.Item1 == RoomChunksPrefabs.Count)
        {
            collapsedCellPrefab = EdgeRoomChunk;
        }
        else
        {
            collapsedCellPrefab = RoomChunksPrefabs[collapsedCellInfo.Item1];
        }
        Vector3 prefabPos = new (CellSize.y * y, 0f, CellSize.x * x);
        Quaternion prefabRot = Quaternion.Euler(new Vector3(-90f, 0f, 0f));

        switch(collapsedCellInfo.Item2.CurrentDirection)
        {
            case ChunkSockets.Direction.NO:         prefabRot = Quaternion.Euler(new Vector3(-90f, 0f, 0f)); break;
            case ChunkSockets.Direction.QUART:      prefabRot = Quaternion.Euler(new Vector3(-90f, 90f, 0f)); break;
            case ChunkSockets.Direction.HALF:       prefabRot = Quaternion.Euler(new Vector3(-90f, 180f, 0f)); break;
            case ChunkSockets.Direction.TRIQUART:   prefabRot = Quaternion.Euler(new Vector3(-90f, 270f, 0f)); break;
        }

        Instantiate(collapsedCellPrefab, prefabPos, prefabRot);
    }
}
