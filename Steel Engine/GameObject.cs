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
        None = -1,
        ShadeFlat = 0,
        ShadeFlatInstanced = 1,
        ShadeTextureUnit = 2,
        ShadeTextureUnitHue = 3,
        ShadeLighting = 4,
        ShadeTextureLighting = 5
    }

    public class GameObject
    {
        public int id;
        public string name;
        public GameObject parent;
        public Vector3 position;
        public Vector3 scale;
        public Vector3 eRotation;
        public Quaternion qRotation { get; private set; }
        
        public RenderShader renderShader;
        private Shader shader;

        private Texture texture0;
        private float mixFactor = 0.5f;

        private int vertexBufferObject;

        private int vao; // specifies which parts of the VBO to use

        private int elementBufferObject; // handle for the Element Buffer Object (EBO)

        public Mesh mesh;

        public List<Component> components = new List<Component>();

        public void AddComponent(Component component) // for runtime adding of components
        {
            components.Add(component);
            component.ComponentInit(this);
        }

        public void SetMixFactor(float factor)
        {
            mixFactor = factor;
        }

        public void ReloadTexture()
        {
            LoadTexture(texture0);
        }

        public void ReloadTexture(TextureMinFilter scaleMethod1, TextureMagFilter scaleMethod2)
        {
            Texture texture = Texture.LoadFromFile(texture0.texturePath, scaleMethod1, scaleMethod2);
            LoadTexture(texture);
        }

        public T GetComponent<T>() where T : class
        {
            T result = null;

            foreach (Component i in components)
            {
                if (i.GetType().IsSubclassOf(typeof(T)) || i.GetType() == typeof(T))
                {
                    result = (T)Convert.ChangeType(i, i.GetType());
                    break;
                }
            }

            return result;
        }

        private void CreateDefaultMesh()
        {
            mesh = new Mesh();
        }

        public void Tick(float deltaTime)
        {
            foreach (Component component in components)
            {
                component.Tick(deltaTime);
            }
            foreach (Component component in components)
            {
                component.LateTick(deltaTime);
            }
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType)
        {
            shader = new Shader(InfoManager.usingDirectory + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.usingDirectory + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = Vector3.Zero;
            scale = Vector3.One;
            eRotation = Vector3.Zero;
            qRotation = Quaternion.Identity;
            renderShader = vertShaderType;
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType, Vector3 pos)
        {
            shader = new Shader(InfoManager.usingDirectory + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.usingDirectory + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = pos;

            scale = Vector3.One;
            eRotation = Vector3.Zero;
            qRotation = Quaternion.Identity;
            renderShader = vertShaderType;
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType, Vector3 pos, Vector3 rot)
        {
            shader = new Shader(InfoManager.usingDirectory + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.usingDirectory + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = pos;

            scale = Vector3.One;
            eRotation = rot;
            qRotation = Quaternion.FromEulerAngles(rot);
            renderShader = vertShaderType;
        }

        public GameObject(RenderShader vertShaderType, RenderShader fragShaderType, Vector3 pos, Quaternion rot)
        {
            shader = new Shader(InfoManager.usingDirectory + @$"\Shaders\Coordinates\{vertShaderType}.vert",
                                InfoManager.usingDirectory + @$"\Shaders\Coordinates\{fragShaderType}.frag");

            CreateDefaultMesh();
            position = pos;

            scale = Vector3.One;
            qRotation = rot;
            Quaternion.ToEulerAngles(rot, out eRotation);
            renderShader = vertShaderType;
        }

        public static GameObject Instantiate(GameObject original)
        {
            GameObject result = new GameObject(original.renderShader, original.renderShader);
            // must do this to create new space in memory
            result.position = new Vector3(original.position.X, original.position.Y, original.position.Z);
            result.qRotation = new Quaternion(original.qRotation.X, original.qRotation.Y, original.qRotation.Z, original.qRotation.W);
            foreach (SteelTriangle tri in original.mesh.triangles)
            {
                result.mesh.AddTriangleRapid(tri.GetVertex(0).position, tri.GetVertex(1).position, tri.GetVertex(2).position, result.mesh);
            }
            result.mesh.RefreshTriangles();
            result.mesh.MergeDuplicates();
            result.mesh.SetColour(original.mesh.vertices[0].colour);
            result.scale = new Vector3(original.scale.X, original.scale.Y, original.scale.Z);
            result.Load();
            return result;
        }

        public static GameObject QuickCopy(GameObject original)
        {
            GameObject result = new GameObject(original.renderShader, original.renderShader);
            result.position = original.position;
            result.qRotation = original.qRotation;
            result.mesh = original.mesh;
            result.scale = original.scale;
            result.Load();
            return result;
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

        public void RotateDegrees(Vector3 rotation)
        {
            Vector3 newRot = new Vector3(MathHelper.DegreesToRadians(rotation.X), MathHelper.DegreesToRadians(rotation.Y), MathHelper.DegreesToRadians(rotation.Z));
            eRotation += newRot;
            qRotation = Quaternion.FromEulerAngles(eRotation);
        }

        public void RotateDegrees(float x, float y, float z)
        {
            Vector3 rotation = new Vector3(MathHelper.DegreesToRadians(x), MathHelper.DegreesToRadians(y), MathHelper.DegreesToRadians(y));
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

        public void LoadTexture(string name, string extension)
        {
            string path = InfoManager.usingDataPath + @$"\Textures\{name}{extension}";

            texture0 = Texture.LoadFromFile(path);
            texture0.textureName = name;
            texture0.textureExtension = extension;
            texture0.texturePath = path;
            texture0.Use(TextureUnit.Texture0);
        }

        public void LoadTexture(string name, string extension, TextureMinFilter scaleMethod1, TextureMagFilter scaleMethod2)
        {
            string path = InfoManager.usingDataPath + @$"\Textures\{name}{extension}";

            texture0 = Texture.LoadFromFile(path, scaleMethod1, scaleMethod2);
            texture0.textureName = name;
            texture0.textureExtension = extension;
            texture0.texturePath = path;
            texture0.Use(TextureUnit.Texture0);
        }

        public void LoadTexture(string path)
        {
            texture0 = Texture.LoadFromFile(path);
            texture0.textureName = path.Split('\\').Last().Split('.').First();
            texture0.textureExtension = "." + path.Split('\\').Last().Split('.').Last();
            texture0.texturePath = path;
            texture0.Use(TextureUnit.Texture0);
        }

        public void LoadTexture(string path, TextureMinFilter scaleMethod1, TextureMagFilter scaleMethod2)
        {
            texture0 = Texture.LoadFromFile(path, scaleMethod1, scaleMethod2);
            texture0.textureName = path.Split('\\').Last().Split('.').First();
            texture0.textureExtension = "." + path.Split('\\').Last().Split('.').Last();
            texture0.texturePath = path;
            texture0.Use(TextureUnit.Texture0);
        }

        public void LoadTexture(Texture texture)
        {
            texture0 = texture;
            texture0.Use(TextureUnit.Texture0);
        }

        public void Load()
        {
            // setup vertex buffer
            vertexBufferObject = GL.GenBuffer(); // get a name for VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject); // bind VBO
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertices.Count * 11 * sizeof(float), GetVertices(), BufferUsageHint.StaticDraw); // allocating GPU memory for VBO
            // the number '11' in the line above represents the number of values per vertex (position(3) + colour(3) + texPos(2) + normal(3) = 11)

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int posLocation = shader.GetAttribLocation("aPosition");
            int colLocation = shader.GetAttribLocation("aColour");
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            int normalLocation = shader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
            GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 6 * sizeof(float));
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(posLocation);
            GL.EnableVertexAttribArray(colLocation);
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.EnableVertexAttribArray(normalLocation);

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

        public void Load(Quaternion preRotation) // JANKYYYY but probably ok because only used by GUI
        {
            // setup vertex buffer
            vertexBufferObject = GL.GenBuffer(); // get a name for VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject); // bind VBO
            GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertices.Count * 11 * sizeof(float), GetVertices(preRotation), BufferUsageHint.StaticDraw); // allocating GPU memory for VBO
            // the number '11' in the line above represents the number of values per vertex (position(3) + colour(3) + texPos(2) + normal(3) = 11)

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int posLocation = shader.GetAttribLocation("aPosition");
            int colLocation = shader.GetAttribLocation("aColour");
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            int normalLocation = shader.GetAttribLocation("aNormal");
            GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
            GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 6 * sizeof(float));
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(posLocation);
            GL.EnableVertexAttribArray(colLocation);
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.EnableVertexAttribArray(normalLocation);

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

            Matrix4 transformation = GetModelMatrix();

            shader.SetMatrix4("model", transformation);
            shader.SetMatrix4("view", InfoManager.engineCamera.GetViewMatrix());
            shader.SetMatrix4("projection", InfoManager.engineCamera.GetProjectionMatrix());

            switch (renderShader)
            {
                case RenderShader.ShadeFlat:
                    int posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    int colLocation = GL.GetAttribLocation(shader.Handle, "aColour");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
                    GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 3 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(colLocation);

                    int[] indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
                case RenderShader.ShadeTextureUnit:
                    texture0.Use(TextureUnit.Texture0);

                    posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    int texPosLocation = GL.GetAttribLocation(shader.Handle, "aTexCoord");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
                    GL.VertexAttribPointer(texPosLocation, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 6 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(texPosLocation);

                    indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
                case RenderShader.ShadeLighting:
                    shader.SetVector3($"lightPosition", LightManager.lights[0].position);
                    shader.SetVector3($"lightColour", LightManager.lights[0].colour);
                    shader.SetVector3($"cameraPosition", InfoManager.engineCamera.Position);
                    //shader.SetFloat($"lightIntensity", LightManager.lights[0].intensity);

                    posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    colLocation = GL.GetAttribLocation(shader.Handle, "aColour");
                    int normLocation = GL.GetAttribLocation(shader.Handle, "aNormal");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
                    GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 3 * sizeof(float));
                    GL.VertexAttribPointer(normLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 8 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(colLocation);
                    GL.EnableVertexAttribArray(normLocation);

                    indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
                case RenderShader.ShadeTextureLighting:
                    texture0.Use(TextureUnit.Texture0);

                    shader.SetVector3($"lightPosition", LightManager.lights[0].position);
                    shader.SetVector3($"lightColour", LightManager.lights[0].colour);
                    shader.SetVector3($"cameraPosition", InfoManager.engineCamera.Position);
                    //shader.SetFloat($"lightIntensity", LightManager.lights[0].intensity);

                    posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    texPosLocation = GL.GetAttribLocation(shader.Handle, "aTexCoord");
                    normLocation = GL.GetAttribLocation(shader.Handle, "aNormal");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
                    GL.VertexAttribPointer(texPosLocation, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 6 * sizeof(float));
                    GL.VertexAttribPointer(normLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 8 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(texPosLocation);
                    GL.EnableVertexAttribArray(normLocation);

                    indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
                case RenderShader.ShadeTextureUnitHue:
                    texture0.Use(TextureUnit.Texture0);

                    shader.SetVector3($"colMod", mesh.vertices[0].colour);
                    shader.SetFloat($"mixFact", mixFactor);

                    posLocation = GL.GetAttribLocation(shader.Handle, "aPosition");
                    texPosLocation = GL.GetAttribLocation(shader.Handle, "aTexCoord");
                    GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);
                    GL.VertexAttribPointer(texPosLocation, 2, VertexAttribPointerType.Float, false, 11 * sizeof(float), 6 * sizeof(float));
                    GL.EnableVertexAttribArray(posLocation);
                    GL.EnableVertexAttribArray(texPosLocation);

                    indices = mesh.GetIndices();

                    GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

                    break;
            }            
        }

        public Texture GetLoadedTexture()
        {
            return texture0;
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

        private float[] GetVertices(Quaternion preRotation) // janky af but only used in GUI so probably ok
        {
            List<float> vertices = new List<float>();

            foreach (SteelVertex vertex in mesh.vertices)
            {
                vertices.AddRange(vertex.GetVertexData(preRotation));
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