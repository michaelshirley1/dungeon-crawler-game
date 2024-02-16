using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OccupancyGridObject
{
    private State _CurrentState;

    public enum State
    {
        FILLED,
        FREE
    }

    public OccupancyGridObject()
    {
        _CurrentState = State.FREE;
    }

    public void SetAsFilled()
    {
        _CurrentState = State.FILLED;
    }

    public State GetState()
    {
        return _CurrentState;
    } 
}
