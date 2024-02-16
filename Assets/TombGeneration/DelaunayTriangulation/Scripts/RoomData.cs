using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public Vector2Int RoomSize;
    [Range(0f, 1f)] 
    public float RoomSpawnWeight; 

    

    public int GetRoomArea()
    {
        return RoomSize.x * RoomSize.y;
    }

}
