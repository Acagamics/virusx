using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleStormControl
{
    static class MapGenerator
    {
        /// <summary>
        /// possible maps to generate
        /// </summary>
        public enum MapType
        {
            NORMAL,
            CAPTURE_THE_CELL
        };

        static public IEnumerable<MapObject> GenerateLevel(MapType mapType, GraphicsDevice device, ContentManager content, int numPlayers, Background outBackground)
        {
            return GenerateLevel(device, content, numPlayers, outBackground);
        }

        static public List<Vector2> GenerateCellPositionGrid(int numX, int numY, float positionJitter, Vector2 border, Vector2 valueRange)
        {
            // equilateral triangles!
            List<Vector2> spawnPositions = new List<Vector2>();
            for (int x = 0; x < numX; ++x)
            {
                for (int y = 0; y < numY; ++y)
                {
                    // "natural skip"
                    if ((x == numX - 1 && y % 2 == 0) ||
                        ((x == 0 || x == numX - 2 + y % 2) && (y == 0 || y == numY - 1)))
                        continue;

                    // position
                    Vector2 pos = new Vector2((float)x / (numX - 1), (float)y / (numY - 1));
                    if (y % 2 == 0)
                        pos.X += 0.5f / (numX - 1);
                    pos *= valueRange - border * 2.0f;
                    pos += border;

                    // position jitter
                    float posJitter = (float)(Random.NextDouble() * 2.0 - 1.0) * positionJitter;
                    pos += Random.NextDirection() * posJitter;

                    spawnPositions.Add(pos);
                }
            }
            return spawnPositions;
        }

        static private IEnumerable<MapObject> GenerateLevel(GraphicsDevice device, ContentManager content, int numPlayers, Background outBackground)
        {
            // player starts
            List<Vector2> cellPositions = new List<Vector2>();
            const float LEVEL_BORDER = 0.2f;
            const float START_POINT_GLOW_SIZE = 0.35f;
            cellPositions.Add(new Vector2(LEVEL_BORDER, Level.RELATIVE_MAX.Y - LEVEL_BORDER));
            cellPositions.Add(new Vector2(Level.RELATIVE_MAX.X - LEVEL_BORDER, LEVEL_BORDER));
            cellPositions.Add(new Vector2(Level.RELATIVE_MAX.X - LEVEL_BORDER, Level.RELATIVE_MAX.Y - LEVEL_BORDER));
            cellPositions.Add(new Vector2(LEVEL_BORDER, LEVEL_BORDER));

            List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
            for (int playerIndex = 0; playerIndex < Settings.Instance.NumPlayers; ++playerIndex)
            {
                if (Settings.Instance.GetPlayer(playerIndex).Type != Player.Type.NONE)
                {
                    int slot = Settings.Instance.GetPlayer(playerIndex).SlotIndex;
                    spawnPoints.Add(new SpawnPoint(cellPositions[slot], 1000.0f, START_POINT_GLOW_SIZE, playerIndex, content));
                }
            }

            // generate in a grid of equilateral triangles
            const int SPAWNS_GRID_X = 6;
            const int SPAWNS_GRID_Y = 3;
            List<Vector2> spawnPositions = GenerateCellPositionGrid(SPAWNS_GRID_X, SPAWNS_GRID_Y, 0.12f, new Vector2(LEVEL_BORDER), Level.RELATIVE_MAX);


            // random skipping - nonlinear randomness!
            const int MAX_SKIPS = 5;
            int numSkips = (int)(Math.Pow(Random.NextDouble(), 4) * MAX_SKIPS + 0.5f);
            Vector2[] removedCells = new Vector2[numSkips];
            for (int i = 0; i < numSkips; ++i)
            {
                int index = Random.Next(spawnPositions.Count);
                removedCells[i] = spawnPositions[index];
                spawnPositions.RemoveAt(index);
            }
#if EMPTY_LEVELDEBUG
            spawnPositions.Clear();
#endif
            // spawn generation
            foreach (Vector2 pos in spawnPositions)
            {
                // brute force..
                double nearestDist = spawnPositions.Min(x => { return x == pos ? 1 : (x - pos).LengthSquared(); });

                float capturesize = (float)(100.0 + nearestDist * nearestDist * 25000);
                capturesize = Math.Min(capturesize, 1100);

                spawnPoints.Add(new SpawnPoint(pos, capturesize, (float)Math.Sqrt(nearestDist), -1, content));
            }

            // background
            if (outBackground != null)
            {
                List<Vector2> renderCells = new List<Vector2>(spawnPoints.Select(x => x.Position));// to keep things simple, place spawnpoints at the beginning
                renderCells.AddRange(removedCells);
                for (int playerIndex = 0; playerIndex < Settings.Instance.NumPlayers; ++playerIndex)
                {
                    if (Settings.Instance.GetPlayer(playerIndex).Type == Player.Type.NONE)
                        renderCells.Add(cellPositions[Settings.Instance.GetPlayer(playerIndex).SlotIndex]);
                }
                outBackground.Generate(device, renderCells, Level.RELATIVE_MAX);
            }

            return spawnPoints.Cast<MapObject>();
        }
    }
}
