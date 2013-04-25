#define AI_DEBUG

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
        Vector2 ownTerritoriumMid = Vector2.Zero;
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
            CheckTargets(level);
            if (TargetTimeReached)
            {
                SelectTarget(level);
                currentTargetTime = 0.0f;
            }
            else
            {
                currentTargetTime += frameTimeInterval;
            }

            MoveCursor(frameTimeInterval);

#if AI_DEBUG
            ownTerritoriumMid = (ownTerritoriumMid + particleAttractionPosition) / 2f;
            cursorPosition = ownTerritoriumMid;
#endif
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

        private void CheckTargets(Level level)
        {
            if (targetSpawnPoint != null)
            {
                if (targetSpawnPoint.PossessingPlayer == Index)
                {
                    targetSpawnPoint = null;
                    currentTargetTime = maxTargetTime + 0.1f;
                }
                /*var ownSpawnpoints = level.SpawnPoints.Where(x => x.PossessingPlayer == Index);
                // check for spawn points which are unter attack and eventually protrect them
                //var ownSpawnPointsUnterAttack = ownSpawnpoints.Where(x => (x.CapturingPlayer != Index && x.CapturingPlayer != -1));
                var ownSpawnPointsUnterAttack = ownSpawnpoints.Where(x => (x.PossessingPercentage < 0.25f));
                //float meanSpawnSize = ownSpawnpoints.Sum(x => x.SpawnSize) / ownSpawnpoints.Count();
                //ownSpawnPointsUnterAttack = ownSpawnPointsUnterAttack.Where(x => (x.SpawnSize >= (meanSpawnSize / 2)));// && x.PossessingPercentage > 0.5));

                if (ownSpawnPointsUnterAttack.Count() > 0)
                {
                    //targetSpawnPoint = selectTargetSpawnPointFromList(ownSpawnPointsUnterAttack);
                    targetSpawnPoint = null;
                    currentTargetTime = maxTargetTime + 0.1f;
                }*/
                /*if (targetSpawnPoint != null)
                    targetPosition = targetSpawnPoint.Position;
                targetPosition.X += (float)Random.NextDouble(-0.03, 0.03);
                targetPosition.Y += (float)Random.NextDouble(-0.03, 0.03);*/
            }
        }

        private void SelectTarget(Level level)
        {
            // find middle
            var ownSpawnpoints = level.SpawnPoints.Where(x => x.PossessingPlayer == Index);
            if (ownSpawnpoints.Count() < 1) ownTerritoriumMid = Player.cursorStartPositions[Index];
            else ownTerritoriumMid = Vector2.Zero;
            foreach (SpawnPoint spawn in ownSpawnpoints)
            {
                ownTerritoriumMid += spawn.Position;// *spawn.Size;
            }
            ownTerritoriumMid /= (ownSpawnpoints.Count());// * PossessingSpawnPointsOverallSize);
            ownTerritoriumMid = (ownTerritoriumMid + particleAttractionPosition) / 2f;
#if AI_DEBUG
            cursorPosition = ownTerritoriumMid;
#endif
            // find nearest not owning spawnpoint;
            var otherSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer != Index);
            // remove current target to prevent that to much time is spent on an unreachable target
            if (otherSpawnPoints.Count() > 1 && targetSpawnPoint != null)
                otherSpawnPoints = otherSpawnPoints.Where(x => x != targetSpawnPoint);
            targetSpawnPoint = null;
            // find nearest spawn point which no player tries to capture
            var noOtherPlayerTriesToCapture = otherSpawnPoints.Where(x => (x.CapturingPlayer == Index || x.CapturingPlayer == -1));
            // eliminate all spawn points in possession of another player
            noOtherPlayerTriesToCapture = noOtherPlayerTriesToCapture.Where(x => (x.PossessingPlayer == -1));

            // check for spawn points which are unter attack and eventually protrect them
            //var ownSpawnPointsUnterAttack = ownSpawnpoints.Where(x => (x.CapturingPlayer != Index && x.CapturingPlayer != -1));
            var ownSpawnPointsUnterAttack = ownSpawnpoints.Where(x => (x.PossessingPercentage < 0.5f));
            float meanSpawnSize = ownSpawnpoints.Sum(x => x.SpawnSize) / ownSpawnpoints.Count();
            ownSpawnPointsUnterAttack = ownSpawnPointsUnterAttack.Where(x => (x.SpawnSize >= (meanSpawnSize/2) && x.PossessingPercentage < 0.5));

            //targetSpawnPoint = selectTargetSpawnPointFromList(ownSpawnPointsUnterAttack);
            if (targetSpawnPoint == null)
                targetSpawnPoint = selectTargetSpawnPointFromList(noOtherPlayerTriesToCapture);
            if (targetSpawnPoint == null)
                targetSpawnPoint = selectTargetSpawnPointFromList(otherSpawnPoints);
            /*float minDistSq = 99999;
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
            }*/

            if (targetSpawnPoint != null)
                targetPosition = targetSpawnPoint.Position;
            else
                targetPosition = ownTerritoriumMid;
            targetPosition.X += (float)Random.NextDouble(-0.03, 0.03);
            targetPosition.Y += (float)Random.NextDouble(-0.03, 0.03);
        }

        private SpawnPoint selectTargetSpawnPointFromList(IEnumerable<SpawnPoint> spawnPointList)
        {
            if (spawnPointList.Count() < 1) return null;
            float minDistSq = 99999;
            SpawnPoint result = null;
            foreach (SpawnPoint spawn in spawnPointList)
            {
                float newDistSq = Vector2.DistanceSquared(spawn.Position, ownTerritoriumMid);
                if (newDistSq < minDistSq)
                {
                    minDistSq = newDistSq;
                    result = spawn;
                }
            }

            return result;
        }

        private void MoveCursor(float frameTimeInterval)
        {
#if AI_DEBUG
            Vector2 cursorMove = targetPosition - particleAttractionPosition;
#else
            Vector2 cursorMove = targetPosition - cursorPosition;
#endif
            if (cursorMove.Length() < 0.02f) return;

            cursorMove /= cursorMove.Length();
            cursorMove *= frameTimeInterval * CURSOR_SPEED;

            float len = cursorMove.Length();
            if (len > 1.0f) cursorMove /= len;
#if AI_DEBUG
            particleAttractionPosition += (cursorMove * 0.65f);


            particleAttractionPosition.X = MathHelper.Clamp(particleAttractionPosition.X, 0.0f, Level.RELATIVE_MAX.X);
            particleAttractionPosition.Y = MathHelper.Clamp(particleAttractionPosition.Y, 0.0f, Level.RELATIVE_MAX.Y);
#else
            cursorPosition += (cursorMove * 0.65f);

            cursorPosition.X = MathHelper.Clamp(cursorPosition.X, 0.0f, Level.RELATIVE_MAX.X);
            cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, 0.0f, Level.RELATIVE_MAX.Y);
            particleAttractionPosition = CursorPosition;
#endif
        }
    }
}
