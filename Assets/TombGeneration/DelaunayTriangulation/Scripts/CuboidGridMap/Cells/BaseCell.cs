using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCell
{
    private Vector2 _CellSize;
    private Vector2Int _GridPosition;
    private Vector2 _Offset;

    public BaseCell(Vector2 cellSize, int x, int y, Vector2 offset)
    {
        _CellSize = cellSize;
        _GridPosition = new Vector2Int(x, y);
        _Offset = offset;
    }

    public Vector2 GetCellCentrePos()
    {
        
        Vector2 relPos =  new(
            ((float)_GridPosition.x * _CellSize.x) + (_CellSize.x / 2f),
            ((float)_GridPosition.y * _CellSize.y) + (_CellSize.y / 2f)
        );

        return relPos + _Offset;
    }

    public Vector2 GetCellPos()
    {
        Vector2 relPos =  new(
            ((float)_GridPosition.x * _CellSize.x),
            ((float)_GridPosition.y * _CellSize.y)
        );

        return relPos + _Offset;
    }

    public Vector2Int GetGridPosition()
    {
        return _GridPosition;
    }

    public Vector2 GetCellSize()
    {
        return _CellSize;
    }

    public Vector2 GetOffset()
    {
        return _Offset;
    }


    public void DrawDebugLines(Color color)
    {
        Vector2 cellPos = GetCellPos();
        Vector3 cellWorldPosA = new( cellPos.x, 0f, cellPos.y );
        Vector3 cellWorldPosB = new( cellPos.x + _CellSize.x, 0f, cellPos.y );
        Vector3 cellWorldPosC = new( cellPos.x + _CellSize.x, 0f, cellPos.y + _CellSize.y );
        Vector3 cellWorldPosD = new( cellPos.x, 0f, cellPos.y + _CellSize.y );

        Debug.DrawLine(cellWorldPosA, cellWorldPosB, color);
        Debug.DrawLine(cellWorldPosB, cellWorldPosC, color);
        Debug.DrawLine(cellWorldPosC, cellWorldPosD, color);
        Debug.DrawLine(cellWorldPosD, cellWorldPosA, color);
    }

    public void DrawCentreLines(Color color)
    {
        Vector2 centrePos = GetCellCentrePos();
        Vector3 centreWorldPos = new( centrePos.x, 0f, centrePos.y );
        Vector3 centreWorldPosAbove = new( centrePos.x, 10f, centrePos.y );

        Debug.DrawLine(centreWorldPos, centreWorldPosAbove, color);
    }
}
