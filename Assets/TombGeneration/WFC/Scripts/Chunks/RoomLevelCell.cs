using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RoomLevelCell : MonoBehaviour
{
    private List<(int, ChunkSockets)> _chunkSockets = new();
    private System.Random _rand = new();
    private (int, ChunkSockets) _collapedChunkSockets = new(-1, new ChunkSockets());

    public void AddChunkSockets(int chunkID, RoomChunks roomChunk)
    {
        if (chunkID < 0)
        {
            throw new InvalidDataException("'chunkID' must be greater than 0");
        }

        var chunkSocketDirection = Enum.GetValues(typeof(ChunkSockets.Direction))
            .Cast<ChunkSockets.Direction>();
        
        foreach (ChunkSockets.Direction dir in chunkSocketDirection)
        {
            ChunkSockets chunkSocketFromDir = roomChunk.GetChunkSockets(dir);
            (int, ChunkSockets) chunkSocket = (chunkID, chunkSocketFromDir);
            _chunkSockets.Add(chunkSocket);
        }
    }

    public void CollapseToASpecific(int chunkID, RoomChunks roomChunk)
    {
        ChunkSockets chunkSocketFromDir = roomChunk.GetChunkSockets(ChunkSockets.Direction.NO);
        _collapedChunkSockets = (chunkID, chunkSocketFromDir);
    }

    public bool IsCollapsed()
    {
        return _collapedChunkSockets.Item1 != -1;
    }

    public int GetEntropy()
    {
        if (IsCollapsed()) return 1;
        return _chunkSockets.Count;
    }

    public (int, ChunkSockets) CollapseCell()
    {
        if (!IsCollapsed())
        {
            int randIndex = _rand.Next(0, _chunkSockets.Count);
            _collapedChunkSockets = _chunkSockets[randIndex];
        }

        return _collapedChunkSockets;
    }


    public (int, ChunkSockets) GetCollapsedCell()
    {
        return _collapedChunkSockets;
    }

    public void UpdateChunkList(ChunkSockets collapsedCell, ChunkSockets.Face socketToCheck)
    {
        for (int i = _chunkSockets.Count - 1; i > -1; i--)
        {
            if (!_chunkSockets[i].Item2.CanConnect(collapsedCell, socketToCheck))
            {
                _chunkSockets.RemoveAt(i);
            }
        }
    }
}
