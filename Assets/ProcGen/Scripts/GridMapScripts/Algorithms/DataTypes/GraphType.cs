using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graph2D
{
    public class Node
    {
        public Vector2 Position { get; set; }

        public Node(Vector2 position)
        {
            Position = position;
        }

        public bool IsEqual(Node other)
        {
            return Position == other.Position;
        }
    }

    public class Edge
    {
        private bool _IsEdgeSet = false;
        private float _EdgeWeight = float.MaxValue;
        private (Node, Node) _Nodes = new();

        public Edge(Vector2 a, Vector2 b)
        {
            Node nodeA = new(a);
            Node nodeB = new(b);
            RegisterNodes(nodeA, nodeB);
        }

        public Edge(Node a, Node b)
        {
            RegisterNodes(a, b);
        }

        private bool RegisterNodes(Node a, Node b)
        {
            if (a.IsEqual(b)) return false;
            _Nodes = (a, b);
            _IsEdgeSet = true;
            _EdgeWeight = Vector2.Distance(a.Position, b.Position);
            return true;
        }

        public (Node, Node) GetNodes()
        {
            return _Nodes;
        }

        public bool IsEqual(Edge other)
        {
            var otherNodes = other.GetNodes();
            return ((otherNodes.Item1.IsEqual(_Nodes.Item1) && otherNodes.Item2.IsEqual(_Nodes.Item2)) ||
                    (otherNodes.Item1.IsEqual(_Nodes.Item2) && otherNodes.Item2.IsEqual(_Nodes.Item1)));
        }

        public bool ContainsNode(Node node)
        {
            return (node.IsEqual(_Nodes.Item1) || node.IsEqual(_Nodes.Item2));
        }

        public bool IsValid()
        {
            return _IsEdgeSet;
        }

        public (Vector2, Vector2) GetNodePositions()
        {
            return (_Nodes.Item1.Position, _Nodes.Item2.Position);
        }

        public string ConvertToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("A: [" + _Nodes.Item1.Position.x.ToString() + ", ");
            sb.Append(_Nodes.Item1.Position.y.ToString() + "] ");
            sb.Append("B: [" +_Nodes.Item2.Position.x.ToString() + ", ");
            sb.Append(_Nodes.Item2.Position.y.ToString() + "]");

            return sb.ToString();
        }

        public float GetEdgeWeight()
        {
            return _EdgeWeight;
        }
    }

    public class Graph
    {
        private List<Node> _NodeList;
        private List<Edge> _EdgeList;

        public Graph()
        {
            _NodeList = new();
            _EdgeList = new();
        }

        public bool ContainsNode(Node node)
        {
            foreach (Node n in _NodeList)
            {
                if (n.IsEqual(node)) return true;
            }

            return false;
        }

        public bool ContainsEdge(Edge edge)
        {
            foreach (Edge e in _EdgeList)
            {
                if (e.IsEqual(edge)) return true;
            }

            return false;
        }

        public bool AddNode(Node node)
        {
            if (ContainsNode(node)) return false;
            _NodeList.Add(node);
            return true;
        }

        public bool AddEdge(Edge edge)
        {
            if (ContainsEdge(edge)) return false;
            _EdgeList.Add(edge);

            var EdgeNodes = edge.GetNodes();
            AddNode(EdgeNodes.Item1);
            AddNode(EdgeNodes.Item2);
            return true;
        }

        public bool AddEdge(Vector2 a, Vector2 b)
        {
            Edge edge = new(a, b);
            if (ContainsEdge(edge)) return false;
            _EdgeList.Add(edge);

            var EdgeNodes = edge.GetNodes();
            AddNode(EdgeNodes.Item1);
            AddNode(EdgeNodes.Item2);
            return true;
        }

        public bool AddEdge(Node a, Node b)
        {
            Edge edge = new(a, b);
            if (ContainsEdge(edge)) return false;
            _EdgeList.Add(edge);

            var EdgeNodes = edge.GetNodes();
            AddNode(EdgeNodes.Item1);
            AddNode(EdgeNodes.Item2);
            return true;
        }

        public bool RemoveNode(Node node)
        {
            if (!ContainsNode(node)) return false;
            
            for(int i = _NodeList.Count - 1; i > -1; i--)
            {
                if (_NodeList[i].IsEqual(node))
                {
                    _NodeList.RemoveAt(i);
                }
            }

            for(int i = _EdgeList.Count - 1; i > -1; i--)
            {
                if (_EdgeList[i].ContainsNode(node))
                {
                    _EdgeList.RemoveAt(i);
                }
            }

            return true;
        }

        public bool RemoveEdge(Edge edge)
        {
            if (!ContainsEdge(edge)) return false;
            for(int i = _EdgeList.Count - 1; i > -1; i--)
            {
                if (_EdgeList[i].IsEqual(edge))
                {
                    _EdgeList.RemoveAt(i);
                }
            }

            return true;
        }

        public List<(Vector2, Vector2)> GetEdgeListPositions()
        {
            List<(Vector2, Vector2)> ListOfPositions = new();

            foreach (Edge e in _EdgeList)
            {
                ListOfPositions.Add(e.GetNodePositions());
            }
            
            return ListOfPositions;
        }


        public List<(Vector3, Vector3)> GetEdgeListPositions3D()
        {
            List<(Vector3, Vector3)> ListOfPositions = new();

            foreach (Edge e in _EdgeList)
            {
                var Nodes2d = e.GetNodePositions();
                ListOfPositions.Add((
                    new Vector3(Nodes2d.Item1.x, 0f, Nodes2d.Item1.y),
                    new Vector3(Nodes2d.Item2.x, 0f, Nodes2d.Item2.y)
                ));
            }
            
            return ListOfPositions;
        }


        public List<Node> GetNodeList()
        {
            return _NodeList;
        }

        public List<Edge> GetEdgeList()
        {
            return _EdgeList;
        }

        public Edge GetLowestEdge(Node node)
        {
            Edge edgeLow = null;
            foreach(Edge e in _EdgeList)
            {
                if (!e.ContainsNode(node)) continue;
                edgeLow ??= e;

                if (edgeLow.GetEdgeWeight() > e.GetEdgeWeight())
                {
                    edgeLow = e;
                }
            }

            return edgeLow;
        }

        public List<Node> GetConnectedNodes(Node node)
        {
            List<Node> connectedNodes = new();

            foreach(Edge e in _EdgeList)
            {
                if (e.ContainsNode(node))
                {
                    (Node, Node) nodes = e.GetNodes();
                    Node connectedNode = null;

                    if (nodes.Item1.IsEqual(node))
                    {
                        connectedNode = nodes.Item2;
                    }
                    else
                    {
                        connectedNode = nodes.Item1;
                    }

                    connectedNodes.Add(connectedNode);
                }
            }

            return connectedNodes;
        }
    }
}
