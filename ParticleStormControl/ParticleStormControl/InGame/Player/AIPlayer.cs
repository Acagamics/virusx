﻿#define AI_DEBUG

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

        class TargetSelector
        {
            SpawnPoint lastTarget = null;
            float lastTargetPossessingPercentage = 0f;
            float timeOnTarget;
            float maxTimeOnTarget = 5f;

            SpawnPoint ignoreSpawnPoint = null;
            float maxIgnoreTime = 2.5f;
            float ignoreTime = 0f;

            public Vector2 LastTargetPosition { get { return lastTarget != null ? lastTarget.Position : Vector2.Zero; } }
            public Vector2 IgnorePosition { get { return ignoreSpawnPoint != null ? ignoreSpawnPoint.Position : Vector2.Zero; } }
            public SpawnPoint TargetSpawnPoint { get { return lastTarget; } }

            public TargetSelector()
            {
            }

            public Vector2 Update(Level level, Player player, float frameTimeInterval)
            {
                if (ignoreSpawnPoint != null)
                {
                    ignoreTime += frameTimeInterval;
                    if (ignoreTime >= maxIgnoreTime)
                    {
                        ignoreSpawnPoint = null;
                    }
                }
                Vector2 newTarget = player.ParticleAttractionPosition;

                SpawnPoint possibleTarget = SelectTarget(level, player);
                if (possibleTarget == null)
                {
                    lastTarget = null;
                    return newTarget;
                }

                if (possibleTarget == lastTarget)
                {
                    timeOnTarget += frameTimeInterval;
                    if (timeOnTarget >= maxTimeOnTarget)
                    {
                        if (ignoreSpawnPoint == null)
                        {
                            ignoreSpawnPoint = possibleTarget;
                            ignoreTime = 0f;
                        }
                        timeOnTarget = 0f;

                        possibleTarget = SelectTarget(level, player);

                        if (possibleTarget == lastTarget)
                            Console.WriteLine("The Same!!!");
                        if (possibleTarget == null)
                            possibleTarget = lastTarget;
                    }
                }
                else timeOnTarget = 0f;

                newTarget = possibleTarget.Position;
                lastTarget = possibleTarget;
                lastTargetPossessingPercentage = lastTarget.PossessingPercentage;

                return newTarget;
            }

            private SpawnPoint SelectTarget(Level level, Player player)
            {
                var ownSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer == player.Index);
                int numberOfOwnSPs = ownSpawnPoints.Count();

                var noOwnerSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer == -1);
                int numberOfNoOwnerSPs = noOwnerSpawnPoints.Count();

                var otherSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer != player.Index && x.PossessingPlayer != -1);
                int numberOfOtherSPs = otherSpawnPoints.Count();

                if (ignoreSpawnPoint != null)
                {
                    noOwnerSpawnPoints = noOwnerSpawnPoints.Where(x =>  x != ignoreSpawnPoint);
                    numberOfNoOwnerSPs = noOwnerSpawnPoints.Count();
                    otherSpawnPoints = otherSpawnPoints.Where(x =>  x != ignoreSpawnPoint);
                    numberOfOtherSPs = otherSpawnPoints.Count();
                }

                Vector2 ownTerritoriumMid = player.ParticleAttractionPosition;
                if (numberOfOwnSPs > 0)
                {
                    ownTerritoriumMid.X = ownSpawnPoints.Average(x => x.Position.X);
                    ownTerritoriumMid.Y = ownSpawnPoints.Average(x => x.Position.Y);
                }
                Vector2 ownTerritoriumMidWithPAP = (ownTerritoriumMid + player.ParticleAttractionPosition) / 2f;

                if (numberOfNoOwnerSPs > 0)
                    noOwnerSpawnPoints = noOwnerSpawnPoints.OrderBy(x => Vector2.DistanceSquared(x.Position, ownTerritoriumMidWithPAP));
                if (numberOfOtherSPs > 0)
                    otherSpawnPoints = otherSpawnPoints.OrderBy(x => Vector2.DistanceSquared(x.Position, ownTerritoriumMidWithPAP));

                var withoutCaptuerer = noOwnerSpawnPoints.Where(x => x.CapturingPlayer == player.Index || x.CapturingPlayer == -1)
                    .OrderBy(x => Vector2.DistanceSquared(x.Position, ownTerritoriumMidWithPAP));

                SpawnPoint possibleTarget = null;
                if (withoutCaptuerer.Count() > 0)
                {
                    if (getMinDist(withoutCaptuerer, ownTerritoriumMidWithPAP) <= getMinDist(noOwnerSpawnPoints, ownTerritoriumMidWithPAP) * 2f)
                    {
                        possibleTarget = withoutCaptuerer.First();
                    }
                    else
                    {
                        possibleTarget = noOwnerSpawnPoints.First();
                    }
                }

                if (possibleTarget == null && numberOfNoOwnerSPs > 0)
                {
                    possibleTarget = noOwnerSpawnPoints.First();
                }

                if (possibleTarget == null && numberOfOtherSPs > 0)
                {
                    possibleTarget = otherSpawnPoints.First();
                }
                else if (numberOfOtherSPs > 0)
                {
                    if (getMinDist(otherSpawnPoints, ownTerritoriumMidWithPAP) <= Vector2.DistanceSquared(possibleTarget.Position, ownTerritoriumMidWithPAP) * 0.8f)
                        possibleTarget = otherSpawnPoints.First();
                }

                return possibleTarget;
            }

            private float getMinDist(IEnumerable<SpawnPoint> spawnPoints, Vector2 toPoint)
            {
                return spawnPoints.Min(x => Vector2.DistanceSquared(x.Position, toPoint));
            }

            private float getMaxDist(IEnumerable<SpawnPoint> spawnPoints, Vector2 toPoint)
            {
                return spawnPoints.Max(x => Vector2.DistanceSquared(x.Position, toPoint));
            }
        }

        #region control variables

        private Vector2 targetPosition = Vector2.Zero;
        Vector2 ownTerritoriumMid = Vector2.Zero;
        private SpawnPoint targetSpawnPoint = null;

        private float maxRndMovementTime = 5f;
        private float currentRndMovmentTime = 2.5f;
        private bool TargetRndMoveTimeReached { get { return currentRndMovmentTime >= maxRndMovementTime; } }

        private TargetSelector targetSelector = new TargetSelector();

        #endregion

        public AIPlayer(int playerIndex, int virusIndex, int colorIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture) :
            base(playerIndex, virusIndex, colorIndex, device, content, noiseTexture)
        {
            targetPosition = particleAttractionPosition = cursorPosition = cursorStartPositions[playerIndex];
        }

        public override void UserControl(float frameTimeInterval, Level level)
        {
            CheckItems(level);

            targetPosition = targetSelector.Update(level, this, frameTimeInterval);
            targetSpawnPoint = targetSelector.TargetSpawnPoint;

            if (TargetRndMoveTimeReached)
            {
                maxRndMovementTime = (float)Random.NextDouble(1.0, 5.0);
                currentRndMovmentTime = 0f;
                targetPosition.X += (float)Random.NextDouble(-0.03, 0.03);
                targetPosition.Y += (float)Random.NextDouble(-0.03, 0.03);
            }
            else currentRndMovmentTime += frameTimeInterval;

            MoveCursor(frameTimeInterval);

#if AI_DEBUG
            //ownTerritoriumMid = (ownTerritoriumMid + particleAttractionPosition) / 2f;
            //cursorPosition = ownTerritoriumMid;
            cursorPosition = targetSelector.IgnorePosition;
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
