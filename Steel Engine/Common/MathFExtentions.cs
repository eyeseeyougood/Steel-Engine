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

        public static bool SeperateAxes(BoxCollider cube1, BoxCollider cube2, Vector3 axis)
        {
            Vector3 halfSize1 = cube1.gameObject.scale / 2;
            Vector3 halfSize2 = cube2.gameObject.scale / 2;

            float projection1 = Vector3.Dot(cube1.gameObject.position, axis);
            float projection2 = Vector3.Dot(cube2.gameObject.position, axis);

            float distance = Math.Abs(projection1 - projection2);
            float minOverlap = (halfSize1.X + halfSize2.X) * Math.Abs(axis.X) +
                               (halfSize1.Y + halfSize2.Y) * Math.Abs(axis.Y) +
                               (halfSize1.Z + halfSize2.Z) * Math.Abs(axis.Z);

            return distance > minOverlap;
        }

        public static bool PointCubeIntersection(Vector3 point, BoxCollider cube)
        {
            // Translate the point and cube center to local space
            Vector3 localPoint = point - cube.gameObject.position;

            // Rotate the local point using the inverse of the cube's orientation
            Quaternion inverseOrientation = cube.gameObject.qRotation.Inverted();
            Vector3 localPointRotated = Vector3.Transform(localPoint, inverseOrientation);

            // Check if the rotated point is within the range of the cube's local space
            return (
                MathF.Abs(localPointRotated.X) <= cube.gameObject.scale.X &&
                MathF.Abs(localPointRotated.Y) <= cube.gameObject.scale.Y &&
                MathF.Abs(localPointRotated.Z) <= cube.gameObject.scale.Z
            );
        }
    }
}