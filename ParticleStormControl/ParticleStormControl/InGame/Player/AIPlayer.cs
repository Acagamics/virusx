using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleStormControl
{
    public class AIPlayer : Player
    {
        public AIPlayer(int playerIndex, int virusIndex, int colorIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture) :
            base(playerIndex, virusIndex, colorIndex, device, content, noiseTexture)
        {
        }

        public override void UserControl(float frameTimeInterval, Level level)
        {
            // find middle
            var ownSpawnpoints = level.SpawnPoints.Where(x => x.PossessingPlayer == Index);
            Vector2 ownTerritoriumMid = Vector2.Zero;
            foreach(SpawnPoint spawn in ownSpawnpoints)
            {
                ownTerritoriumMid += spawn.Position * spawn.Size;
            }
            ownTerritoriumMid /= PossessingSpawnPointsOverallSize;

            // find nearest not owning spawnpoint;
            var otherSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer != Index);
            // find nearest spawn point which no player tries to capture
            var noOtherPlayerTriesToCapture = otherSpawnPoints.Where(x => (x.CapturingPlayer == Index || x.CapturingPlayer == -1));
            
            float minDistSq = 99999;
            if (noOtherPlayerTriesToCapture.Count() != 0)
            {
                foreach (SpawnPoint spawn in noOtherPlayerTriesToCapture)
                {
                    float newDistSq = Vector2.DistanceSquared(spawn.Position, ownTerritoriumMid);
                    if (newDistSq < minDistSq)
                    {
                        minDistSq = newDistSq;
                        particleAttractionPosition = spawn.Position;
                    }
                }
            }
            else
            {
                foreach (SpawnPoint spawn in otherSpawnPoints)
                {
                    float newDistSq = Vector2.DistanceSquared(spawn.Position, ownTerritoriumMid);
                    if (newDistSq < minDistSq)
                    {
                        minDistSq = newDistSq;
                        particleAttractionPosition = spawn.Position;
                    }
                }
            }

            particleAttractionPosition.X += (float)Random.NextDouble(-0.03, 0.03);
            particleAttractionPosition.Y += (float)Random.NextDouble(-0.03, 0.03);
        }
    }
}
