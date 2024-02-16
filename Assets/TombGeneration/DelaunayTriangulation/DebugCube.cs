using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DebugCube : MonoBehaviour
{
    private Vector2Int _gridPos;
    public TMP_Text Gscore;
    public TMP_Text Hscore;
    public TMP_Text Fscore;

    public void ClearText()
    {
        Gscore.text = "";
        Hscore.text = "";
        Fscore.text = "";
    }
    
    public void SetColor(Color color)
    {
        MeshRenderer meshRenderer = gameObject.transform.Find("TestCube").GetComponent<MeshRenderer>();
        meshRenderer.material.color = color;
    }

    public void SetGridPos(Vector2Int pos)
    {
        _gridPos = pos;
    }

    public Vector2Int GetGridPos()
    {
        return _gridPos;
    }

    public void UpdateScores(float gScore, float hScore, float fScore)
    {
        Gscore.text = gScore.ToString("F0");
        Hscore.text = hScore.ToString("F0");
        Fscore.text = fScore.ToString("F0");
    }

}
