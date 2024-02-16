using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Socket
{
    public int SocketID;
    public bool IsSymmetrical;
    public bool IsAsymmetrical;
    public bool IsFlippedSocket;

    public bool CanConnect(Socket socket)
    {
        return SocketID == socket.SocketID;
    }
} 

[System.Serializable]
public struct ChunkSockets
{
    public enum Direction
    {
        NO, QUART, HALF, TRIQUART 
    }

    public enum Face
    {
        PosX, PosZ, NegX, NegZ
    }

    public Socket PosXSocket;
    public Socket NegXSocket;
    public Socket PosZSocket;
    public Socket NegZSocket;
    public Direction CurrentDirection;

    public bool CanConnect(ChunkSockets chunkSockets, Face face)
    {   
        bool canConnect = false;
        switch(face)
        {
            case Face.PosX: canConnect = PosXSocket.CanConnect(chunkSockets.NegXSocket); break;
            case Face.PosZ: canConnect = PosZSocket.CanConnect(chunkSockets.NegZSocket); break;
            case Face.NegX: canConnect = NegXSocket.CanConnect(chunkSockets.PosXSocket); break;
            case Face.NegZ: canConnect = NegZSocket.CanConnect(chunkSockets.PosZSocket); break;
        }

        return canConnect;
    }
}

public class RoomChunks : MonoBehaviour
{
    public Vector3 RoomSize;
    public ChunkSockets Sockets;

    

    public ChunkSockets GetChunkSockets(ChunkSockets.Direction direction)
    {
        ChunkSockets chunkSockets = new()
        {
            CurrentDirection = direction
        };

        switch (direction)
        {
            case ChunkSockets.Direction.NO: chunkSockets = Sockets; break;
            case ChunkSockets.Direction.QUART: 
                chunkSockets.PosXSocket = Sockets.PosZSocket;
                chunkSockets.NegXSocket = Sockets.NegZSocket;
                chunkSockets.PosZSocket = Sockets.PosXSocket;
                chunkSockets.NegZSocket = Sockets.NegXSocket;
                break;
            case ChunkSockets.Direction.HALF: 
                chunkSockets.PosXSocket = Sockets.NegXSocket;
                chunkSockets.NegXSocket = Sockets.PosXSocket;
                chunkSockets.PosZSocket = Sockets.NegZSocket;
                chunkSockets.NegZSocket = Sockets.PosZSocket;
                break;
            case ChunkSockets.Direction.TRIQUART: 
                chunkSockets.PosXSocket = Sockets.NegZSocket;
                chunkSockets.NegXSocket = Sockets.PosZSocket;
                chunkSockets.PosZSocket = Sockets.NegXSocket;
                chunkSockets.NegZSocket = Sockets.PosXSocket;
                break;
        };

        return chunkSockets;
    }

}
