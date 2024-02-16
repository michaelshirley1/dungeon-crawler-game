using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BSPData : IComparable<int>
{
    private Vector2Int _size = Vector2Int.zero;
    private Vector2Int _anchor = Vector2Int.zero;

    public BSPData(Vector2Int size, Vector2Int anchor)
    {
        _size = size;
        _anchor = anchor;
    }

    public (BSPData, BSPData) SplitPolygon()
    {   
        Vector2Int updatedSize = _size;
        Vector2Int updatedAnchor = _anchor;

        if (_size.x >= _size.y)
        {
            updatedSize.x = _size.x / 2;
            updatedAnchor.x += updatedSize.x;
        }
        else
        {
            updatedSize.y = _size.y / 2;
            updatedAnchor.y += updatedSize.y;
        }
        
        return new(
            new BSPData(updatedSize, _anchor),
            new BSPData(updatedSize, updatedAnchor)
        );
    }

    public Vector2Int GetSize()
    {
        return _size;
    }

    public Vector2Int GetAnchor()
    {
        return _anchor;
    }

    public int CompareTo(int other)
    {
        if (_size.x <= _size.y)
        {
            return _size.x.CompareTo(other);  
        }

        return _size.y.CompareTo(other);
    }

    public static bool operator >  (BSPData operand1, int operand2)
    {
        return operand1.CompareTo(operand2) > 0;
    }

    public static bool operator <  (BSPData operand1, int operand2)
    {
        return operand1.CompareTo(operand2) < 0;
    }

    public static bool operator >=  (BSPData operand1, int operand2)
    {
        return operand1.CompareTo(operand2) >= 0;
    }

    public static bool operator <=  (BSPData operand1, int operand2)
    {
        return operand1.CompareTo(operand2) <= 0;
    }

    public override string ToString()
    {
        return "Size: " + _size.ToString() + ", Anchor: " + _anchor.ToString();
    }
}

public class BSPNode
{
    public BSPData Value  { get; private set; }
    public BSPNode Left { get; private set; } = null!;
    public BSPNode Right { get; private set; } = null!;

    private bool _isLeftLeaf = false;
    private bool _isRightLeaf = false;
    private bool _finalNode = false;

    public BSPNode(BSPData data, int minTarget, bool isRoot = false)
    {
        Value = data;
        var splitData = data.SplitPolygon();
        System.Random rand = new();
        var choice = rand.NextDouble();
        
        var splitSizeA = splitData.Item1.GetSize();
        var splitSizeB = splitData.Item2.GetSize();
        var nodeSize = data.GetSize();


        if ((choice < 0.5 && !isRoot && 
            nodeSize.x <= 12 && nodeSize.y <= 12) || 
            (splitData.Item1 < minTarget && splitData.Item2 < minTarget))
        {

            _finalNode = true;
            _isLeftLeaf = true;
            _isRightLeaf = true;
            return;
        }

        if (splitData.Item1 >= minTarget)
        {
            Left ??= new BSPNode(splitData.Item1, minTarget);
        }
        else
        {
            _isLeftLeaf = true;
        }

        if (splitData.Item2 >= minTarget)
        {
            Right ??= new BSPNode(splitData.Item2, minTarget);
        }
        else
        {
            _isRightLeaf = true;
        }

        
    }

    public BSPData GetData()
    {
        return Value;
    }

    public List<BSPData> GetAllData(List<BSPData> bsp = null)
    {
        bsp ??= new();
        if (_finalNode)
        {
            bsp.Add(Value);
        }
        
        if (!_isLeftLeaf)
        {
            bsp.Concat(Left.GetAllData(bsp));
        }

        if (!_isRightLeaf)
        {
            bsp.Concat(Right.GetAllData(bsp));
        }

        return bsp;
    }

    public bool IsLeftLeaf()
    {
        return _isLeftLeaf;
    }

    public bool IsRightLeaf()
    {
        return _isRightLeaf;
    }
};


public class BSPTree
{
    private int _minSize;
    private BSPNode _Root { get; set; } = null!;

    public BSPTree(Vector2Int size, Vector2Int anchor, int minSize)
    {
        _minSize = minSize;
        BSPData rootData = new(size, anchor);

        _Root = new(rootData, minSize, true);
    }

    public List<BSPData> GetAllData()
    {
        return _Root.GetAllData();
    }
}
