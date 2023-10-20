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
            bool result = false;

            // find all cube vertices

            Vector3[] vertices = new Vector3[8];
            vertices[0] = cube.min;
            vertices[1] = new Vector3(cube.min.X, cube.min.Y, cube.max.Z);// next to min but forwards
            vertices[2] = new Vector3(cube.max.X, cube.min.Y, cube.min.Z);// next to min but sideways
            vertices[3] = new Vector3(cube.max.X, cube.min.Y, cube.max.Z);

            vertices[4] = new Vector3(cube.max.X, cube.max.Y, cube.min.Z);
            vertices[5] = new Vector3(cube.min.X, cube.max.Y, cube.min.Z);// above the min
            vertices[6] = new Vector3(cube.min.X, cube.max.Y, cube.max.Z);

            vertices[7] = cube.max;

            // convert to real position
            
            Vector3[] convertedVertices = new Vector3[8];
            Matrix3 rot = Matrix3.CreateFromQuaternion(cube.gameObject.qRotation);
            int index = 0;
            foreach (Vector3 vertex in vertices)
            {
                Vector3 newVertexPos = vertex * rot;
                newVertexPos += cube.gameObject.position;

                convertedVertices[index] = newVertexPos;
                index++;
            }

            // find parameters

            Vector3 pVector = point - convertedVertices[0]; // min vertex
            Vector3 hVector = convertedVertices[5] - convertedVertices[0]; // height
            Vector3 wVector = convertedVertices[2] - convertedVertices[0]; // width
            Vector3 dVector = convertedVertices[1] - convertedVertices[0]; // depth

            // calculate face plane normals

            Vector3 frontPlaneNormal = Vector3.Cross(hVector, wVector).Normalized();
            Vector3 leftPlaneNormal = Vector3.Cross(hVector, dVector).Normalized();

            // flatten pVector onto planes

            Vector3 flattenedZ = LinePlaneIntersection(convertedVertices[5], convertedVertices[0], convertedVertices[2], point, frontPlaneNormal);// the point 'p' is now on the same plane as the front plane
            Vector3 flattenedX = LinePlaneIntersection(convertedVertices[0], convertedVertices[1], convertedVertices[5], point, leftPlaneNormal);// the point 'p' is now on the same plane as the left plane


            // calculate dot products

            float dot1 = Vector3.Dot(flattenedZ, wVector);
            float dot2 = Vector3.Dot(flattenedZ, hVector);
            float dot3 = Vector3.Dot(flattenedX, dVector);

            // make dot comparisons

            if (0 <= dot1 && dot1 <= wVector.Length) // REPLACE LATER IF TOO LAGGY
            {
                if (0 <= dot2 && dot2 <= hVector.Length)
                {
                    if (0 <= dot3 && dot3 <= dVector.Length)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }
}