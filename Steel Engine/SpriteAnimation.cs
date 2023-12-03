using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class SpriteAnimation
    {
        public List<Texture> animation = new List<Texture>();

        public void AddFrame(string framePath, string folderPath)
        {
            Texture texture = Texture.LoadFromFile(framePath, OpenTK.Graphics.OpenGL4.TextureMinFilter.Nearest, OpenTK.Graphics.OpenGL4.TextureMagFilter.Nearest);
            texture.textureName = framePath.Replace(folderPath, "").Split('.')[0];
            texture.textureExtension = "."+framePath.Replace(folderPath, "").Split('.')[1];
            animation.Add(texture);
        }

        public void AddFrame(string spriteListName, string frameName, string extension) // will use usingdataPath
        {
            Texture texture = Texture.LoadFromFile(InfoManager.usingDataPath + @$"\Textures\Animations\{spriteListName}\{frameName}{extension}",
                OpenTK.Graphics.OpenGL4.TextureMinFilter.Nearest, OpenTK.Graphics.OpenGL4.TextureMagFilter.Nearest);
            texture.textureName = frameName;
            texture.textureExtension = extension;
            animation.Add(texture);
        }

        public static SpriteAnimation FromFolder(string path)
        {
            SpriteAnimation result = new SpriteAnimation();

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"ERROR: error while loading animation from folder, does the given file path exists? Path: '{path}'");
                return null;
            }

            foreach (string file in Directory.GetFiles(path))
            {
                result.AddFrame(file, path);
            }

            result.animation = SortAnimationFrames(result.animation);

            return result;
        }

        private static List<Texture> SortAnimationFrames(List<Texture> frames) // all frames must be named just their order number
        {
            Texture[] result = new Texture[frames.Count];

            foreach (Texture frame in frames)
            {
                if (!int.TryParse(frame.textureName, out int id)) {
                    Console.WriteLine("ERROR: animation frame file name is invalid, an animation frame must be named its own order in the animation");
                    return null;
                }
                result[id] = frame;
            }

            return result.ToList();
        }
    }
}