using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinimumSpanningTree
{
    private Graph2D.Graph _initalGraph;
    private Graph2D.Graph _trimmedGraph = new();
    private System.Random _rand = new();

    public void TrimGraph(Graph2D.Graph graph)
    {
        _initalGraph = graph;
        _trimmedGraph = new();

        // Pick an arbritrary node
        var NodeList = _initalGraph.GetNodeList();
        var initalNodeCount = NodeList.Count;
        var initialNode = NodeList[_rand.Next(NodeList.Count)];

        _trimmedGraph.AddNode(initialNode);
        Graph2D.Edge lowestInitialEdge = _initalGraph.GetLowestEdge(initialNode);
        _trimmedGraph.AddEdge(lowestInitialEdge);

        while(_trimmedGraph.GetNodeList().Count != initalNodeCount)
        {
            List<Graph2D.Edge> FilteredEdges = new();
            foreach (Graph2D.Edge e in _initalGraph.GetEdgeList())
            {
                (Graph2D.Node, Graph2D.Node) nodes = e.GetNodes();

                if (_trimmedGraph.ContainsNode(nodes.Item1) && 
                    _trimmedGraph.ContainsNode(nodes.Item2))
                {
                    continue;
                }

                if (_trimmedGraph.ContainsNode(nodes.Item1) || 
                    _trimmedGraph.ContainsNode(nodes.Item2))
                {
                    FilteredEdges.Add(e);
                }
            }

            Graph2D.Edge lowestEdge = null;
            foreach(Graph2D.Edge e in FilteredEdges)
            {
                lowestEdge ??= e;
                if (lowestEdge.GetEdgeWeight() > e.GetEdgeWeight())
                {
                    lowestEdge = e;
                }
            }


            if (lowestEdge != null)
            {
                _trimmedGraph.AddEdge(lowestEdge);
            }
        }
    }


    public List<Graph2D.Edge> GetUnusedEdges()
    {
        if (_initalGraph.GetEdgeList().Count == 0 || _trimmedGraph.GetEdgeList().Count == 0)
        {
            return new();
        }

        List<Graph2D.Edge> fullInitialList = _initalGraph.GetEdgeList();
        List<Graph2D.Edge> unusedEdges = new();

        foreach(Graph2D.Edge e in fullInitialList)
        {
            if (!_trimmedGraph.ContainsEdge(e))
            {
                unusedEdges.Add(e);
            }
        }

        return unusedEdges;
    }

    public Graph2D.Graph GetTrimmedGraph()
    {
        return _trimmedGraph;
    }

    public List<(Vector2, Vector2)> RandomPickUnusedEdgePositions(float percentage)
    {
        List<Graph2D.Edge> unusedEdges = GetUnusedEdges();
        List<Graph2D.Edge> listOfChosenEdges = new();

        System.Random rand = new();
        int numOfInts = (int)Mathf.Ceil((unusedEdges.Count - 1) * percentage);

        var ints = Enumerable.Range(0, numOfInts)
            .Select(i => new Tuple<int, int>(rand.Next(numOfInts), i))
            .OrderBy(i => i.Item1)
            .Select(i => i.Item2);
        
        foreach(int index in ints)
        {
            listOfChosenEdges.Add(unusedEdges[index]);
        }

        List<(Vector2, Vector2)> chosenEdgePositions = new();
        foreach(var e in listOfChosenEdges)
        {
            chosenEdgePositions.Add(e.GetNodePositions());
        }

        return chosenEdgePositions;
    }
}
