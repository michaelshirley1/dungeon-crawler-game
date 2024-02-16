using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRoomDebugger : MonoBehaviour
{
    public class DebugCell
    {
        private Color _color;
        private GameObject _gameObject = null;
        
        public DebugCell()
        {
            _color = Color.white;
        }

        public void SetColor(Color color)
        {
            _color = color;
        }


        public void SpawnObject(GameObject gameObjectToSpawn, Vector3 position, Transform parent)
        {
            if (_gameObject == null)
            {
                _gameObject = Instantiate(gameObjectToSpawn, position, Quaternion.identity, parent);

                Material instancedMat = _gameObject.transform.Find("DebugMesh").GetComponent<Renderer>().material;
                instancedMat.SetColor("_Color", _color);
            }
        }

        public void DestroyObject()
        {
            if (_gameObject != null)
            {
                Destroy(_gameObject);
                _gameObject = null;
            }
        }

    }

    public GameObject DebugObject;
    public bool DrawLines;

    private Vector3Int _GridSize;
    private Vector3 _CellSize;
    private GridMap<DebugCell> _debugGridMap;
    private bool _hasInitialised = false;

    void Update()
    {
       if (_hasInitialised && DrawLines)
       {
            _debugGridMap.DrawDebugLines(Color.red);
       }
    }

    public void InitialiseDebugMap(Vector3Int GridSize, Vector3 CellSize)
    {
        _GridSize = GridSize;
        _CellSize = CellSize;
        _debugGridMap = new(_GridSize, _CellSize, transform.position, () => { return new DebugCell(); });

        _hasInitialised = true;
    }
    
    public void SpawnDebugObject(int x, int y, int z, Color color)
    {
        if (!_hasInitialised)
        {
            return;
        }

        var cell = _debugGridMap.GetCell(x, y, z);
        var worldPosition = _debugGridMap.GetWorldPosition(x, y, z);
        cell.SetColor(color);
        cell.SpawnObject(DebugObject, worldPosition, transform);
        _debugGridMap.SetCell(x, y, z, cell);
    }

    public void DestroyDebugObject(int x, int y, int z)
    {
        if (!_hasInitialised)
        {
            return;
        }

        var cell = _debugGridMap.GetCell(x, y, z);
        cell.DestroyObject();
        _debugGridMap.SetCell(x, y, z, cell);
    }
}
