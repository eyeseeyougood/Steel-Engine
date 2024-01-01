using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Steel_Engine.Common;

namespace Steel_Engine.ParticleSystem
{
    public struct Particle
    {
        public Vector3 position;
        public Vector4 rotation;
        public Vector3 scale;
        public Vector3 colour;
    }

    public class ParticleEmitter
    {
        public List<Particle> particles = new List<Particle>();

        public Mesh mesh;

        private int meshVbo;
        private int instancesVbo;
        private int ebo;

        private int vao;

        private Shader shader;

        public Vector3 position;
        public Vector3 scale;
        public Vector3 eRotation;
        public Quaternion qRotation { get; private set; }

        private int[] indices;

        public Matrix4 GetModelMatrix()
        {
            Matrix4 transformation = Matrix4.Identity;
            transformation = transformation * Matrix4.CreateScale(scale);
            transformation = transformation * Matrix4.CreateFromQuaternion(qRotation);
            transformation = transformation * Matrix4.CreateTranslation(position);
            return transformation;
        }

        public void Rotate(Vector3 rotation)
        {
            eRotation += rotation;
            qRotation = Quaternion.FromEulerAngles(eRotation);
        }
        public void Rotate(float x, float y, float z)
        {
            Vector3 rotation = new Vector3(x, y, z);
            eRotation += rotation;
            qRotation = Quaternion.FromEulerAngles(eRotation);
        }

        public void SetRotation(Vector3 rotation)
        {
            eRotation = rotation;
            qRotation = Quaternion.FromEulerAngles(eRotation);
        }

        public void SetRotation(Quaternion rotation)
        {
            Quaternion.ToEulerAngles(rotation, out eRotation);
            qRotation = rotation;
        }

        private float[] GetParticleData()
        {
            List<float> vertices = new List<float>();

            foreach (Particle particle in particles)
            {
                vertices.Add(particle.position.X);
                vertices.Add(particle.position.Y);
                vertices.Add(particle.position.Z);
                vertices.Add(particle.rotation.X);
                vertices.Add(particle.rotation.Y);
                vertices.Add(particle.rotation.Z);
                vertices.Add(particle.rotation.W);
                vertices.Add(particle.scale.X);
                vertices.Add(particle.scale.Y);
                vertices.Add(particle.scale.Z);
                vertices.Add(particle.colour.X);
                vertices.Add(particle.colour.Y);
                vertices.Add(particle.colour.Z);
            }

            return vertices.ToArray();
        }

        private float[] GetVertices()
        {
            // example vertex (6 floats)
            // position, colour
            // 0, 0, 0,  1, 0.5f, 0

            List<float> vertices = new List<float>();

            foreach (SteelVertex vertex in mesh.vertices)
            {
                vertices.AddRange(vertex.GetVertexData());
            }

            return vertices.ToArray();
        }

        public void Init()
        {
            int particleSizeInBytes = 13 * sizeof(float);

            indices = mesh.GetIndices();

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            meshVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVbo); // bind VBO
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertices.Count * 11 * sizeof(float), GetVertices(), BufferUsageHint.StaticDraw);
            instancesVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, instancesVbo); // bind VBO
            GL.BufferData(BufferTarget.ArrayBuffer, particles.Count * particleSizeInBytes, GetParticleData(), BufferUsageHint.StaticDraw);

            shader = new Shader(InfoManager.usingDirectory + @$"\Shaders\Coordinates\{RenderShader.ShadeFlatInstanced}.vert",
                    InfoManager.usingDirectory + @$"\Shaders\Coordinates\{RenderShader.ShadeFlatInstanced}.frag");

            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, instancesVbo);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.VertexAttribDivisor(1, 1);

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes, 3 * sizeof(float));
            GL.VertexAttribDivisor(2, 1);

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 7 * sizeof(float));
            GL.VertexAttribDivisor(3, 1);

            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 10 * sizeof(float));
            GL.VertexAttribDivisor(4, 1);

            GL.BindVertexArray(0);
        }

        public void Render()
        {
            shader.Use();

            Matrix4 transformation = GetModelMatrix();

            shader.SetMatrix4("model", transformation);
            shader.SetMatrix4("view", InfoManager.engineCamera.GetViewMatrix());
            shader.SetMatrix4("projection", InfoManager.engineCamera.GetProjectionMatrix());

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, instancesVbo);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.VertexAttribDivisor(1, 1);

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, Vector4.SizeInBytes, 3 * sizeof(float));
            GL.VertexAttribDivisor(2, 1);

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 7 * sizeof(float));
            GL.VertexAttribDivisor(3, 1);

            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 10 * sizeof(float));
            GL.VertexAttribDivisor(4, 1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, instancesVbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            GL.DrawElementsInstanced(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, particles.Count);
        }
    }
}
