using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public enum RenderShader
    {
        ShadeFlat = 0,
        ShadeTextureUnit = 1,
        ShadeLighting = 2
    }

    public class GameObject
    {
        public int id;
        public GameObject parent;
        public Vector3 position;
        public Vector3 scale;
        public Vector3 eRotation;
        private Quaternion qRotation;

        public RenderShader renderShader;
        private Shader shader;

        private Texture texture0;

        private int vertexBufferObject;

        private int vao; // specifies which parts of the VBO to use

        private int elementBufferObject; // handle for the Element Buffer Object (EBO)

        public Mesh mesh;

        public List<Component> components = new List<Component>();

        private void CreateDefaultMesh()
        {
            mesh = new Mesh();
        }

        public Matrix4 GetTransform()
        {
            Matrix4 tranformation = Matrix4.Identity;
            tranformation = tranformation * Matrix4.CreateScale(scale);
            tranformation = tranformation * Matrix4.CreateFromQuaternion(qRotation);
            tranformation = tranformation * Matrix4.CreateTranslation(position);

            return tranformation;
        }

        public void Tick(float deltaTime)
        {
            foreach (Component component in components)
            {
                component.Tick(deltaTime);
            }
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType)
        {
            shader = new Shader(InfoManager.currentDir + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.currentDir + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = Vector3.Zero;
            scale = Vector3.One;
            eRotation = Vector3.Zero;
            qRotation = Quaternion.Identity;
            renderShader = vertShaderType;
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType, Vector3 pos)
        {
            shader = new Shader(InfoManager.currentDir + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.currentDir + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = pos;

            scale = Vector3.One;
            eRotation = Vector3.Zero;
            qRotation = Quaternion.Identity;
            renderShader = vertShaderType;
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType, Vector3 pos, Vector3 rot)
        {
            shader = new Shader(InfoManager.currentDir + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.currentDir + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = pos;

            scale = Vector3.One;
            eRotation = rot;
            qRotation = Quaternion.FromEulerAngles(rot);
            renderShader = vertShaderType;
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType, Vector3 pos, Quaternion rot)
        {
            shader = new Shader(InfoManager.currentDir + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.currentDir + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = pos;

            scale = Vector3.One;
            qRotation = rot;
            Quaternion.ToEulerAngles(rot, out eRotation);
            renderShader = vertShaderType;
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

        public void LoadTexture(string name, string extention)
        {
            string path = InfoManager.dataPath + @$"\Textures\{name}{extention}";

            texture0 = Texture.LoadFromFile(path);
            texture0.Use(TextureUnit.Texture0);
        }

        public void LoadTexture(string path)
        {
            texture0 = Texture.LoadFromFile(path);
            texture0.Use(TextureUnit.Texture0);
        }

        public void Load()
        {
            // setup vertex buffer
            vertexBufferObject = GL.GenBuffer(); // get a name for VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject); // bind VBO
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertices.Count * 8 * sizeof(float), GetVertices(), BufferUsageHint.StaticDraw); // allocating GPU memory for VBO
            // the number '8' in the line above represents the number of values per vertex (position(3) + colour(3) + texPos(2) = 8)

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int posLocation = shader.GetAttribLocation("aPosition");
            int colLocation = shader.GetAttribLocation("aColour");
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(posLocation);
            GL.EnableVertexAttribArray(colLocation);
            GL.EnableVertexAttribArray(texCoordLocation);

            // init elementBufferObject
            int[] indices = mesh.GetIndices();

            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);

            // init shader
            shader.Use();

            // init components
            foreach (Component component in components)
            {
                component.ComponentInit(this);
            }
        }

        public void Render()
        {
            shader.Use();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject); // bind VBO
            GL.BindVertexArray(vao); // bind VAO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            Matrix4 tranformation = Matrix4.Identity;
            tranformation = tranformation * Matrix4.CreateScale(scale);
            tranformation = tranformation * Matrix4.CreateFromQuaternion(qRotation);
            tranformation = tranformation * Matrix4.CreateTranslation(position);

            shader.SetMatrix4("model", tranformation);
            shader.SetMatrix4("view", InfoManager.engineCamera.GetViewMatrix());
            shader.SetMatrix4("projection", InfoManager.engineCamera.GetProjectionMatrix());

            switch (renderShader)
            {
                case RenderShader.ShadeFlat:
                    int posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    int colLocation = GL.GetAttribLocation(shader.Handle, "aColour");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                    GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(colLocation);


                    int[] indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
                case RenderShader.ShadeTextureUnit:
                    texture0.Use(TextureUnit.Texture0);

                    posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    int texPosLocation = GL.GetAttribLocation(shader.Handle, "aTexCoord");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                    GL.VertexAttribPointer(texPosLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(texPosLocation);

                    indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
                case RenderShader.ShadeLighting:
                    shader.SetMatrix4($"lightModel", Matrix4.CreateTranslation(LightManager.lights[0].position));
                    shader.SetVector3($"lightColour", LightManager.lights[0].colour);
                    shader.SetFloat($"lightIntensity", LightManager.lights[0].intensity);

                    posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    colLocation = GL.GetAttribLocation(shader.Handle, "aColour");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                    GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(colLocation);

                    indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
            }            
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

        private int[] GetIndices()
        {
            // example index (3 floats)
            // index of first vertex in vertex list, index of second vertex in vertex list, index of third vertex in vertex list (all to make up 1 triangle)
            // 0,                                    1,                                     2

            return mesh.GetIndices();
        }
    }
}