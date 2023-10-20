using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Steel_Engine
{
    public class Mesh
    {
        public List<SteelVertex> vertices = new List<SteelVertex>(); // in NDC (normalised device coordinates)
        public List<SteelEdge> edges = new List<SteelEdge>(); // in NDC (normalised device coordinates)
        public List<SteelTriangle> triangles = new List<SteelTriangle>();

        public void AddTriangle(SteelTriangle triangle)
        {
            triangles.Add(triangle);

            RefreshTriangles();

            MergeDuplicates();
        }

        public void AddTriangleQuickly(SteelTriangle triangle)
        {
            triangles.Add(triangle);

            RefreshTriangles();
        }

        public void AddTriangleRapid(SteelTriangle triangle)
        {
            triangles.Add(triangle);

            vertices.Add(triangle.GetVertex(0));
            vertices.Add(triangle.GetVertex(1));
            vertices.Add(triangle.GetVertex(2));

            edges.Add(triangle.edges[0]);
            edges.Add(triangle.edges[1]);
            edges.Add(triangle.edges[2]);
        }

        public void AddTriangleRapid(Vector3 p1, Vector3 p2, Vector3 p3, Mesh meshParent)
        {
            SteelTriangle triangle = new SteelTriangle(p1, p2, p3, meshParent);

            triangles.Add(triangle);

            vertices.Add(triangle.GetVertex(0));
            vertices.Add(triangle.GetVertex(1));
            vertices.Add(triangle.GetVertex(2));

            edges.Add(triangle.edges[0]);
            edges.Add(triangle.edges[1]);
            edges.Add(triangle.edges[2]);
        }

        public void RefreshTriangles()
        {
            List<SteelVertex> newVertices = new List<SteelVertex>();
            List<SteelEdge> newEdges = new List<SteelEdge>();

            foreach (SteelTriangle triangle in triangles)
            {
                newVertices.Add(triangle.GetVertex(0));
                newVertices.Add(triangle.GetVertex(1));
                newVertices.Add(triangle.GetVertex(2));

                newEdges.Add(triangle.edges[0]);
                newEdges.Add(triangle.edges[1]);
                newEdges.Add(triangle.edges[2]);
            }

            SteelVertex[] tempVertArr = new SteelVertex[newVertices.Count];
            SteelEdge[] tempEdgeArr = new SteelEdge[newEdges.Count];

            newVertices.CopyTo(tempVertArr);
            newEdges.CopyTo(tempEdgeArr);

            vertices = tempVertArr.ToList();
            edges = tempEdgeArr.ToList();
        }

        public int[] GetIndices() // unstable (wont guarrantee it works)
        {
            List<int> indices = new List<int>();

            foreach (SteelEdge edge in edges)
            {
                indices.Add(edge.startVertexIndex);
            }

            return indices.ToArray();
        }

        public void MergeDuplicates()
        {
            List<SteelVertex> newVertices = new List<SteelVertex>();
            List<SteelEdge> newEdges = new List<SteelEdge>();

            foreach (SteelEdge edge in edges)
            {
                if (!newVertices.Contains(edge.GetVertexData(0))) // if the new vertex list doesn't contain the current vertex, add it
                {
                    newVertices.Add(edge.GetVertexData(0));
                }// otherwise ignore it and move on

                if (!newVertices.Contains(edge.GetVertexData(1))) // if the new vertex list doesn't contain the current vertex, add it
                {
                    newVertices.Add(edge.GetVertexData(1));
                }// otherwise ignore it and move on
            }

            // all duplicate vertices are out the of the equation,
            // but now the edges must be recreated to keep the shape

            foreach (SteelEdge edge in edges)
            {
                SteelEdge newEdge = new SteelEdge(newVertices.IndexOf(edge.GetVertexData(0)), newVertices.IndexOf(edge.GetVertexData(1)), this);
                newEdges.Add(newEdge);
            }

            vertices.Clear();
            edges.Clear();

            SteelVertex[] tempVertArr = new SteelVertex[newVertices.Count];
            SteelEdge[] tempEdgeArr = new SteelEdge[newEdges.Count];

            newVertices.CopyTo(tempVertArr);
            newEdges.CopyTo(tempEdgeArr);

            vertices = tempVertArr.ToList();
            edges = tempEdgeArr.ToList();
        }

        public void SetColour(Vector3 colour)
        {
            foreach (SteelTriangle i in triangles)
            {
                i.SetColour(colour);
            }

            RefreshTriangles();
            MergeDuplicates();
        }
    }
}