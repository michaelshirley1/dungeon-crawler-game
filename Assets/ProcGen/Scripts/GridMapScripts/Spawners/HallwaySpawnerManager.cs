using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallwaySpawnerManager : MonoBehaviour
{
    private class HallwayCell
    {
        private GameObject _instantiatedObject;
        private GameObject _prefabObject;
        private Vector3 _position;
        private Quaternion _rotation;
        private Transform _parent;

        public HallwayCell()
        {
            _instantiatedObject = null;
            _prefabObject = null;
        }

        public void ConfigureHallway(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            _prefabObject = prefab;
            _position = position;
            _rotation = rotation;
            _parent = parent;
        }

        public void SpawnHallway()
        {
            if (_prefabObject == null)
            {
                return;
            }

            if (_instantiatedObject == null)
            {
                _instantiatedObject = Instantiate(_prefabObject, _position, _rotation, _parent);
            }
        }

        public void DestroyHallway()
        {
            if (_instantiatedObject != null)
            {
                Destroy(_instantiatedObject);
            }
        }
    }

    public List<GameObject> HallwayPrefabs;
    private List<HallwayData> _HallwayData = new();

    private int _hallwayLevel;
    private GridMap<HallwayCell> _hallwayMap;
    private bool _isInitialised = false;
    private bool _isSpawned = false;

    public void Initialise(Vector3Int gridSize, Vector3 cellSize, int hallwayLevel)
    {
        if (!_isInitialised)
        {
            foreach(var hp in HallwayPrefabs)
            {
                if (hp == null)
                {
                    Debug.LogError("Empty Game Object");
                    continue;
                }

                if (hp.TryGetComponent<HallwayData>(out var hallwayData))
                {
                    _HallwayData.Add(hallwayData);
                }
                else
                {
                    Debug.LogError("GameObject of name '" + hp.name.ToString() + "' does not have the HallwayData component configured.");
                }
            }
        }

        if (_isSpawned)
        {
            var currentGridSize = _hallwayMap.GetGridSize();
            for(int y = 0; y < currentGridSize.z; y++)
            {
                for(int x = 0; x < currentGridSize.x; x++)
                {
                    var cell = _hallwayMap.GetCell(x, 0, y);
                    cell.DestroyHallway();
                    _hallwayMap.SetCell(x, 0, y, cell);
                }
            }
        }

        _hallwayLevel = hallwayLevel;

        Vector3Int singleLevelGridSize = new(gridSize.x, 1, gridSize.z);
        Vector3 hallwayOrigin = transform.position + new Vector3(0, cellSize.y * (float)hallwayLevel, 0);
        _hallwayMap = new GridMap<HallwayCell>(singleLevelGridSize, cellSize, hallwayOrigin, () => { return new HallwayCell(); });
        
        _isInitialised = true;
        _isSpawned = false;
    }

    public void ConfigureHallway(int x, int y, HallwayData.Config config)
    {
        if (!_isInitialised)
        {
            return;
        }

        List<(HallwayData, Vector3, Quaternion)> PossibleHallways = new();

        foreach(var hd in _HallwayData)
        {
            if (hd.GetPositionOffsetAndRotation(config, out Vector3 hallwayOffset, out Quaternion hallwayRot))
            {
                PossibleHallways.Add((hd, hallwayOffset, hallwayRot));
            }
        }

        if (PossibleHallways.Count == 0)
        {
            print("No Possible Hallways!");
            return;
        }
        
        System.Random rand = new();
        var chosenHallway = PossibleHallways[rand.Next(PossibleHallways.Count)];
        print((x, y));
        Vector3 cellPosition = _hallwayMap.GetWorldPosition(x, 0, y);

        var cell = _hallwayMap.GetCell(x, 0, y);
        cell.ConfigureHallway(
            chosenHallway.Item1.gameObject, 
            cellPosition + chosenHallway.Item2,
            chosenHallway.Item3, 
            transform
        );
        _hallwayMap.SetCell(x, 0, y, cell);
    }

    public void SpawnHallways()
    {
        if (!_isInitialised)
        {
            return;
        }

        Vector3Int currentGridSize = _hallwayMap.GetGridSize();
        for(int y = 0; y < currentGridSize.z; y++)
        {
            for(int x = 0; x < currentGridSize.x; x++)
            {
                var cell = _hallwayMap.GetCell(x, 0, y);
                cell.SpawnHallway();
                _hallwayMap.SetCell(x, 0, y, cell);
            }
        }

        _isSpawned = true;
    }

    public bool IsSpawned()
    {
        return _isSpawned;
    }

    public void DrawDebugLines(Color color)
    {
        if (_isInitialised)
        {
            _hallwayMap.DrawDebugLines(color);
        }
    }

}
