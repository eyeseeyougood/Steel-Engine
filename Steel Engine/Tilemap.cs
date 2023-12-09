using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.Tilemaps
{
    public class Tilemap
    {
        public Vector3 position { get; private set; }
        public float gridSize { get; private set; }
        private Dictionary<Vector2i, Tile> tiles = new Dictionary<Vector2i, Tile>();
        private Dictionary<Vector2i, Tile> bakedTiles = new Dictionary<Vector2i, Tile>();
        private List<GameObject> gameObjects = new List<GameObject>();
        private GameObject quad;

        public List<Vector2i> GetPositions()
        {
            return bakedTiles.Keys.ToList();
        }

        public Vector2 TileSpaceToWorldSpace(Vector2i tileSpacePosition)
        {
            return new Vector2(tileSpacePosition.X * gridSize * 2 + position.X, tileSpacePosition.Y * gridSize * 2 + position.Y);
        }

        public Vector2i WorldSpaceToTileSpace(Vector2 worldSpacePosition)
        {
            return new Vector2i((int)(MathF.Round((worldSpacePosition.X-position.X) / gridSize / 2) * gridSize * 2), (int)(MathF.Round((worldSpacePosition.Y-position.Y) / gridSize / 2) * gridSize * 2));
        }

        public void SetTile(Vector2i position, Tile tile)
        {
            if (!tiles.ContainsKey(position))
            {
                tiles.Add(position, tile);
            }
            else
            {
                tiles[position] = tile;
            }
        }

        public void RemoveTile(Vector2i position)
        {
            if (tiles.ContainsKey(position))
                tiles.Remove(position);
        }

        public Tile GetTile(Vector2i position)
        {
            Tile result = null;

            if (bakedTiles.ContainsKey(position))
                result = bakedTiles[position];

            return result;
        }

        public void Clear()
        {
            tiles.Clear();
            gameObjects.Clear();
        }

        public void GenerateTilemap()
        {
            gameObjects.Clear();
            Dictionary<Vector2i, Tile> temp = new Dictionary<Vector2i, Tile>(tiles);
            bakedTiles = temp;
            foreach (KeyValuePair<Vector2i, Tile> tile in tiles)
            {
                GameObject go = GameObject.QuickCopy(quad);
                Vector2 worldPos = TileSpaceToWorldSpace(tile.Key);
                go.position = new Vector3(worldPos.X, worldPos.Y, position.Z);
                go.scale = Vector3.One * tile.Value.scale;
                go.LoadTexture(tile.Value.texture);
                gameObjects.Add(go);
            }
        }

        public void Render()
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Render();
            }
        }

        public Tilemap(Vector3 tilemapPosition)
        {
            GenQuad();
            position = tilemapPosition;
            gridSize = 1.0f;
        }

        public Tilemap(Vector3 tilemapPosition, float tilemapGridSize)
        {
            GenQuad();
            position = tilemapPosition * 2;
            gridSize = tilemapGridSize;
        }

        private void GenQuad()
        {
            quad = new GameObject(RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit);
            quad.mesh = OBJImporter.LoadOBJFromPath(InfoManager.usingDirectory + @"\EngineResources\EngineModels\Quad.obj", true);
            quad.Load();
        }
    }
}