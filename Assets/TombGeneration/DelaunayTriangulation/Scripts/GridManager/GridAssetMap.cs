using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAssetMap : MonoBehaviour
{
    public Vector3 CellSize;
    public Vector3Int GridSize;

    public Vector3 GetCenteredWorldPosition()
    {
        Vector3 Midpoint = new (
            CellSize.x * GridSize.x,
            CellSize.y * GridSize.y,
            CellSize.z * GridSize.z
        );
        
        return (Midpoint / 2f) + gameObject.transform.position;
    }

    public Vector3Int GetGridPosition()
    {
        Vector3 worldPosition = gameObject.transform.position;
        Vector3Int gridPosition = new(
            (int)((worldPosition.x / CellSize.x) + (0.5f)),
            (int)((worldPosition.y / CellSize.y) + (0.5f)),
            (int)((worldPosition.z / CellSize.z) + (0.5f))
        );
        
        return gridPosition;
    }

    public Vector2Int GetPositionOfNearestDoor(Vector3 centeredPos)
    {
        Vector3 Midpoint = new (
            centeredPos.x + ((CellSize.x * GridSize.x) / 2f),
            centeredPos.y + ((CellSize.y * GridSize.y) / 2f),
            centeredPos.z + ((CellSize.z * GridSize.z) / 2f)
        );

        Vector2Int gridPosition = new(
            (int)((Midpoint.x / CellSize.x) + (0.5f)),
            (int)((Midpoint.z / CellSize.z) + (0.5f))
        );

        return gridPosition;
    }
}
