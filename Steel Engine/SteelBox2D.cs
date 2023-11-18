using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class SteelShape2D
    {
        public Vector2[] vertices;

        public SteelShape2D(Vector2[] vertices)
        {
            this.vertices = vertices;
        }
    }
}
