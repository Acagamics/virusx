﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ParticleStormControl
{
    public class Level
    {
        public List<MapObject> mapObjects = new List<MapObject>();

        private Texture2D pixelTexture;
        
        private Texture2D controlPadTexture;
        private Texture2D controlIcon;
        private const float controlPadFadeTime = 0.5f;

        private SpriteBatch spriteBatch;

        #region debuff ressources
        private SoundEffect debuffExplosionSound;
        private Texture2D debuffItemTexture;
        private Texture2D debuffExplosionTexture;
        #endregion

        #region dangerzone ressources
        private SoundEffect dangerZoneSound;
        private Texture2D dangerZoneItem;
        private Texture2D dangerZoneInnerTexture;
        private Texture2D dangerZoneOuterTexture;
        #endregion

        public static BlendState ShadowBlend = new BlendState
                                                   {
                                                       ColorSourceBlend = Blend.One,
                                                       ColorDestinationBlend = Blend.InverseSourceAlpha,
                                                       ColorBlendFunction = BlendFunction.Add
                                                   };

        #region field dimension
        public Vector2 FieldPixelSize
        {
            get { return new Vector2(fieldSize_pixel, fieldSize_pixel); }
        }
        private int fieldSize_pixel;


        public Vector2 FieldPixelOffset
        {
            get { return new Vector2(fieldOffsetX_pixel, fieldOffsetY_pixel); }
        }
        private int fieldOffsetX_pixel;
        private const int fieldOffsetY_pixel = 0;
        #endregion

        #region switch

        private SoundEffect switchSound;
        private bool switchCountdownActive = false;
        private float switchCountdownTimer;
        private const float switchCountdownLength = 6.0f;
        private SpriteFont fontCountdownLarge;

        #endregion

        private Stopwatch pickuptimer;

        private RenderTarget2D particleTexture;

        private BlendState ScreenBlend = new BlendState()
                                             {
                                                 ColorBlendFunction = BlendFunction.Add,
                                                 ColorSourceBlend = Blend.InverseDestinationColor,
                                                 ColorDestinationBlend = Blend.One,
                                                 AlphaBlendFunction = BlendFunction.Add,
                                                 AlphaSourceBlend = Blend.InverseDestinationAlpha,
                                                 AlphaDestinationBlend = Blend.One
                                             };

        private Random random = new Random();

        public Level(GraphicsDevice device, ContentManager content, int numPlayers)
        {
            pickuptimer = new Stopwatch();
            pickuptimer.Start();

            spriteBatch = new SpriteBatch(device);
            pixelTexture = content.Load<Texture2D>("pix");

            // pad
            controlPadTexture = content.Load<Texture2D>("netzdiagramm");
            controlIcon = content.Load<Texture2D>("cursor_net");

            // debuff
            debuffExplosionSound = content.Load<SoundEffect>("sound/explosion");
            debuffExplosionTexture = content.Load<Texture2D>("explosion");
            debuffItemTexture = content.Load<Texture2D>("debuff");

            // dangerzone
            dangerZoneSound = content.Load<SoundEffect>("sound/danger_zone");
            dangerZoneItem = content.Load<Texture2D>("buff");
            dangerZoneInnerTexture = content.Load<Texture2D>("danger_zone_inner");
            dangerZoneOuterTexture = content.Load<Texture2D>("danger_zone_outer");

            // switch
            switchSound = content.Load<SoundEffect>("sound/switch");
            fontCountdownLarge = content.Load<SpriteFont>("fontCountdown");

            Resize(device);
            CreateLevel(content, numPlayers);

            Texture2D crossHairTexture = content.Load<Texture2D>("basic_crosshair");
            for (int i = 0; i < numPlayers; ++i )
                mapObjects.Add(new Crosshair(i, crossHairTexture));
        }

        private void CreateLevel(ContentManager content, int numPlayers)
        {
            // needed ressources
            SoundEffect captureExplosion = content.Load<SoundEffect>("sound/captureExplosion");
            SoundEffect capture = content.Load<SoundEffect>("sound/capture");
            Texture2D captureGlow = content.Load<Texture2D>("capture_glow");
            Texture2D glowTexture = content.Load<Texture2D>("glow");
            Texture2D hqInner = content.Load<Texture2D>("unit_hq_inner");
            Texture2D hqOuter = content.Load<Texture2D>("unit_hq_outer");

            // how many?
            int pointcount = random.Next(3) + 3;


            // player starts
            List<MapObject> newCapturePoints = new List<MapObject>();



            newCapturePoints.Add(new SpawnPoint(new Vector2(0.1f, 0.9f), 1000.0f, 0, capture, captureExplosion,
                                                    glowTexture, captureGlow, hqInner, hqOuter));
            newCapturePoints.Add(new SpawnPoint(new Vector2(0.9f, 0.1f), 1000.0f, 1, capture, captureExplosion,
                                                    glowTexture, captureGlow, hqInner, hqOuter));
            if(numPlayers >= 3)
            {
                newCapturePoints.Add(new SpawnPoint(new Vector2(0.9f, 0.9f), 1000.0f, 2, capture, captureExplosion,
                                    glowTexture, captureGlow, hqInner, hqOuter));
            }
            if (numPlayers == 4)
            {
                newCapturePoints.Add(new SpawnPoint(new Vector2(0.1f, 0.1f), 1000.0f, 3, capture, captureExplosion,
                                    glowTexture, captureGlow, hqInner, hqOuter));
            }


            int tooCloseCounter = 0;
            for (int i = 0; i < pointcount; i++)
            {
                Vector2 randomposition = new Vector2((float) (((random.NextDouble()*0.8)/2) + 0.5),
                                                     (float) (random.NextDouble()*0.8 + 0.1));

                bool tooclose = false;
                foreach (SpawnPoint currenCP in newCapturePoints)
                {
                    if ((currenCP.Position - randomposition).Length() < (0.3f*(3.0f/(float) pointcount)))
                        tooclose = true;
                }

                if ((new Vector2(0.9f, 0.1f) - randomposition).Length() < (0.3f * (3.0f / (float)pointcount)))
                    tooclose = true;

                if (!tooclose)
                {
                    float capturesize = 100.0f + ((float)random.NextDouble() * 500);
                    newCapturePoints.Add(new SpawnPoint(randomposition, capturesize, -1, capture, captureExplosion, glowTexture, captureGlow, hqInner, hqOuter));
                    newCapturePoints.Add(new SpawnPoint(new Vector2(1.0f, 1.0f) - randomposition, capturesize, -1, capture, captureExplosion, glowTexture, captureGlow, hqInner, hqOuter));
                }
                else
                {
                    ++tooCloseCounter;
                    if (tooCloseCounter > 50)
                        break;
                    i--; // try again
                }
            }

            mapObjects.AddRange(newCapturePoints);
        }

        public Rectangle ComputePixelRect(Vector2 position, float size)
        {
            int rectSize = (int)(size * fieldSize_pixel);
            int rectx = (int)(position.X * FieldPixelSize.X + FieldPixelOffset.X);
            int recty = (int)(position.Y * FieldPixelSize.Y + FieldPixelOffset.Y);

            return new Rectangle(rectx, recty, rectSize, rectSize);
        }

        public Rectangle ComputePixelRect_Centered(Vector2 position, float size)
        {
            int rectSize = (int)(size * fieldSize_pixel);
            int halfSize = rectSize / 2;

            int rectx = (int)(position.X * FieldPixelSize.X + FieldPixelOffset.X);
            int recty = (int)(position.Y * FieldPixelSize.Y + FieldPixelOffset.Y);
            
            return new Rectangle(rectx - halfSize, recty - halfSize, rectSize, rectSize);
        }

        public void ApplyDamage(DamageMap damageMap, float timeInterval)
        {
            foreach (MapObject interest in mapObjects)
                interest.ApplyDamage(damageMap, timeInterval);
        }

        public void Update(float frameTimeSeconds, float totalTimeSeconds, Player[] players)
        {
            // update
            foreach (MapObject mapObject in mapObjects)
            {
                mapObject.Update(frameTimeSeconds, totalTimeSeconds);

                Crosshair crosshair = mapObject as Crosshair;
                if (crosshair != null)
                {
                    mapObject.Position = players[crosshair.PlayerIndex].CursorPosition;
                    mapObject.Alive = players[crosshair.PlayerIndex].Alive;
                }
            }

            // remove dead objects
            for (int i = 0; i < mapObjects.Count; ++i)
            {
                if (!mapObjects[i].Alive)
                {
                    mapObjects.RemoveAt(i);
                    --i;
                }
            }

            // random events
            if ((pickuptimer.Elapsed.TotalSeconds > 5) && (random.NextDouble() > 0.75))
            {
                // random position within a certain range
                Vector2 position = new Vector2((float)(random.NextDouble()) * 0.8f + 0.1f, (float)(random.NextDouble()) * 0.8f + 0.1f);

                double rand = random.NextDouble();
                if (rand > 0.6)
                    mapObjects.Add(new Debuff(position, debuffExplosionSound, debuffItemTexture, debuffExplosionTexture));
                else if ((rand < 0.6) && (rand > 0.2))
                    mapObjects.Add(new DangerZone(position, dangerZoneSound, dangerZoneItem, dangerZoneInnerTexture, dangerZoneOuterTexture));
                else if (rand < 0.065 && !switchCountdownActive)
                {
                    switchCountdownTimer = switchCountdownLength;
                    switchCountdownActive = true;
                }

                // restart timer
                pickuptimer.Reset();
                pickuptimer.Start();
            }


            
        }

        public void UpdateSwitching(float frameTimeSeconds, Player[] players)
        {
            switchCountdownTimer -= frameTimeSeconds;
            if (switchCountdownActive && switchCountdownTimer < 0.0f)
            {
                switchSound.Play();

                int[] playerIndices = { 0, 1, 2, 3 };

                // count alive players, create reduced playerlist
                int alivePlayerCount = players.Length;
                foreach (Player player in players)
                    alivePlayerCount -= player.Alive ? 0 : 1;
                Player[] reducedPlayerList = new Player[alivePlayerCount];
                int index = 0;
                foreach (Player player in players)
                {
                    if (player.Alive)
                    {
                        reducedPlayerList[index] = player;
                        index++;
                    }
                }


                if (reducedPlayerList.Length == 2)
                {
                    Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[1]);
                    SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[1].Index);
                }
                else if (reducedPlayerList.Length == 3)
                {
                    bool rotateLeft = random.Next(2) == 0;
                    if (rotateLeft)
                    {
                        Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[1]);
                        SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[1].Index);
                        Player.SwitchPlayer(reducedPlayerList[1], reducedPlayerList[2]);
                        SwapInts(playerIndices, reducedPlayerList[1].Index, reducedPlayerList[2].Index);
                    }
                    else
                    {
                        Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[1]);
                        SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[1].Index);
                        Player.SwitchPlayer(reducedPlayerList[0], reducedPlayerList[2]);
                        SwapInts(playerIndices, reducedPlayerList[0].Index, reducedPlayerList[2].Index);
                    }
                }
                else if (reducedPlayerList.Length == 4)
                {
                    int first = random.Next(0, 4);
                    int second = (first + random.Next(1, 4)) % 4;
                    Player.SwitchPlayer(players[first], players[second]);
                    SwapInts(playerIndices, reducedPlayerList[first].Index, reducedPlayerList[second].Index);

                    int third = -1;
                    int fourth = -1;
                    for (int i = 0; i < 4; ++i)
                    {
                        if (first != i && second != i)
                        {
                            if (third == -1)
                                third = i;
                            else
                                fourth = i;
                        }
                    }
                    Player.SwitchPlayer(players[third], players[fourth]);
                    SwapInts(playerIndices, reducedPlayerList[third].Index, reducedPlayerList[fourth].Index);

                    playerIndices[first] = second;
                    playerIndices[second] = first;
                    playerIndices[third] = fourth;
                    playerIndices[fourth] = third;
                }

                foreach (MapObject mapObject in mapObjects)
                    mapObject.SwitchPlayer(playerIndices);

                switchCountdownActive = false;
            }    
        }

        static void SwapInts(int[] array, int position1, int position2)
        {
            int temp = array[position1];
            array[position1] = array[position2];
            array[position2] = temp;
        }

        public void BeginDrawInternParticleTarget(GraphicsDevice device)
        {
            // particles to offscreen target
            device.SetRenderTarget(particleTexture);
            device.Clear(ClearOptions.Target, new Color(0, 0, 0, 0), 0, 0);
            spriteBatch.GraphicsDevice.BlendState = ScreenBlend;
        }

        public void EndDrawInternParticleTarget(GraphicsDevice device)
        {
            device.SetRenderTarget(null);
        }

        public void Draw(float totalTimeSeconds, GraphicsDevice device, Player[] players)
        {
            DrawMap(device);

            // screenblend stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, ScreenBlend);
            foreach (MapObject mapObject in mapObjects)
            {
                if (mapObject.Alive)
                    mapObject.Draw_ScreenBlended(spriteBatch, this, totalTimeSeconds);
            }
            spriteBatch.End();


            DrawParticles(device);

            // alphablended stuff
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            DrawCountdown(device);
            foreach (MapObject mapObject in mapObjects)
            {
                if (mapObject.Alive)
                    mapObject.Draw_AlphaBlended(spriteBatch, this, totalTimeSeconds);
            }
            DrawControls(players, device);
            //DrawBar(players, device);

            spriteBatch.End();
        }

        private void DrawParticles(GraphicsDevice device)
        {
          /*  spriteBatch.Begin(SpriteSortMode.Deferred, ShadowBlend, SamplerState.LinearClamp, DepthStencilState.None,
                              RasterizerState.CullNone);
            spriteBatch.Draw(particleTexture,
                             new Rectangle((int)FieldPixelOffset.X - 1, (int)FieldPixelOffset.Y - 1, 2 + particleTexture.Width, 2 + particleTexture.Height), new Color(255, 255, 255, 150));
            spriteBatch.End();
            */
            spriteBatch.Begin(SpriteSortMode.Deferred, ShadowBlend, SamplerState.LinearClamp,
                              DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(particleTexture,
                             new Rectangle((int)FieldPixelOffset.X, (int)FieldPixelOffset.Y, particleTexture.Width, particleTexture.Height),
                             Color.White);
            spriteBatch.End(); 
        }

        private void DrawMap(GraphicsDevice device)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap,
                              DepthStencilState.Default, RasterizerState.CullNone);

            // lines
            spriteBatch.Draw(pixelTexture, new Rectangle(fieldOffsetX_pixel - 4, fieldOffsetY_pixel, 1, fieldSize_pixel), Color.Black);
            spriteBatch.Draw(pixelTexture, new Rectangle(fieldOffsetX_pixel - 2, fieldOffsetY_pixel, 2, fieldSize_pixel), Color.Black);
            spriteBatch.Draw(pixelTexture, new Rectangle(fieldOffsetX_pixel + fieldSize_pixel, fieldOffsetY_pixel, 2, fieldSize_pixel),
                             Color.Black);
            spriteBatch.Draw(pixelTexture, new Rectangle(fieldOffsetX_pixel + fieldSize_pixel + 3, fieldOffsetY_pixel, 1, fieldSize_pixel),
                             Color.Black);
            spriteBatch.End();
        }

        private void DrawControls(Player[] players, GraphicsDevice device)
        {
            int sizePad = fieldSize_pixel/4;
            int sizeIcon = sizePad/4;

            int halfViewPortHeight = (int) (device.Viewport.Height*0.5f);
            int leftPadsX = device.Viewport.Width - sizePad - 20;
            int halfSize = sizePad/2;

            if (players.Length == 2)
            {
                DrawParticleControlPad(players[0], 20,        halfViewPortHeight - halfSize, sizePad, sizeIcon);
                DrawParticleControlPad(players[1], leftPadsX, halfViewPortHeight - halfSize, sizePad, sizeIcon);
            }
            else if (players.Length == 3)
            {
                DrawParticleControlPad(players[0], 20,        halfViewPortHeight - halfSize, sizePad, sizeIcon);
                DrawParticleControlPad(players[1], leftPadsX, halfViewPortHeight - sizePad - 10, sizePad, sizeIcon);
                DrawParticleControlPad(players[2], leftPadsX, halfViewPortHeight + 10, sizePad, sizeIcon);
            }
            else
            {
                DrawParticleControlPad(players[0], 20,        halfViewPortHeight + 10,           sizePad, sizeIcon);
                DrawParticleControlPad(players[1], leftPadsX, halfViewPortHeight - sizePad - 10, sizePad, sizeIcon);
                DrawParticleControlPad(players[2], leftPadsX, halfViewPortHeight + 10,           sizePad, sizeIcon);
                DrawParticleControlPad(players[3], 20,        halfViewPortHeight - sizePad - 10, sizePad, sizeIcon);
            }
        }

        private void DrawParticleControlPad(Player player, int positionX, int positionY, int sizePad, int sizeIcon)
        {
            if (player.TimeDead < controlPadFadeTime)
            {
                Rectangle rect = new Rectangle(positionX, positionY, sizePad, sizePad);

                float alpha = 1.0f - player.TimeDead / controlPadFadeTime;
                Color color = player.Color * alpha;

                spriteBatch.Draw(controlPadTexture, rect, color);

                int padMidX = positionX + sizePad / 2;
                int padMidY = positionY + sizePad / 2; 

                rect = new Rectangle(
                    (int)(padMidX - 0.5f * sizeIcon - player.Disciplin_speed * sizePad * 0.5f * 0.75f),
                    (int)(padMidY - 0.5f * sizeIcon + player.Mass_health * sizePad * 0.5f * 0.75f),
                    sizeIcon, sizeIcon);

                spriteBatch.Draw(controlIcon, rect, color);

                // resting time alive
                float remainingTimeAlive = player.RemainingTimeAlive;
                if(remainingTimeAlive < 10.0f)
                {
                    string countdown = ((int)remainingTimeAlive).ToString();
                    Vector2 size = fontCountdownLarge.MeasureString(countdown) * 0.25f;
                    spriteBatch.DrawString(fontCountdownLarge, countdown,
                                           new Vector2(padMidX - size.X / 2, padMidY - size.Y / 2),
                                           new Color(0, 0, 0, 160), 0.0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0.0f);
                }
            }
        }

      /*  private void DrawBar(Player[] players, GraphicsDevice device)
        {
            float width = players[0].OverallHealth/
                          Math.Max(1, players[0].OverallHealth + players[1].OverallHealth);
            spriteBatch.Draw(pixelTexture, new Rectangle(0, 0, (int)(width * device.Viewport.Width), 20), Color.Red);
            spriteBatch.Draw(pixelTexture,
                             new Rectangle((int) (width*device.Viewport.Width), 0,
                                           (int)
                                           (device.Viewport.Width -
                                            width*device.Viewport.Width), 20), Color.Blue);
        } */

        public void DrawCountdown(GraphicsDevice device)
        {
            if (switchCountdownActive)
            {
                string text = ((int) (switchCountdownTimer + 1)).ToString();
                spriteBatch.DrawString(fontCountdownLarge, text,
                                       new Vector2(
                                           (device.Viewport.Width - fontCountdownLarge.MeasureString(text).X)*
                                           0.5f,
                                           (device.Viewport.Height - fontCountdownLarge.MeasureString(text).Y)*
                                           0.5f + 40), Color.FromNonPremultiplied(80, 80, 80, 80));
            }
        }

        public void Resize(GraphicsDevice device)
        {
            fieldSize_pixel = device.Viewport.Height - fieldOffsetY_pixel;
            fieldOffsetX_pixel = (int)((device.Viewport.Width - device.Viewport.Height)*0.5f);

            CreateParticleTarget(device);
        }

        private void CreateParticleTarget(GraphicsDevice device)
        {
            if (particleTexture != null)
                particleTexture.Dispose();

            particleTexture = new RenderTarget2D(device, (int)(FieldPixelSize.X),
                                                         (int)(FieldPixelSize.Y),
                                                    false, SurfaceFormat.Color, DepthFormat.None, 0,
                                                    RenderTargetUsage.PreserveContents);
        }

        public void DrawToDamageMap(SpriteBatch damageSpriteBatch)
        {
            foreach (MapObject point in mapObjects)
                point.DrawToDamageMap(damageSpriteBatch);
        }
    }
}
