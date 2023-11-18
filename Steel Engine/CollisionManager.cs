using OpenTK.Mathematics;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    [SteelComponent]
    public class Collider : Component
    {
        protected override void Init()
        {
            CollisionManager.colliders.Add(this);
        }

        public override void Tick(float deltaTime)
        {

        }

        public virtual Vector3 CalculateCollisionNormal(Collider other)
        {
            return Vector3.Zero;
        }

        public virtual bool CheckSATCollision(Collider other)
        {
            bool result = false;
            return result;
        }

        public virtual bool CheckCollision(Collider other)
        {
            bool result = false;
            return result;
        }

        public virtual bool CheckCollision(Collider other, Vector3 targetPos)
        {
            bool result = false;
            return result;
        }
    }


    public class BoxCollider : Collider
    {
        public Vector3 min;
        public Vector3 max;

        public Vector3[] vertices;

        protected override void Init()
        {
            base.Init();
            min = -Vector3.One;
            max = Vector3.One;
            vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(-1, -1, 1);
            vertices[2] = new Vector3(1, -1, 1);
            vertices[3] = new Vector3(1, -1, -1);
            vertices[4] = new Vector3(1, 1, -1);
            vertices[5] = new Vector3(-1, 1, 1);
            vertices[6] = new Vector3(-1, 1, -1);
            vertices[7] = max;
            // transform vertices by position, scale, rotation
            List<Vector3> finalVertices = new List<Vector3>();
            foreach (Vector3 vert in vertices)
            {
                Vector3 transformed = vert * gameObject.scale;
                transformed = Vector3.Transform(transformed, gameObject.qRotation);
                transformed = transformed + gameObject.position;
                finalVertices.Add(transformed);
            }
            vertices = finalVertices.ToArray();
        }

        public override void Tick(float deltaTime)
        {
            min = -Vector3.One;
            max = Vector3.One;
            vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(-1, -1, 1);
            vertices[2] = new Vector3(1, -1, 1);
            vertices[3] = new Vector3(1, -1, -1);
            vertices[4] = new Vector3(1, 1, -1);
            vertices[5] = new Vector3(-1, 1, 1);
            vertices[6] = new Vector3(-1, 1, -1);
            vertices[7] = max;
            // transform vertices by position, scale, rotation
            List<Vector3> finalVertices = new List<Vector3>();
            foreach (Vector3 vert in vertices)
            {
                Vector3 transformed = vert * gameObject.scale;
                transformed = Vector3.Transform(transformed, gameObject.qRotation);
                transformed = transformed + gameObject.position;
                finalVertices.Add(transformed);
            }
            vertices = finalVertices.ToArray();
        }


        public override bool CheckCollision(Collider other)
        {
            bool result = false;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = CheckBox((BoxCollider)other);
                    break;
            }

            return result;
        }

        public override bool CheckSATCollision(Collider other)
        {
            bool result = false;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = SAT3D(this, (BoxCollider)other, out Vector3 vec);
                    Console.WriteLine(result);
                    break;
            }

            return result;
        }

        public bool SAT3D(BoxCollider box1, BoxCollider box2, out Vector3 mtv) // mtv = minimum translation vector
        {
            bool result = true;

            SteelPlane3D[] planes = FindPlanes3D(box1, box2);

            Vector3 totalTV = new Vector3(0, 0, 0);

            foreach (SteelPlane3D plane in planes)
            {
                // get plane normal
                Vector3 planeNormal = Vector3.Cross(plane.points[1] - plane.points[0], plane.points[2] - plane.points[0]);
                planeNormal.Normalize();

                // project box1 onto the plane
                Vector3[] projectedVertices = new Vector3[8];
                int index = 0;
                foreach (Vector3 vertex in box1.vertices)
                {
                    float dist = Vector3.Dot(vertex, planeNormal);
                    Vector3 diff = dist * planeNormal;
                    projectedVertices[index] = vertex - diff;
                    index++;
                }

                // project box2 onto the plane
                Vector3[] projectedVertices1 = new Vector3[8];
                index = 0;
                foreach (Vector3 vertex in box2.vertices)
                {
                    float dist = Vector3.Dot(vertex, planeNormal);
                    Vector3 diff = dist * planeNormal;
                    projectedVertices1[index] = vertex - diff;
                    index++;
                }

                // Find orthonormal bases
                Vector3 a = (plane.points[1] - plane.points[0]).Normalized();
                Vector3 b = Vector3.Cross(a, planeNormal).Normalized();
                // Find rotation matrix
                Matrix3 rotationMatrix = new Matrix3(a, b, planeNormal).Inverted();
                // Convert Vertices to 2D space
                string debug1 = "";
                string debug2 = "";
                List<Vector2> convertedVertices = new List<Vector2>();
                foreach (Vector3 vertex in projectedVertices)
                {
                    Vector3 rotated = vertex * rotationMatrix;
                    Vector2 converted = new Vector2(rotated.X, rotated.Y);
                    convertedVertices.Add(converted);
                    debug1 += $",({converted.X}, {converted.Y})";
                }

                List<Vector2> convertedVertices1 = new List<Vector2>();
                foreach (Vector3 vertex in projectedVertices1)
                {
                    Vector3 rotated = vertex * rotationMatrix;
                    Vector2 converted = new Vector2(rotated.X, rotated.Y);
                    convertedVertices1.Add(converted);
                    debug2 += $",({converted.X}, {converted.Y})";
                }

                // Create shapes
                SteelShape2D shape1 = new SteelShape2D(convertedVertices.ToArray());
                SteelShape2D shape2 = new SteelShape2D(convertedVertices1.ToArray());

                // perform 2D SAT on projected shapes
                if (!SAT2D(shape1, shape2, out float bum))
                {
                    // there is a plane on which the objects don't overlap
                    result = false;
                    break;
                }
            }

            if (result)
            {
                mtv = Vector3.Zero;
            }

            mtv = Vector3.Zero;
            return result;
        }

        public bool SAT2D(SteelShape2D shape1, SteelShape2D shape2, out float overlapDist)// overlapDist will be -1 if there isn't an overlap
        {
            bool result = true;

            Vector2[] axes = FindAxes2D(shape1, shape2);

            Vector2 collisionNormal = Vector2.Zero;

            foreach (Vector2 axis in axes)
            {
                // project box1 onto axis
                float minDist = float.MaxValue;
                float maxDist = float.MinValue;
                foreach (Vector2 vertex in shape1.vertices)
                {
                    float dist = Vector2.Dot(vertex, axis.Normalized());
                    if (dist < minDist) { minDist = dist; }
                    if (dist > maxDist) { maxDist = dist; }
                }

                // project box2 onto axis
                float minDist1 = float.MaxValue;
                float maxDist1 = float.MinValue;
                foreach (Vector2 vertex in shape2.vertices)
                {
                    float dist1 = Vector2.Dot(vertex, axis.Normalized());
                    if (dist1 < minDist1) { minDist1 = dist1; }
                    if (dist1 > maxDist1) { maxDist1 = dist1; }
                }

                // check for overlap
                if (!(minDist1 <= maxDist && minDist1 >= minDist || maxDist1 <= maxDist && maxDist1 >= minDist) && !(minDist <= maxDist1 && minDist >= minDist1 || maxDist <= maxDist1 && maxDist >= minDist1))
                {
                    // there isn't an overlap
                    result = false;
                    break;
                }
                else
                {
                    // there is an overlap on this axis
                    //if ()
                }
            }

            if (result)
            {
                // there is an overlap
                overlapDist = -1f;
            }
            else
            {
                overlapDist = -1f;
            }

            return result;
        }

        public SteelPlane3D[] FindPlanes3D(BoxCollider box1, BoxCollider box2)
        {
            List<SteelPlane3D> planes = new List<SteelPlane3D>();
            SteelPlane3D up = new SteelPlane3D(box1.vertices[5], box1.vertices[7], box1.vertices[6]);
            SteelPlane3D right = new SteelPlane3D(box1.vertices[2], box1.vertices[7], box1.vertices[3]);
            SteelPlane3D forward = new SteelPlane3D(box1.vertices[1], box1.vertices[5], box1.vertices[2]);
            SteelPlane3D up1 = new SteelPlane3D(box2.vertices[5], box2.vertices[7], box2.vertices[6]);
            SteelPlane3D right1 = new SteelPlane3D(box2.vertices[2], box2.vertices[7], box2.vertices[3]);
            SteelPlane3D forward1 = new SteelPlane3D(box2.vertices[1], box2.vertices[5], box2.vertices[2]);
            planes.Add(up);
            planes.Add(up1);
            planes.Add(right);
            planes.Add(right1);
            planes.Add(forward);
            planes.Add(forward1);

            // cleanup duplicate planes
            List<SteelPlane3D> finalPlanes = new List<SteelPlane3D>();
            foreach (SteelPlane3D plane in planes)
            {
                if (!finalPlanes.ContainsAnInstanceEqualTo(plane))
                {
                    finalPlanes.Add(plane);
                }
            }

            return planes.ToArray();
        }

        public Vector2[] FindAxes2D(SteelShape2D shape1, SteelShape2D shape2)
        {
            List<Vector2> axes = new List<Vector2>();
            Vector2 prevVert = shape1.vertices[0];
            foreach (Vector2 vert in shape1.vertices)
            {
                if (vert != prevVert)
                {
                    axes.Add(vert - prevVert);
                    prevVert = vert;
                }
            }
            Vector2 prevVert1 = shape2.vertices[0];
            foreach (Vector2 vert in shape2.vertices)
            {
                if (vert != prevVert)
                {
                    axes.Add(vert - prevVert);
                    prevVert1 = vert;
                }
            }

            // cleanup duplicate axes
            List<Vector2> finalAxes = new List<Vector2>();
            foreach (Vector2 axis in axes)
            {
                if (!finalAxes.ContainsAnInstanceEqualTo(axis))
                {
                    finalAxes.Add(axis);
                }
            }

            return finalAxes.ToArray();
        }

        public override Vector3 CalculateCollisionNormal(Collider other)
        {
            Vector3 result = Vector3.Zero;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = CalculateBoxCollisionNormal((BoxCollider)other);
                    break;
            }

            return result;
        }

        private Vector3 CalculateBoxCollisionNormal(BoxCollider other)
        {
            Vector3 result = Vector3.Zero;

            float xDist = MathF.Abs(gameObject.position.X - other.gameObject.position.X);
            float xHeight = (max.X - min.X) / 2 + (other.max.X - other.min.X) / 2;
            float xMoveDist = 0;
            if (xDist < xHeight)
            {
                xMoveDist = xHeight - xDist;
            }

            float yDist = MathF.Abs(gameObject.position.Y - other.gameObject.position.Y);
            float yHeight = (max.Y - min.Y)/2 + (other.max.Y - other.min.Y)/2;
            float yMoveDist = 0;
            if (yDist < yHeight)
            {
                yMoveDist = yHeight - yDist;
            }

            float zDist = MathF.Abs(gameObject.position.Z - other.gameObject.position.Z);
            float zHeight = (max.Y - min.Y) / 2 + (other.max.Y - other.min.Y) / 2;
            float zMoveDist = 0;
            if (zDist < zHeight)
            {
                zMoveDist = zHeight - zDist;
            }

            if (xMoveDist < yMoveDist && xMoveDist < zMoveDist)
            {
                result = new Vector3(xMoveDist, 0, 0);
            }
            else if (yMoveDist < xMoveDist && yMoveDist < zMoveDist)
            {
                result = new Vector3(0, yMoveDist, 0);
            }
            else if (zMoveDist < xMoveDist && zMoveDist < yMoveDist)
            {
                result = new Vector3(0, 0, zMoveDist);
            }

            return result;
        }

        public override bool CheckCollision(Collider other, Vector3 targetPos)
        {
            bool result = false;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = CheckBox((BoxCollider)other, targetPos);
                    break;
            }

            return result;
        }

        private bool CheckBox(BoxCollider other)
        {
            bool result = false;

            // as this is axis-aligned bounding boxes (AABB) it won't work with rotations (because it's axis aligned)
            if (min.X >= other.min.X && min.X <= other.max.X || max.X >= other.min.X && max.X <= other.max.X)
            {
                if (min.Y >= other.min.Y && min.Y <= other.max.Y || max.Y >= other.min.Y && max.Y <= other.max.Y)
                {
                    if (min.Z >= other.min.Z && min.Z <= other.max.Z || max.Z >= other.min.Z && max.Z <= other.max.Z)
                    {
                        // overlapping
                        result = true;
                    }
                }
            }

            return result;
        }

        private bool CheckBox(BoxCollider other, Vector3 targetPos) // AABB
        {
            bool result = false;

            Vector3 _min = -Vector3.One * gameObject.scale + targetPos;
            Vector3 _max = Vector3.One * gameObject.scale + targetPos;

            // as this is axis-aligned bounding boxes (AABB) it won't work with rotations (because it's axis aligned)
            if (_min.X >= other.min.X && _min.X <= other.max.X || _max.X >= other.min.X && _max.X <= other.max.X)
            {
                if (_min.Y >= other.min.Y && _min.Y <= other.max.Y || _max.Y >= other.min.Y && _max.Y <= other.max.Y)
                {
                    if (_min.Z >= other.min.Z && _min.Z <= other.max.Z || _max.Z >= other.min.Z && _max.Z <= other.max.Z)
                    {
                        // overlapping
                        result = true;
                    }
                }
            }

            return result;
        }

        private bool CheckMesh(MeshCollider other)
        {
            bool result = false;

            SteelVertex[] vertices = new SteelVertex[gameObject.mesh.vertices.Count];
            gameObject.mesh.vertices.CopyTo(vertices);

            foreach (SteelVertex currentVertex in vertices)
            {
                float[] vertData = currentVertex.GetVertexData();
                float[] vertPosition = new float[3]; // NDC (Normalised device coordinates)
                vertPosition[0] = vertData[0];
                vertPosition[1] = vertData[1];
                vertPosition[2] = vertData[2];
                Vector3 vertexPosition = new Vector3(vertPosition[0], vertPosition[1], vertPosition[2]);
                // convert to world space
                Vector4 point = new Vector4(vertexPosition, 1) * gameObject.GetModelMatrix();
                vertexPosition = point.Xyz/point.W;
                // cast ray
                SteelRay raycast = new SteelRay(vertexPosition, Vector3.UnitX);
                foreach (SteelTriangle triangle in other.gameObject.mesh.triangles)
                {
                    Vector3 vert1 = triangle.GetVertex(0).position;
                    Vector3 vert2 = triangle.GetVertex(1).position;
                    Vector3 vert3 = triangle.GetVertex(2).position;
                    Vector3 intersection = MathFExtentions.LinePlaneIntersection(vert1, vert2, vert3, raycast.worldPosition, raycast.worldDirection);

                    // Move the triangle so that the point becomes the 
                    // triangles origin
                    vert1 -= intersection;
                    vert2 -= intersection;
                    vert3 -= intersection;

                    // The point should be moved too, so they are both
                    // relative, but because we don't use p in the
                    // equation anymore, we don't need it!
                    // p -= p;

                    // Compute the normal vectors for triangles:
                    // u = normal of PBC
                    // v = normal of PCA
                    // w = normal of PAB

                    Vector3 u = Vector3.Cross(vert2, vert3);
                    Vector3 v = Vector3.Cross(vert3, vert1);
                    Vector3 w = Vector3.Cross(vert1, vert2);

                    // Test to see if the normals are facing 
                    // the same direction, return false if not
                    if (Vector3.Dot(u, v) < 0.0f && Vector3.Dot(u, w) < 0.0f)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    // NOT STABLE MESH COLLIDER
    public class MeshCollider : Collider
    {
        public override bool CheckCollision(Collider other)
        {
            bool result = false;

            switch (other.GetType().Name)
            {
                case "BoxCollider":
                    result = CheckMesh((MeshCollider)other);
                    break;
            }

            return result;
        }

        private bool CheckMesh(MeshCollider other) // could be modified to use a MUCH faster check as it is a box
        {
            bool result = false;

            SteelVertex[] vertices = new SteelVertex[gameObject.mesh.vertices.Count];
            gameObject.mesh.vertices.CopyTo(vertices);

            foreach (SteelVertex currentVertex in vertices)
            {
                float[] vertData = currentVertex.GetVertexData();
                float[] vertPosition = new float[3]; // NDC (Normalised device coordinates)
                vertPosition[0] = vertData[0];
                vertPosition[1] = vertData[1];
                vertPosition[2] = vertData[2];
                Vector3 vertexPosition = new Vector3(vertPosition[0], vertPosition[1], vertPosition[2]);
                // convert to world space
                Vector4 point = new Vector4(vertexPosition, 1) * gameObject.GetModelMatrix();
                vertexPosition = point.Xyz / point.W;
                // cast ray
                SteelRay raycast = new SteelRay(vertexPosition, Vector3.UnitX);
                foreach (SteelTriangle triangle in other.gameObject.mesh.triangles)
                {
                    Vector3 vert1 = triangle.GetVertex(0).position;
                    Vector3 vert2 = triangle.GetVertex(1).position;
                    Vector3 vert3 = triangle.GetVertex(2).position;
                    Vector3 intersection = MathFExtentions.LinePlaneIntersection(vert1, vert2, vert3, raycast.worldPosition, raycast.worldDirection);

                    // Move the triangle so that the point becomes the 
                    // triangles origin
                    vert1 -= intersection;
                    vert2 -= intersection;
                    vert3 -= intersection;

                    // The point should be moved too, so they are both
                    // relative, but because we don't use p in the
                    // equation anymore, we don't need it!
                    // p -= p;

                    // Compute the normal vectors for triangles:
                    // u = normal of PBC
                    // v = normal of PCA
                    // w = normal of PAB

                    Vector3 u = Vector3.Cross(vert2, vert3);
                    Vector3 v = Vector3.Cross(vert3, vert1);
                    Vector3 w = Vector3.Cross(vert1, vert2);

                    // Test to see if the normals are facing 
                    // the same direction, return false if not
                    if (Vector3.Dot(u, v) < 0.0f && Vector3.Dot(u, w) < 0.0f)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    public static class CollisionManager
    {
        public static List<Collider> colliders = new List<Collider>();
    }
}
