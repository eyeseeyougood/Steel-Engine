using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Steel_Engine.Common;
using System.Runtime.InteropServices;

namespace Steel_Engine.ParticleSystem
{
    public struct Particle
    {
        public Vector3 position;
        public Vector4 rotation;
        public Vector3 scale;
        public Vector3 colour;
        public float spawnTime;
        public bool active;

        public Particle(float spawnTime)
        {
            position = new Vector3();
            rotation = new Vector4(0,0,0,1);
            scale = Vector3.One;
            colour = new Vector3();
            this.spawnTime = spawnTime;
            active = true;
        }

        public Particle(Vector3 position, Vector4 rotation, Vector3 scale, Vector3 colour, float spawnTime)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.colour = colour;
            this.spawnTime = spawnTime;
            active = true;
        }
    }

    public enum EmissionType
    {
        Continuous
    }

    public abstract class EmissionShape
    {
        public abstract Particle SpawnParticle(EmitterSettings settings, Particle deactivatedParticle);
    }

    public class EmissionCube : EmissionShape
    {
        public override Particle SpawnParticle(EmitterSettings settings, Particle deactivatedParticle)
        {
            float xOffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.spread;
            float yOffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.spread;
            float zOffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.spread;

            float xROffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.rotationSpread;
            float yROffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.rotationSpread;
            float zROffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.rotationSpread;

            float scaleOffset = ((float)settings.random.Next(-10, 10)) / 10.0f * settings.scaleSpread;

            Quaternion rot = Quaternion.FromEulerAngles(new Vector3(MathHelper.DegreesToRadians(xROffset), MathHelper.DegreesToRadians(yROffset), MathHelper.DegreesToRadians(zROffset)));

            deactivatedParticle.position = new Vector3(xOffset, yOffset, zOffset);
            deactivatedParticle.rotation = new Vector4(rot.X, rot.Y, rot.Z, rot.W);
            deactivatedParticle.scale = new Vector3( settings.particleScale.X + scaleOffset, settings.particleScale.Y + scaleOffset, settings.particleScale.Z + scaleOffset );
            deactivatedParticle.colour = settings.particleColour;
            deactivatedParticle.spawnTime = (float)Time.upTime;
            deactivatedParticle.active = true;

            return deactivatedParticle;
        }
    }

    public class EmitterSettings
    {
        // general settings
        public EmissionType emissionType;
        public EmissionShape emissionShape;

        public Random random;
        public Mesh particleMesh;

        // constant emission type settings
        public int rate;
        public float lifetime;
        public float spread;
        public float rotationSpread;
        public Vector3 particleScale;
        public float scaleSpread;
        public Vector3 particleColour;

        public EmitterSettings(Mesh particleMesh)
        {
            this.emissionType = EmissionType.Continuous;
            this.emissionShape = new EmissionCube();
            this.random = new Random();
            this.particleMesh = particleMesh;
        }
    }

    [SteelComponent]
    public class ParticleEmitter : Component
    {
        public List<Particle> particles = new List<Particle>();
        public int[] particleActivities = new int[128];
        public Vector3[] particlePositions = new Vector3[128];
        public Vector4[] particleRotations = new Vector4[128];
        public Vector3[] particleScales = new Vector3[128];
        public Vector3[] particleColours = new Vector3[128];

        public EmitterSettings settings;

        private float emissionIncrementer;

        private int meshVbo;
        private int ebo;

        private int vao;

        private Shader shader;

        public Vector3 position;
        public Vector3 scale;
        public Vector3 eRotation;
        public Quaternion qRotation { get; private set; }

        private int[] indices;

        public ParticleEmitter()
        {
            Rotate(Vector3.Zero);
        }

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

        private float[] GetVertices()
        {
            // example vertex (6 floats)
            // position, colour
            // 0, 0, 0,  1, 0.5f, 0

            List<float> vertices = new List<float>();

            foreach (SteelVertex vertex in settings.particleMesh.vertices)
            {
                float[] data = vertex.GetVertexData();
                float[] positionData = new float[3];
                positionData[0] = data[0];
                positionData[1] = data[1];
                positionData[2] = data[2];
                vertices.AddRange(positionData);
            }

            return vertices.ToArray();
        }

        private void InitialiseParticles()
        {
            particles.Clear();

            int numParticles = 128;

            if (settings == null)
            {
                settings = new EmitterSettings(OBJImporter.LoadOBJFromPath(InfoManager.usingDirectory + @"\EngineResources\EngineModels\Quad.obj", true));
            }

            // Adapt later for optimisation, removed for now as levels of optimisation don't exist yet :(
            switch (settings.emissionType)
            {
                case EmissionType.Continuous:
                    numParticles = settings.rate * (int)settings.lifetime; // +1 for int cast might round down, +1 for slight timing inaccuracies for despawning causing not enough particles
                    break;
            }

            if (numParticles > 128)
            {
                throw new Exception("Cannot exceed 128 particles per emitter as this has not been implemented yet!");
            }

            numParticles = 128;

            int index = 0;
            while (index < numParticles)
            {
                Particle particle = new Particle((float)Time.upTime);
                particle.active = false;
                particles.Add(particle);
                index++;
            }

        }

        protected override void Init()
        {
            int particleSizeInBytes = 13 * sizeof(float);

            shader = new Shader(InfoManager.usingDirectory + @$"\Shaders\Coordinates\{RenderShader.ShadeFlatParticles128}.vert",
                    InfoManager.usingDirectory + @$"\Shaders\Coordinates\{RenderShader.ShadeFlatParticles128}.frag");

            InitialiseParticles();

            indices = settings.particleMesh.GetIndices();

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            meshVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVbo); // bind VBO
            GL.BufferData(BufferTarget.ArrayBuffer, settings.particleMesh.vertices.Count * 3 * sizeof(float), GetVertices(), BufferUsageHint.StaticDraw);

            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, meshVbo);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribDivisor(0, 0);

            shader.Use();
        }

        public override void Tick(float deltaTime)
        {
            // despawn old particles
            int index = 0;
            while (index < particles.Count)
            {
                Particle particle = particles[index];
                if (particle.spawnTime + settings.lifetime <= (float)Time.upTime && particle.active)
                {
                    particle.active = false;
                    particles[index] = particle;
                }
                index++;
            }

            // spawn particles
            switch (settings.emissionType)
            {
                case EmissionType.Continuous:
                    emissionIncrementer += deltaTime;
                    if (emissionIncrementer > 1.0f / settings.rate)
                    {
                        emissionIncrementer = 0;
                        // find deactivated particle
                        Particle deactivatedParticle = new Particle();
                        bool found = false;
                        int id = -1;
                        index = 0;
                        foreach (Particle p in particles)
                        {
                            if (!p.active)
                            {
                                deactivatedParticle = p;
                                found = true;
                                id = index;
                                break;
                            }
                            index++;
                        }

                        // 'spawn' the particle
                        if (found)
                            particles[id] = settings.emissionShape.SpawnParticle(settings, deactivatedParticle);
                    }
                    break;
            }

            RefreshActivities();
            RefreshPositions();
            RefreshRotations();
            RefreshScales();
            RefreshColours();
        }

        private void RefreshPositions()
        {
            int index = 0;
            while (index < particlePositions.Length)
            {
                if (index < particles.Count)
                {
                    particlePositions[index] = particles[index].position;
                }
                else
                {
                    particlePositions[index] = Vector3.Zero;
                }
                index++;
            }
        }

        private void RefreshRotations()
        {
            int index = 0;
            while (index < particleRotations.Length)
            {
                if (index < particles.Count)
                {
                    particleRotations[index] = particles[index].rotation;
                }
                else
                {
                    particleRotations[index] = Vector4.Zero;
                }
                index++;
            }
        }

        private void RefreshScales()
        {
            int index = 0;
            while (index < particleScales.Length)
            {
                if (index < particles.Count)
                {
                    particleScales[index] = particles[index].scale;
                }
                else
                {
                    particleScales[index] = Vector3.Zero;
                }
                index++;
            }
        }

        private void RefreshColours()
        {
            int index = 0;
            while (index < particleColours.Length)
            {
                if (index < particles.Count)
                {
                    particleColours[index] = particles[index].colour;
                }
                else
                {
                    particleColours[index] = Vector3.Zero;
                }
                index++;
            }
        }

        private void RefreshActivities()
        {
            int index = 0;
            while (index < particleActivities.Length)
            {
                if (index < particles.Count)
                {
                    particleActivities[index] = particles[index].active ? 1 : 0;
                }
                else
                {
                    particleActivities[index] = 0;
                }
                index++;
            }
        }

        public override void OnGameObjectRendered()
        {
            shader.Use();

            Matrix4 transformation = GetModelMatrix();

            shader.SetMatrix4("model", transformation);
            shader.SetMatrix4("view", InfoManager.engineCamera.GetViewMatrix());
            shader.SetMatrix4("projection", InfoManager.engineCamera.GetProjectionMatrix());

            shader.SetIntArray("activities[0]", particleActivities);
            shader.SetVector3Array("positions[0]", particlePositions);
            shader.SetVector4Array("rotations[0]", particleRotations);
            shader.SetVector3Array("scales[0]", particleScales);
            shader.SetVector3Array("colours[0]", particleColours);

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            GL.DrawElementsInstanced(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, particles.Count);
        }
    }
}