using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandler : MonoBehaviour
{
    public GameObject DoorObjects;
    private bool _isDoorHandleInit = false;
    private List<DoorState> _doorStates = new();

    private List<int> _isWallIndex = new();
    private List<int> _isDoorIndex = new();

    private void InitialiseDoorHandler()
    {
        if (!_isDoorHandleInit)
        {
            if (_doorStates.Count == 0)
            {
                var childCount = DoorObjects.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    DoorState dr = DoorObjects.transform.GetChild(i).GetComponent<DoorState>();
                    _doorStates.Add(dr);
                    _isWallIndex.Add(i);
                }
            }

            _isDoorHandleInit = true;
        }
    }

    public void CollapseAllToWall()
    {
        int setCount = 0;
        foreach(int di in _isDoorIndex)
        {
            print(di);
            _doorStates[di].SetToDoor();
        }

        foreach(int wi in _isWallIndex)
        {
            _doorStates[wi].SetToWall();
        }

        print("[DH] - " + setCount.ToString());
    }

    public Vector2 SetClosestPointToDoor2D(Vector2 point2D)
    {
        InitialiseDoorHandler();
        Vector3 point = new(point2D.x, 0f, point2D.y);

        DoorState closestDoor = null;
        int lowestIndex = -1;
        for(int i = 0; i < _doorStates.Count; i++)
        {
            if (closestDoor == null)
            {
                closestDoor = _doorStates[i];
                lowestIndex = i;
                continue;
            }

            Vector3 currentDoorPos = _doorStates[i].gameObject.transform.position;
            Vector3 closestDoorPos = closestDoor.gameObject.transform.position;
            
            if (Vector3.Distance(currentDoorPos, point) < Vector3.Distance(closestDoorPos, point))
            {
                closestDoor = _doorStates[i];
                lowestIndex = i;
            }
        }

        bool isInDoor = false;
        foreach(int i in _isDoorIndex)
        {
            if (lowestIndex == i)
            {
                isInDoor = true;
                break;
            }
        }

        if (!isInDoor)
        {
            _isDoorIndex.Add(lowestIndex);
            _isWallIndex.Remove(lowestIndex);
        }
        
        return new Vector2(
            closestDoor.gameObject.transform.position.x,
            closestDoor.gameObject.transform.position.z
        );
    } 

}
