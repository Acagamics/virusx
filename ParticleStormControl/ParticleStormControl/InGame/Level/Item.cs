using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParticleStormControl
{
    public class Item : CapturableObject
    {
        public enum ItemType
        {
            DANGER_ZONE,
            MUTATION,
            WIPEOUT,

            NONE
        };
        public ItemType Type { get; private set; }

        private Texture2D itemTexture;

        public Item(Vector2 position, ItemType type, ContentManager content) :
            base(position, -1, 0.01f, 15.0f, 3)
        {
            this.Type = type;
            switch(Type)
            {
                case ItemType.DANGER_ZONE:
                    itemTexture = content.Load<Texture2D>("buff");
                    break;
                default:
                    break;
            }
        }

        public override void Update(float frameTimeSeconds, float totalTimeSeconds)
        {
            base.Update(frameTimeSeconds, totalTimeSeconds);

            // gathered?
            if (PossessingPercentage == 1.0f && PossessingPlayer != -1)
                Alive = false;
        }

        protected override void OnPossessingChanged()
        {
            // TODO insert gather sound here 

            // doesn't work because Level can reject Alive=false
            // apparently a bad solution, but otherwise this item had to know about the player :/
        }

        public override void Draw_AlphaBlended(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            spriteBatch.Draw(itemTexture, level.ComputePixelRect(Position, Size), null, ComputeColor(),
                                   totalTimeSeconds, new Vector2(itemTexture.Width, itemTexture.Height) / 2, SpriteEffects.None, 1.0f);
        }
    }
}
