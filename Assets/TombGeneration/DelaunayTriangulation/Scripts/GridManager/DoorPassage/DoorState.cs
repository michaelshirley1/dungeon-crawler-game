using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorState : MonoBehaviour
{
    private bool isSet = false;
    public GameObject Wall;
    public GameObject Door;

    public void SetToDoor()
    {
        Door.SetActive(true);
        Wall.SetActive(false);
    }

    public void SetToWall()
    {
        Wall.SetActive(true);
        Door.SetActive(false);
    }

    public bool IsSet()
    {
        return isSet;
    }

    public Vector2Int GetDoorDirection()
    {
        if (gameObject.transform.eulerAngles.y == 90f) return new Vector2Int(-1, 0);
        if (gameObject.transform.eulerAngles.y == 0f) return new Vector2Int(0, -1);
        if (gameObject.transform.eulerAngles.y == 180f) return new Vector2Int(0, 1);

        return new Vector2Int(1, 0);
    }
}
