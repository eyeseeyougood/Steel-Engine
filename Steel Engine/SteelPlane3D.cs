using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class SteelPlane3D
    {
        public Vector3[] points;

        public SteelPlane3D(Vector3[] points)
        {
            this.points = points;
        }

        public SteelPlane3D(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3[] points = new Vector3[3]{p0, p1, p2};
            this.points = points;
        }

        public override bool Equals(object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                SteelPlane3D p = (SteelPlane3D)obj;
                return (points[0] == p.points[0]) && (points[1] == p.points[1]) && (points[2] == p.points[2]);
            }
        }
    }
}
