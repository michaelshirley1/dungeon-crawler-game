using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
    public enum State
    {
        OCCUPIED, FREE, PADDING, DOOR, HALLWAY
    }

    public State _currentState = State.FREE;

    public bool IsObstacle()
    {
        return _currentState != State.FREE;
    }

    public void AddHallway()
    {
        _currentState = State.HALLWAY;
    }
}
