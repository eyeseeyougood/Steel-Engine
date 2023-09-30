using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Steel_Engine.Common
{
    public static class MathFExtentions
    {
        public static Vector3 LinePlaneIntersection(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 lineStart, Vector3 lineDirection)
        {
            // Calculate the normal vector of the plane using the cross product of two vectors on the plane.
            Vector3 planeNormal = Vector3.Cross(point2 - point1, point3 - point1).Normalized();

            // Calculate the distance from the origin to the plane.
            float d = -Vector3.Dot(planeNormal, point1);

            // Calculate the parameter (t) at which the line intersects the plane.
            float t = -(Vector3.Dot(planeNormal, lineStart) + d) / Vector3.Dot(planeNormal, lineDirection);

            // Calculate the intersection point using the parameter (t).
            Vector3 intersectionPoint = lineStart + t * lineDirection;

            return intersectionPoint;
        }
    }
}
