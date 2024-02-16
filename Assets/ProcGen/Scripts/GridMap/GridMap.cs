using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
///     The Generic Grid Map contains a 3d matrix of cells that does a certain function
///     and holds certain data.
///     
///     The structure of the cells in the gridmap is initialised by
///     - GridMap[y,z,x]
///     
///     The cell at the cell position [0, 0, 0] is always at the game objects
///     transform position is at
/// 
/// </summary>
/// <typeparam name="T">Any object type</typeparam>
public class GridMap<T>
{
    private Vector3Int _GridSize;
    private Vector3 _CellSize;
    private Vector3 _Origin;

    private T[,,] _GridMap;

    public GridMap(Vector3Int gridSize, Vector3 cellSize, Vector3 origin, Func<T> cellConstructor)
    {
        _Origin = origin;
        _GridSize = gridSize;
        _CellSize = cellSize;

        _GridMap = new T[gridSize.y, gridSize.z, gridSize.x];

        for (int y = 0; y < _GridSize.y; y++)
        {
            for (int z = 0; z < _GridSize.z; z++)
            {
                for (int x = 0; x < _GridSize.x; x++)
                {
                    _GridMap[y,z,x] = cellConstructor();
                }
            }
        }
    }

    public Vector3 GetCellPositionFromWorld(Vector3 worldPosition)
    {
        Vector3 LocalPosition = worldPosition - _Origin;

        return new Vector3(
            LocalPosition.x / _CellSize.x ,
            LocalPosition.y / _CellSize.y ,
            LocalPosition.z / _CellSize.z 
        );
    }


    public Vector3 GetWorldPosition(int x, int y, int z)
    {
        Vector3 LocalPosition = Vector3.Scale(new Vector3Int(x, y, z), _CellSize);
        
        return _Origin + LocalPosition;
    }

    public T GetCell(int x, int y, int z)
    {
        if ((x >= 0 && x < _GridSize.x) &&
            (y >= 0 && y < _GridSize.y) &&
            (z >= 0 && z < _GridSize.z))
        {
            return _GridMap[y, z, x];
        }

        return default(T);
    }

    public void SetCell(int x, int y, int z, T cell)
    {
        if ((x >= 0 && x < _GridSize.x) &&
            (y >= 0 && y < _GridSize.y) &&
            (z >= 0 && z < _GridSize.z))
        {
            _GridMap[y, z, x] = cell;
        }
    }

    public Vector3Int GetGridSize()
    {
        return _GridSize;
    }

    public Vector3 GetCellSize()
    {
        return _CellSize;
    }

    public void DrawDebugLines(Color color)
    {

        for(int y = 0; y < _GridSize.y; y++)
        {
            for(int z = 0; z < _GridSize.z; z++)
            {
                for(int x = 0; x < _GridSize.x; x++)
                {
                    DrawCell(x, y, z, color);
                }
            }
        }
    }

    public void DrawCell(int x, int y, int z, Color color)
    {
        Vector3 CurrCellPos = new((float)x, (float)y, (float)z);
        Vector3 CurrPos = _Origin + Vector3.Scale(CurrCellPos, _CellSize);

        Vector3 PointA = CurrPos + new Vector3(0f, 0f, _CellSize.z);
        Vector3 PointB = CurrPos + new Vector3(_CellSize.x, 0f, _CellSize.z);
        Vector3 PointC = CurrPos + new Vector3(_CellSize.x, 0f, 0f);

        Debug.DrawLine(CurrPos, PointA, color);
        Debug.DrawLine(PointA, PointB, color);
        Debug.DrawLine(PointB, PointC, color);
        Debug.DrawLine(PointC, CurrPos, color);
    }
}
