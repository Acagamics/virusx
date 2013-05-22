using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using VirusX.Menu;

namespace VirusX
{
    class Tutorial
    {
        internal List<InterfaceElement> Interface = new List<InterfaceElement>();
        ParticleStormControl game;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menu"></param>
        public Tutorial(ParticleStormControl game)
        {
            this.game = game;
        }

        /// <summary>
        /// Loads content
        /// </summary>
        /// <param name="content"></param>
        public virtual void LoadContent(ContentManager content)
        {
            foreach (InterfaceElement element in Interface)
            {
                element.LoadContent(content);
            }
        }
        
        /// <summary>
        /// Updates the interface and all specific content of the page
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            foreach (InterfaceElement element in Interface)
            {
                element.Update(gameTime);
            }
        }
        
        /// <summary>
        /// Draws the interface and all specific content of the page
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            foreach (InterfaceElement element in Interface)
            {
                if (element.Visible())
                    element.Draw(spriteBatch, gameTime);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// callback for menu page changed
        /// </summary>
        public void OnMenuPageChanged(Menu.Menu.Page newPage, Menu.Menu.Page oldPage)
        {
            Interface.Clear();
            switch (newPage)
            {
                case VirusX.Menu.Menu.Page.MAINMENU:
                    //if(Settings.Instance.FirstStart)
                        Interface.Add(new InterfaceTooltip(() => { return "Tutorial"; }, () => { return "we recommend to play the tutorial mode..."; }, new Vector2(550, 350), () => { return ((Menu.MainMenu)game.Menu.Pages[(int)newPage]).SubmenuVisible; }, 500, InterfaceTooltip.ArrowPosition.BOTTOM));
                    break;
                case VirusX.Menu.Menu.Page.NEWGAME:
                    break;
                case VirusX.Menu.Menu.Page.INGAME:
                    if (Settings.Instance.GameMode == InGame.GameMode.TUTORIAL)
                    {
                        Interface.Add(new InterfaceTooltip(() => { return "Your base"; }, () => { return "this is your start base. you better not lose it..."; }, game.InGame.Level.ComputePixelPosition(game.InGame.Level.SpawnPoints[0].Position) + new Vector2(50, 0), () => true, 500, InterfaceTooltip.ArrowPosition.LEFT));
                        Interface.Add(new InterfaceTooltip(() => { return "Enemy base"; }, () => { return "try to own this one"; }, game.InGame.Level.ComputePixelPosition(game.InGame.Level.SpawnPoints[1].Position) - new Vector2(50, 0), () => true, 500, InterfaceTooltip.ArrowPosition.RIGHT));
                    }    
                break;
            }
            LoadContent(game.Content);
        }
    }
}
