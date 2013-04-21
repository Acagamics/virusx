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

        #region control variables

        private Vector2 targetPosition = Vector2.Zero;
        private SpawnPoint targetSpawnPoint = null;
        //private Item targetTarget = null;

        private float maxTargetTime = 5f;
        private float currentTargetTime = 2.5f;
        private bool TargetTimeReached { get { return currentTargetTime >= maxTargetTime; } }

        #endregion

        public AIPlayer(int playerIndex, int virusIndex, int colorIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture) :
            base(playerIndex, virusIndex, colorIndex, device, content, noiseTexture)
        {
            targetPosition = particleAttractionPosition = cursorPosition = cursorStartPositions[playerIndex];
        }

        public override void UserControl(float frameTimeInterval, Level level)
        {
            CheckItems(level);
            CheckTargets();
            if (TargetTimeReached)
            {
                SelectTarget(level);
                currentTargetTime = 0.0f;
            }
            else
            {
                currentTargetTime += frameTimeInterval;
            }

            //if(targetPosition != new Vector2(-1f,-1f))
                MoveCursor(frameTimeInterval);

            /*particleAttractionPosition.X += (float)Random.NextDouble(-0.03, 0.03);
            particleAttractionPosition.Y += (float)Random.NextDouble(-0.03, 0.03);*/
        }

        private void CheckItems(Level level)
        {
            int step = level.GameStatistics.Steps-1;
            switch (ItemSlot)
            {
                case Item.ItemType.DANGER_ZONE:
                    if (targetSpawnPoint != null && (targetPosition - cursorPosition).Length() < 0.02f)
                    {
                        UseItem(level);
                    }
                    break;
                case Item.ItemType.MUTATION:
                    if (level.GameStatistics.getDominationInStep(Index, step) < 0.20f)
                    {
                        UseItem(level);
                    }
                    break;
                case Item.ItemType.WIPEOUT:
                    uint overall = level.GameStatistics.getParticlesInStep(step);
                    if ((float)NumParticlesAlive / overall < 0.25f)
                    {
                        UseItem(level);
                    }
                    for(int i=0;i<level.GameStatistics.PlayerCount;++i)
                    {
                        if(i!= Index)
                            if (level.GameStatistics.getPossessingSpawnPointsInStep(i, step) == 0)
                            {
                                UseItem(level);
                            }
                    }
                    break;
                default: break;
            }
        }

        private void UseItem(Level level)
        {
            level.PlayerUseItem(this);
            ItemSlot = Item.ItemType.NONE;
        }

        private void CheckTargets()
        {
            if (targetSpawnPoint != null)
            {
                if (targetSpawnPoint.PossessingPlayer == Index)
                {
                    targetSpawnPoint = null;
                    currentTargetTime = maxTargetTime + 0.1f;
                }
            }
        }

        private void SelectTarget(Level level)
        {
            // find middle
            var ownSpawnpoints = level.SpawnPoints.Where(x => x.PossessingPlayer == Index);
            Vector2 ownTerritoriumMid = Vector2.Zero;
            foreach (SpawnPoint spawn in ownSpawnpoints)
            {
                ownTerritoriumMid += spawn.Position * spawn.Size;
            }
            ownTerritoriumMid /= PossessingSpawnPointsOverallSize;

            // find nearest not owning spawnpoint;
            var otherSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer != Index);
            // remove current target to prevent that to much time is spent on an unreachable target
            if (otherSpawnPoints.Count() > 1 && targetSpawnPoint != null)
                otherSpawnPoints = otherSpawnPoints.Where(x => x != targetSpawnPoint);
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
                        targetPosition = spawn.Position;
                        targetSpawnPoint = spawn;
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
                        targetPosition = spawn.Position;
                        targetSpawnPoint = spawn;
                    }
                }
            }

            targetPosition.X += (float)Random.NextDouble(-0.03, 0.03);
            targetPosition.Y += (float)Random.NextDouble(-0.03, 0.03);
        }

        private void MoveCursor(float frameTimeInterval)
        {
            Vector2 cursorMove = targetPosition - cursorPosition;
            if (cursorMove.Length() < 0.02f) return;

            cursorMove /= cursorMove.Length();
            cursorMove *= frameTimeInterval * CURSOR_SPEED;

            float len = cursorMove.Length();
            if (len > 1.0f) cursorMove /= len;
            cursorPosition += (cursorMove * 0.65f);

            cursorPosition.X = MathHelper.Clamp(cursorPosition.X, 0.0f, Level.RELATIVE_MAX.X);
            cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, 0.0f, Level.RELATIVE_MAX.Y);
            particleAttractionPosition = CursorPosition;
        }
    }
}
