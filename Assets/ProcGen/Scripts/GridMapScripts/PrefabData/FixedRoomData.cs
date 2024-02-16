using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedRoomData : MonoBehaviour
{
    public Vector3Int RoomSize;
    public Vector3 CellSize;

    public List<Vector3Int> DoorPositions;

    public bool IsEqual(Vector3Int roomSize)
    {
        return (RoomSize == roomSize);
    }

    public List<Vector3Int> GetDoorPositions(Vector3Int offset)
    {
        List<Vector3Int> listOfOffsettedDoorPos = new();

        foreach(var dp in DoorPositions)
        {   
            Vector3Int offsettedDP = dp + offset;
            listOfOffsettedDoorPos.Add(offsettedDP);
        }

        return listOfOffsettedDoorPos;
    }
}
