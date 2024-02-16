using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubSectionTest : MonoBehaviour
{
    public GameObject SingleEntranceSpawner;

    [Header("Sub Section Configurations")]
    public Vector3Int GridSize;
    public Vector3 CellSize;
    public Vector3Int EntryPoint;

    public int maxRoomSize;

    private SingleEntranceSpawnStrategy _spawnStrategy;
    private System.Random _randomEngine = new();

    void Start()
    {
        if (SingleEntranceSpawner == null)
        {
            throw new ArgumentNullException("Single Entrance Spawner is not set to a Game Object");
        }

        SingleEntranceSpawner.GetComponent<SingleEntranceSpawnStrategy>();

        var instancedGameObject = Instantiate(SingleEntranceSpawner, transform);
        if (!instancedGameObject.TryGetComponent<SingleEntranceSpawnStrategy>(out _spawnStrategy))
        {
            throw new ArgumentException("Game Object of name '" + SingleEntranceSpawner.name + "' has no component of type SingleEntranceSpawnStrategy");
        }

        double choice = _randomEngine.NextDouble();
        double flipChoice = _randomEngine.NextDouble();
        
        int compDir = 1;
        int randPos = 0;
        Vector2Int randDir = Vector2Int.zero;
        
        if (flipChoice >= 0.5)
        {
            compDir = -1;
        }

        if (choice >= 0.5)
        {
            randDir.x = compDir;
            randPos = _randomEngine.Next(3, GridSize.z - 3);
        }
        else
        {
            randDir.y = compDir;
            randPos = _randomEngine.Next(3, GridSize.x - 3);
        }

        _spawnStrategy.Initialise(GridSize, CellSize, 3, 24, randDir, randPos);
    }

    void Update()
    {
    }

}
