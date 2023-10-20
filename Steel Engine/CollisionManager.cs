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
    public class Collider : Component
    {
        protected override void Init()
        {
            CollisionManager.colliders.Add(this);
        }

        public override void Tick(float deltaTime)
        {

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

        protected override void Init()
        {
            base.Init();
            min = -Vector3.One * gameObject.scale;
            max = Vector3.One * gameObject.scale;
        }

        public override void Tick(float deltaTime)
        {
            min = -Vector3.One * gameObject.scale;
            max = Vector3.One * gameObject.scale;
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

            // find all own vertices

            Vector3[] vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(min.X, min.Y, max.Z);
            vertices[2] = new Vector3(max.X, min.Y, min.Z);
            vertices[3] = new Vector3(max.X, min.Y, max.Z);

            vertices[4] = new Vector3(max.X, max.Y, min.Z);
            vertices[5] = new Vector3(min.X, max.Y, min.Z);
            vertices[6] = new Vector3(min.X, max.Y, max.Z);

            vertices[7] = max;

            // iterate

            Matrix3 rot = Matrix3.CreateFromQuaternion(gameObject.qRotation);
            int index = 0; // TEST CODE
            foreach (Vector3 vertex in vertices)
            {
                // convert to real position
                Vector3 newVertexPos = vertex * rot;
                newVertexPos += gameObject.position;

                // make check
                if (MathFExtentions.PointCubeIntersection(newVertexPos, other))
                {
                    result = true;
                    break;
                }

                index++; // TEST CODE
            }

            return result;
        }

        private bool CheckBox(BoxCollider other, Vector3 targetPos)
        {
            bool result = false;

            // find all own vertices

            Vector3[] vertices = new Vector3[8];
            vertices[0] = min;
            vertices[1] = new Vector3(min.X, min.Y, max.Z);
            vertices[2] = new Vector3(max.X, min.Y, min.Z);
            vertices[3] = new Vector3(max.X, min.Y, max.Z);

            vertices[4] = new Vector3(max.X, max.Y, min.Z);
            vertices[5] = new Vector3(min.X, max.Y, min.Z);
            vertices[6] = new Vector3(min.X, max.Y, max.Z);

            vertices[7] = max;

            // iterate

            Matrix3 rot = Matrix3.CreateFromQuaternion(gameObject.qRotation);
            int index = 0; // TEST CODE
            foreach (Vector3 vertex in vertices)
            {
                // convert to real position
                Vector3 newVertexPos = vertex * rot;
                newVertexPos += targetPos;

                // make check
                if (MathFExtentions.PointCubeIntersection(newVertexPos, other))
                {
                    result = true;
                    break;
                }

                index++; // TEST CODE
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
