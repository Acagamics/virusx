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
            base(position, -1, 0.1f, 10.0f, 3)
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
            if(!Alive)
                base.Update(frameTimeSeconds, totalTimeSeconds);
        }

        protected override void OnPossessingChanged()
        {
            // TODO insert gather sound here 

            Alive = false;
        }

        public override void Draw_AlphaBlended(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Level level, float totalTimeSeconds)
        {
            spriteBatch.Draw(itemTexture, level.ComputePixelRect(Position, Size), null, Color.Lerp(Color.White, Color.Black, PossessingPercentage),
                                   totalTimeSeconds, new Vector2(itemTexture.Width, itemTexture.Height) / 2, SpriteEffects.None, 1.0f);
        }
    }
}
