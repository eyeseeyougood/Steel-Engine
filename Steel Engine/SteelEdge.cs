using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class SteelEdge
    {
        public int startVertexIndex { get; private set; }
        public int endVertexIndex { get; private set; }

        private Mesh meshParent;

        private SteelVertex startVertexCopy;
        private SteelVertex endVertexCopy;

        public SteelEdge(SteelVertex startVertex, SteelVertex endVertex, Mesh meshParent)
        {
            this.meshParent = meshParent;
            if (!meshParent.vertices.ContainsAnInstanceEqualTo(startVertex))
                meshParent.vertices.Add(startVertex);
            if (!meshParent.vertices.ContainsAnInstanceEqualTo(endVertex))
                meshParent.vertices.Add(endVertex);
            startVertexIndex = meshParent.vertices.IndexOf(startVertex);
            endVertexIndex = meshParent.vertices.IndexOf(endVertex);
            startVertexCopy = startVertex;
            endVertexCopy = endVertex;
        }

        public SteelEdge(int startVertexIndex, int endVertexIndex, Mesh meshParent)
        {
            this.meshParent = meshParent;
            this.startVertexIndex = startVertexIndex;
            this.endVertexIndex = endVertexIndex;
        }

        public SteelVertex GetVertexData(int index)
        {
            if (index == 0)
            {
                return startVertexCopy;
            }
            return endVertexCopy;
        }
    }
}