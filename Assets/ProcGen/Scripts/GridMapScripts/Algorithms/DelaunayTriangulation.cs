using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation
{
    public class Triangle2D
    {
        private Graph2D.Graph _triangleGraph;
        private Vector2 _circumcenter;
        private float _circumradius;

        public Triangle2D(Vector2 a, Vector2 b, Vector2 c)
        {
            _triangleGraph = new();

            _triangleGraph.AddEdge(a, b);
            _triangleGraph.AddEdge(b, c);
            _triangleGraph.AddEdge(c, a);

            CalculateCircumcircle();
        }

        public Triangle2D(Graph2D.Node a, Graph2D.Node b, Graph2D.Node c)
        {
            _triangleGraph = new();

            _triangleGraph.AddEdge(a, b);
            _triangleGraph.AddEdge(b, c);
            _triangleGraph.AddEdge(c, a);

            CalculateCircumcircle();
        }

        public List<(Vector2, Vector2)> GetGraphEdges()
        {
            return _triangleGraph.GetEdgeListPositions();
        }

        private void CalculateCircumcircle()
        {
            List<Graph2D.Node> NodeList = _triangleGraph.GetNodeList();
            Vector2 PointA = NodeList[0].Position;
            Vector2 PointB = NodeList[1].Position;
            Vector2 PointC = NodeList[2].Position;

            // Find mind point of the line
            var MidpointAB = (PointA + PointB) / 2f;
            var MidpointBC = (PointB + PointC) / 2f;
            var MidpointCA = (PointC + PointA) / 2f;

            // slope of ab
            float SlopePernAB = -1f / ((PointB.y - PointA.y) / (PointB.x - PointA.x));
            float SlopePernBC = -1f / ((PointC.y - PointB.y) / (PointC.x - PointB.x)); 
            float SlopePernCA = -1f / ((PointA.y - PointC.y) / (PointA.x - PointC.x)); 

            float Con_AB = (-1f * SlopePernAB * MidpointAB.x) + (MidpointAB.y);
            float Con_BC = (-1f * SlopePernBC * MidpointBC.x) + (MidpointBC.y);
            float Con_CA = (-1f * SlopePernCA * MidpointCA.x) + (MidpointCA.y);

            _circumcenter = new();
            _circumradius = 0.0f;
            if (float.IsInfinity(SlopePernAB))
            {
                float BCCA_x = (Con_BC - Con_CA) / (SlopePernCA - SlopePernBC);
                _circumcenter.x = BCCA_x;
                _circumcenter.y = (SlopePernBC * BCCA_x) + Con_BC;
                _circumradius = Vector2.Distance(_circumcenter, PointA);
            }
            else if(float.IsInfinity(SlopePernBC))
            {  
                float CAAB_x = (Con_CA - Con_AB) / (SlopePernAB - SlopePernCA);
                _circumcenter.x = CAAB_x;
                _circumcenter.y = (SlopePernCA * CAAB_x) + Con_CA;
                _circumradius = Vector2.Distance(_circumcenter, PointB);
            }
            else
            {
                float ABBC_x = (Con_AB - Con_BC) / (SlopePernBC - SlopePernAB);
                _circumcenter.x = ABBC_x;
                _circumcenter.y = (SlopePernAB * ABBC_x) + Con_AB;
                _circumradius = Vector2.Distance(_circumcenter, PointC);
            }
        }

        public bool ContainsNode(Graph2D.Node node)
        {
            return _triangleGraph.ContainsNode(node);
        }

        public bool ContainsEdge(Graph2D.Edge edge)
        {
            return _triangleGraph.ContainsEdge(edge);
        }

        public float GetCircumRadius()
        {
            return _circumradius;
        }

        public Vector2 GetCircumCenter()
        {
            return _circumcenter;
        }

        public List<Graph2D.Edge> GetTriangleEdges()
        {
            return _triangleGraph.GetEdgeList();
        }

        public List<Graph2D.Node> GetTriangleNodes()
        {
            return _triangleGraph.GetNodeList();
        }

        public bool IsEqual(Triangle2D triangle)
        {
            foreach(Graph2D.Edge e in triangle.GetTriangleEdges())
            {
                if (!ContainsEdge(e))
                {
                    return false;
                }
            }

            return true;
        }
    }


    private List<Triangle2D> _triangleList = new();
    private Triangle2D _superTriangle;
    
    public DelaunayTriangulation(Vector2 minPoint, Vector2 maxPoint)
    {
        float RoomWidth = maxPoint.x - minPoint.x;
        float RoomHeight = maxPoint.y - minPoint.y;
        _superTriangle = new
        (
            new Vector2(minPoint.x - RoomWidth, minPoint.y - 2f),
            new Vector2(minPoint.x + (RoomWidth * 2f), minPoint.y - 2f),
            new Vector2(minPoint.x + (RoomWidth / 2f), minPoint.y + (RoomHeight * 2f))
        );

        _triangleList.Add(_superTriangle);
    }

    public void AddPoint(Vector2 point)
    {
        List<Triangle2D> badTriangles = new();

        foreach(Triangle2D t in _triangleList)
        {
            var circumRadius = t.GetCircumRadius();
            var circumCenter = t.GetCircumCenter();
            var distFromCircumcenter = Vector2.Distance(circumCenter, point);

            if (distFromCircumcenter < circumRadius)
            {
                badTriangles.Add(t);
            }
        }

        List<Graph2D.Edge> polygon = new();
        for (int bt = 0; bt < badTriangles.Count; bt++)
        {
            foreach (Graph2D.Edge e in badTriangles[bt].GetTriangleEdges())
            {
                bool isEdgeUnqiue = true;

                for (int obt = 0; obt < badTriangles.Count; obt++)
                {
                    if (obt != bt && badTriangles[obt].ContainsEdge(e))
                    {
                        isEdgeUnqiue = false;
                        break;
                    }
                }
                
                if (isEdgeUnqiue) polygon.Add(e);
            }
        }

        foreach(Triangle2D t in badTriangles)
        {
            for(int i = _triangleList.Count - 1; i > -1; i--)
            {
                if (t.IsEqual(_triangleList[i])) _triangleList.RemoveAt(i);
            }
        }

        foreach(Graph2D.Edge e in polygon)
        {
            var polyNodes = e.GetNodes();
            Graph2D.Node pointNode = new(point);  
            Triangle2D newTri = new(polyNodes.Item1, polyNodes.Item2, pointNode);

            _triangleList.Add(newTri);
        }
    }

    public Graph2D.Graph GetTriangulationStruct(bool addSuperTriangle = false)
    {
        Graph2D.Graph TriangulationGraph = new();

        foreach(Triangle2D t in _triangleList)
        {
            bool ContainsSuperTriangleNode = false;
            foreach(Graph2D.Node n in t.GetTriangleNodes())
            {
                if (_superTriangle.ContainsNode(n))
                {
                    ContainsSuperTriangleNode = true;
                    break;
                }
            }

            if (!ContainsSuperTriangleNode || addSuperTriangle)
            {
                foreach(Graph2D.Edge e in t.GetTriangleEdges())
                {
                    TriangulationGraph.AddEdge(e);
                }
            };
        }

        return TriangulationGraph;
    }
}
