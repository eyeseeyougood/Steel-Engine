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
            min = -Vector3.One * gameObject.scale + gameObject.position;
            max = Vector3.One * gameObject.scale + gameObject.position;
        }

        public override void Tick(float deltaTime)
        {
            min = -Vector3.One * gameObject.scale + gameObject.position;
            max = Vector3.One * gameObject.scale + gameObject.position;
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
