// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;

// public class LayoutGenerator : MonoBehaviour
// {
//     public GameObject player;
//     private CuboidGridMap _cuboidGridMap;
//     private DelaunayTriangulation _triangulator;
//     private MinSpanningTree _mst;

//     private GraphStruct2D _trimmedGraph = new();
//     private List<Vector3> _listOfHallways = new();
//     private List<Edge2DType> _listOfEdges = new();
//     private int _edgeListCount = 0;
//     private bool _inSolveMode = false;

//     private System.Random _rand = new();
//     private float _timeSinceLastStep = 0f;

//     void Start()
//     {
//         // Spawn objects to scene
//         _cuboidGridMap = gameObject.GetComponent<CuboidGridMap>();
//         _cuboidGridMap.InitialiseGridMap();
        
//         // Perform Triangulation
//         var pointBoundaries = _cuboidGridMap.GetBoundariesFromPointList();
//         _triangulator = new(pointBoundaries.Item1, pointBoundaries.Item2);
        
//         foreach(Vector2 p in _cuboidGridMap.GetSpawnedObjectsPosition())
//         {
//             _triangulator.AddPoint(p);
//         }

//         // Filter The edges using an MST
//         _mst = new();
//         _mst.TrimGraph(_triangulator.GetTriangulationStruct());
//         _trimmedGraph = _mst.GetTrimmedGraph();

//         foreach(Edge2DType e in _mst.GetUnusedEdges())
//         {
//             var RandomNum = _rand.NextDouble();

//             if (RandomNum > 0.8)
//             {
//                 _trimmedGraph.AddEdge(e);
//             }
//         }

//         foreach(Edge2DType e in _trimmedGraph.GetEdgeList())
//         {
//             _cuboidGridMap.SolvePath(e);
//         }

//         _cuboidGridMap.PlaceAllHallways();
//         player.transform.position = _cuboidGridMap.GetRandomRoomPosition();
//     }

//     void Update()
//     {

//     }
// }
