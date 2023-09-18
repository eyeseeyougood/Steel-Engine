using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class SteelTriangle
    {
        public SteelEdge[] edges;

        public SteelTriangle(Vector3 pos1, Vector3 pos2, Vector3 pos3, Mesh meshParent)
        {
            edges = new SteelEdge[3];
            edges[0] = new SteelEdge(new SteelVertex(pos1), new SteelVertex(pos2), meshParent);
            edges[1] = new SteelEdge(new SteelVertex(pos2), new SteelVertex(pos3), meshParent);
            edges[2] = new SteelEdge(new SteelVertex(pos3), new SteelVertex(pos1), meshParent);
        }

        public SteelTriangle(SteelVertex vert1, SteelVertex vert2, SteelVertex vert3, Mesh meshParent)
        {
            edges = new SteelEdge[3];
            edges[0] = new SteelEdge(vert1, vert2, meshParent);
            edges[1] = new SteelEdge(vert2, vert3, meshParent);
            edges[2] = new SteelEdge(vert3, vert1, meshParent);
        }

        public SteelTriangle(SteelEdge edge1, SteelEdge edge2, SteelEdge edge3, Mesh meshParent)
        {
            edges = new SteelEdge[3];
            edges[0] = edge1;
            edges[1] = edge2;
            edges[2] = edge3;
        }

        public void SetColour(Vector3 colour)
        {
            edges[0].GetVertexData(0).colour = colour;
            edges[1].GetVertexData(0).colour = colour;
            edges[2].GetVertexData(0).colour = colour;
        }

        public void SetVertexColour(int index, Vector3 colour)
        {
            edges[index].GetVertexData(0).colour = colour;
        }

        public SteelVertex GetVertex(int index)
        {
            return edges[index].GetVertexData(0);
        }
    }
}
