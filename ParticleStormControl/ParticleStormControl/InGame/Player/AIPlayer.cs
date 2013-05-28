//#define AI_DEBUG

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirusX
{
    class AIPlayer : Player
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
            
            // items
            bool ignoreItem = false;
            float timeOnItem = 0f;
            float maxTimeOnItem = 2.5f;
            float ignoreItemTime = 0f;
            float maxIgnoreItemTime = 4f;

            public Vector2 LastTargetPosition { get { return lastTarget != null ? lastTarget.Position : Vector2.Zero; } }
            public Vector2 IgnorePosition { get { return ignoreSpawnPoint != null ? ignoreSpawnPoint.Position : Vector2.Zero; } }
            public SpawnPoint TargetSpawnPoint { get { return lastTarget; } }

            Vector2 ownTerritoriumMid;

            /// <summary>
            /// game mode influences behaviour
            /// </summary>
            readonly InGame.GameMode gameMode;

            public TargetSelector(InGame.GameMode gameMode)
            {
                this.gameMode = gameMode;
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
                            ignoreTime = 0f + (float)Random.NextDouble(0.5) + 0.25f;
                        }
                        timeOnTarget -= maxTimeOnTarget;

                        possibleTarget = SelectTarget(level, player);

                        if (possibleTarget == lastTarget)
                            Console.WriteLine("The Same!!!");
                        if (possibleTarget == null)
                            possibleTarget = lastTarget;
                    }
                }
                else timeOnTarget = 0f + (float)Random.NextDouble(0.5) + 0.25f;

                Item possibleItem = SelectItem(level, player);

                bool selectSpawnPoint = true;
                float influenceCircle = 0.2f;
                if (!ignoreItem && possibleItem != null && player.ItemSlot == Item.ItemType.NONE)
                {
                    if (possibleTarget.CapturingPlayer == player.Index && possibleTarget.PossessingPercentage > 0.75f)
                    {
                        selectSpawnPoint = true;
                    }
                    else
                    {
                        float distSq = Vector2.DistanceSquared(possibleItem.Position, player.ParticleAttractionPosition);
                        if (influenceCircle >= distSq)
                        {
                            newTarget = possibleItem.Position;
                            selectSpawnPoint = false;
                        }
                        timeOnItem += frameTimeInterval;
                        if (timeOnItem >= maxTimeOnItem)
                        {
                            ignoreItem = true;
                            ignoreItemTime = 0f - (float)Random.NextDouble(0.2) - 0.1f;
                            selectSpawnPoint = true;
                        }
                    }
                        
                }
                if (selectSpawnPoint)
                {
                    newTarget = possibleTarget.Position;
                    lastTarget = possibleTarget;
                    lastTargetPossessingPercentage = lastTarget.PossessingPercentage;
                }

                if (ignoreItem)
                {
                    ignoreItemTime += frameTimeInterval;
                    if (ignoreItemTime >= maxIgnoreItemTime)
                    {
                        timeOnItem = 0f - (float)Random.NextDouble(0.3) - 0.1f;
                        ignoreItem = false;
                    }
                }
                return newTarget;
            }

            private SpawnPoint SelectTarget(Level level, Player player)
            {
                var ownSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer == player.Index);
                int numberOfOwnSPs = ownSpawnPoints.Count();

                var noOwnerSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer == -1);
                int numberOfNoOwnerSPs = noOwnerSpawnPoints.Count();

                var otherSpawnPoints = level.SpawnPoints.Where(x => x.PossessingPlayer != player.Index && x.PossessingPlayer != -1);
                if (gameMode == InGame.GameMode.CAPTURE_THE_CELL || gameMode == InGame.GameMode.LEFT_VS_RIGHT)
                    otherSpawnPoints = otherSpawnPoints.Where(x => Settings.Instance.GetPlayer(x.PossessingPlayer).Team != player.Team);
                otherSpawnPoints = otherSpawnPoints.Where(x => x.Captureable == true);
                int numberOfOtherSPs = otherSpawnPoints.Count();

                if (ignoreSpawnPoint != null)
                {
                    noOwnerSpawnPoints = noOwnerSpawnPoints.Where(x =>  x != ignoreSpawnPoint);
                    numberOfNoOwnerSPs = noOwnerSpawnPoints.Count();
                    otherSpawnPoints = otherSpawnPoints.Where(x =>  x != ignoreSpawnPoint);
                    numberOfOtherSPs = otherSpawnPoints.Count();
                }

                ownTerritoriumMid = player.ParticleAttractionPosition;
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

            private Item SelectItem(Level level, Player player)
            {
                var items = level.Items;

                if (items.Count() == 0) return null;

                Item possibleTarget = null;

                var distanceList = items.OrderBy(x => Vector2.DistanceSquared(player.CursorPosition, x.Position));

                possibleTarget = distanceList.First() as Item;
                if (player.PossessingSpawnPoints <= 3)
                {
                    var switchList = distanceList.Where(x => (x as Item).Type == Item.ItemType.MUTATION);
                    if (switchList.Count() > 0)
                        possibleTarget = switchList.First() as Item;
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

        private readonly TargetSelector targetSelector;

        /// <summary>
        /// let the ai player sleep a bit so that it is not as responsive as before ;)
        /// It is the minimum amount of time in seconds the ai player will sleep
        /// </summary>
        private float minSleepTime = 0.05f;
        /// <summary>
        /// let the ai player sleep a bit so that it is not as responsive as before ;)
        /// It is the maximum amount of time in seconds the ai player will sleep
        /// </summary>
        private float maxSleepTime = 0.15f;
        private float currentSleepTime = 0.1f;
        #endregion

        public AIPlayer(int playerIndex, VirusSwarm.VirusType virusIndex, int colorIndex, Teams team, InGame.GameMode gameMode, GraphicsDevice device, ContentManager content, Texture2D noiseTexture) :
            base(playerIndex, virusIndex, colorIndex, team, gameMode, device, content, noiseTexture)
        {
            targetSelector = new TargetSelector(gameMode);
            targetPosition = particleAttractionPosition = cursorPosition = cursorStartPositions[playerIndex];
        }

        public override void UserControl(float frameTimeInterval, Level level)
        {
            /*if (currentSleepTime > 0f)
            {
                currentSleepTime -= frameTimeInterval;
            }
            else
            {
                currentSleepTime = (float)Random.NextDouble(minSleepTime, maxSleepTime);
            */
                targetPosition = targetSelector.Update(level, this, frameTimeInterval);
                targetSpawnPoint = targetSelector.TargetSpawnPoint;

                CheckItems(level);
            /*}*/

            

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
                    if (step > 0)
                    {
                        if (level.GameStatistics.getDominationInStep(Index, step) < 0.20f)
                        {
                            UseItem(level);
                        }
                    }
                    break;
                case Item.ItemType.WIPEOUT:
                    if (step > 0)
                    {
                        uint overall = level.GameStatistics.getParticlesInStep(step);
                        if ((float)NumParticlesAlive / overall < 0.25f)
                        {
                            UseItem(level);
                        }
                        for (int i = 0; i < level.GameStatistics.PlayerCount; ++i)
                        {
                            if (i != Index)
                                if (level.GameStatistics.getPossessingSpawnPointsInStep(i, step) == 0)
                                {
                                    UseItem(level);
                                }
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
