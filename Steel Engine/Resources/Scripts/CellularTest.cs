using OpenTK.Mathematics;
using Steel_Engine;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using Steel_Engine.Tilemaps;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class CellularTest : Component
{
    private Tile aliveTile;
    private Tilemap tilemap;

    private bool advance;

    // TestCode
    private int inc = 0;
    private int frameSkip = 4;

    protected override void Init()
    {
        Texture sample = Texture.SampleSpriteSheet(InfoManager.usingDataPath + @"\Textures\FreePlatformerNA\Foreground\Tileset.png", new Vector2i(64, 32), new Vector2i(79, 47));
        aliveTile = new Tile(sample, 0.5f);

        tilemap = new Tilemap(new Vector3(0, 0, 0), 0.5f);
        SceneManager.tilemaps.Add(tilemap);
        SceneManager.ChangeClearColour(Vector3.Zero);
    }

    public int GetNeighbourCount(Vector2i pos)
    {
        int count = 0;
        if (tilemap.GetTile(pos + new Vector2i(-1, 0)) == aliveTile)
        {
            count++;
        }
        if (tilemap.GetTile(pos + new Vector2i(1, 0)) == aliveTile)
        {
            count++;
        }
        if (tilemap.GetTile(pos + new Vector2i(0, 1)) == aliveTile)
        {
            count++;
        }
        if (tilemap.GetTile(pos + new Vector2i(0, -1)) == aliveTile)
        {
            count++;
        }

        if (tilemap.GetTile(pos + new Vector2i(-1, -1)) == aliveTile)
        {
            count++;
        }
        if (tilemap.GetTile(pos + new Vector2i(1, -1)) == aliveTile)
        {
            count++;
        }
        if (tilemap.GetTile(pos + new Vector2i(1, 1)) == aliveTile)
        {
            count++;
        }
        if (tilemap.GetTile(pos + new Vector2i(-1, 1)) == aliveTile)
        {
            count++;
        }
        return count;
    }

    public List<Vector2i> GetDeadCells(List<Vector2i> aliveCells)
    {
        List<Vector2i> deadCells = new List<Vector2i>();
        List<Vector2i> cellsToCheck = new List<Vector2i>();
        foreach (Vector2i cell in aliveCells)
        {
            cellsToCheck.Add(cell + new Vector2i(1, 0));
            cellsToCheck.Add(cell + new Vector2i(-1, 0));
            cellsToCheck.Add(cell + new Vector2i(0, -1));
            cellsToCheck.Add(cell + new Vector2i(0, 1));

            cellsToCheck.Add(cell + new Vector2i(1, -1));
            cellsToCheck.Add(cell + new Vector2i(-1, -1));
            cellsToCheck.Add(cell + new Vector2i(1, 1));
            cellsToCheck.Add(cell + new Vector2i(-1, 1));
        }
        foreach (Vector2i cell in cellsToCheck)
        {
            if (tilemap.GetTile(cell) == null)
            {
                deadCells.Add(cell);
            }
        }
        return deadCells;
    }

    public override void Tick(float deltaTime)
    {
        if (InputManager.GetMouseButtonDown(0))
        {
            SteelRay ray = SceneManager.CalculateRay(InputManager.mousePosition);
            Vector2 worldPos = MathFExtentions.LinePlaneIntersection(Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, ray.worldPosition, ray.worldDirection).Xy;
            Vector2i tilePos = tilemap.WorldSpaceToTileSpace(worldPos);
            if (tilemap.GetTile(tilePos) == aliveTile)
            {
                tilemap.RemoveTile(tilePos);
            }
            else
            {
                tilemap.SetTile(tilePos, aliveTile);
            }
        }
        if (InputManager.GetMouseButton(0))
        {
            SteelRay ray = SceneManager.CalculateRay(InputManager.mousePosition);
            Vector2 worldPos = MathFExtentions.LinePlaneIntersection(Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, ray.worldPosition, ray.worldDirection).Xy;
            Vector2i tilePos = tilemap.WorldSpaceToTileSpace(worldPos);
            if (tilemap.GetTile(tilePos) == null)
            {
                tilemap.SetTile(tilePos, aliveTile);
            }
        }
        if (InputManager.GetMouseButtonUp(0))
        {
            tilemap.GenerateTilemap();
        }


        if (advance)
        {
            foreach (Vector2i tile in tilemap.GetPositions())
            {
                if (tilemap.GetTile(tile + new Vector2i(0, -1)) == null)
                {
                    tilemap.RemoveTile(tile);
                }
            }
            foreach (Vector2i cell in GetDeadCells(tilemap.GetPositions()))
            {
                if (tilemap.GetTile(cell + new Vector2i(0, 1)) == aliveTile)
                {
                    tilemap.SetTile(cell, aliveTile);
                }
            }
            tilemap.GenerateTilemap();
            advance = false;
        }

        if (InputManager.GetKeyDown(Keys.E))
        {
            advance = true;
        }

        if (InputManager.GetKey(Keys.F))
        {
            if (inc == frameSkip)
            {
                advance = true;
                inc = 0;
            }
            else
            {
                inc++;
            }
        }

        if (InputManager.GetKey(Keys.W))
        {
            InfoManager.engineCamera.Position += Vector3.UnitY * deltaTime * 8f;
        }
        if (InputManager.GetKey(Keys.S))
        {
            InfoManager.engineCamera.Position += -Vector3.UnitY * deltaTime * 8f;
        }
        if (InputManager.GetKey(Keys.A))
        {
            InfoManager.engineCamera.Position += -Vector3.UnitX * deltaTime * 8f;
        }
        if (InputManager.GetKey(Keys.D))
        {
            InfoManager.engineCamera.Position += Vector3.UnitX * deltaTime * 8f;
        }

        if (InputManager.GetKey(Keys.Z))
        {
            InfoManager.engineCamera.Fov += 15f * deltaTime;
        }
        if (InputManager.GetKey(Keys.X))
        {
            InfoManager.engineCamera.Fov -= 15f * deltaTime;
        }
    }
}