// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using UnityEngine;

// public class MinSpanningTree
// {
//     private GraphStruct2D _initalGraph;
//     private GraphStruct2D _trimmedGraph = new();

//     private System.Random _rand = new();

//     public void TrimGraph(GraphStruct2D graph)
//     {
//         _initalGraph = graph;
//         _trimmedGraph = new();

//         // Pick an arbritrary node
//         var NodeList = _initalGraph.GetNodeList();
//         var initalNodeCount = NodeList.Count;
//         var initialNode = NodeList[_rand.Next(NodeList.Count)];

//         _trimmedGraph.AddNode(initialNode);
//         Edge2DType lowestInitialEdge = _initalGraph.GetLowestEdge(initialNode);
//         _trimmedGraph.AddEdge(lowestInitialEdge);

//         while(_trimmedGraph.GetNodeList().Count != initalNodeCount)
//         {
//             List<Edge2DType> FilteredEdges = new();
//             foreach (Edge2DType e in _initalGraph.GetEdgeList())
//             {
//                 (Node2DType, Node2DType) nodes = e.GetNodes();

//                 if (_trimmedGraph.ContainsNode(nodes.Item1) && 
//                     _trimmedGraph.ContainsNode(nodes.Item2))
//                 {
//                     continue;
//                 }

//                 if (_trimmedGraph.ContainsNode(nodes.Item1) || 
//                     _trimmedGraph.ContainsNode(nodes.Item2))
//                 {
//                     FilteredEdges.Add(e);
//                 }
//             }

//             Edge2DType lowestEdge = null;
//             foreach(Edge2DType e in FilteredEdges)
//             {
//                 lowestEdge ??= e;
//                 if (lowestEdge.GetEdgeWeight() > e.GetEdgeWeight())
//                 {
//                     lowestEdge = e;
//                 }
//             }


//             if (lowestEdge != null)
//             {
//                 _trimmedGraph.AddEdge(lowestEdge);
//             }
//         }
//     }


//     public List<Edge2DType> GetUnusedEdges()
//     {
//         if (_initalGraph.GetEdgeList().Count == 0 || _trimmedGraph.GetEdgeList().Count == 0)
//         {
//             return new();
//         }

//         List<Edge2DType> fullInitialList = _initalGraph.GetEdgeList();
//         List<Edge2DType> unusedEdges = new();

//         foreach(Edge2DType e in fullInitialList)
//         {
//             if (!_trimmedGraph.ContainsEdge(e))
//             {
//                 unusedEdges.Add(e);
//             }
//         }

//         return unusedEdges;
//     }

//     public GraphStruct2D GetTrimmedGraph()
//     {
//         return _trimmedGraph;
//     }
    
// }
