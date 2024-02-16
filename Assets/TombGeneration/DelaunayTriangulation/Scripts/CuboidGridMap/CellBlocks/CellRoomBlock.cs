using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellRoomBlock : MonoBehaviour
{
    public Vector3 RoomSize;

    public Vector2Int GetSizeInGrid(Vector2 cellSize)
    {
        return new Vector2Int(
            (int)(RoomSize.x / cellSize.x),
            (int)(RoomSize.z / cellSize.y) 
        );
    }

    public Vector3 GetCenteredWorldPosition()
    {
        return (RoomSize / 2f) + gameObject.transform.position;
    }

    public Vector2 GetCenteredWorldPosition2D()
    {
        return new Vector2(
            GetCenteredWorldPosition().x,
            GetCenteredWorldPosition().z
        );
    }

    public Vector2 GetClosestDoorPosition(Vector2 position)
    {
        DoorHandler dh = gameObject.GetComponent<DoorHandler>();
        return dh.SetClosestPointToDoor2D(position);
    }
}
