using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Cell
{
    public enum State
    {
        UNOCCUPIED, OCCUPIED, HALLWAY, PADDING, DOOR
    }

    private State _currentCellState = State.UNOCCUPIED;
    
    

};

public class GridManager : MonoBehaviour
{
    public Vector2Int GridSize;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
