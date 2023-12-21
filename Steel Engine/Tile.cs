using OpenTK.Mathematics;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.Tilemaps
{
    public class Tile
    {
        public float scale { get; private set; }
        public Texture texture { get; private set; }

        public Tile(ImageReference imageReference)
        {
            texture = Texture.LoadFromFile(imageReference.path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }

        public Tile(ImageReference imageReference, float scaleFactor)
        {
            texture = Texture.LoadFromFile(imageReference.path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            scale = scaleFactor;
        }

        public Tile(Texture sprite)
        {
            texture = sprite;
            scale = 1;
        }

        public Tile(Texture sprite, float scaleFactor)
        {
            texture = sprite;
            scale = scaleFactor;
        }
    }
}