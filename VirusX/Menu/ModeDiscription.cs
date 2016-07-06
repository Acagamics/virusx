using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using VirusX;


namespace VirusX.Menu
{
    class ModeDiscription : MenuPage
    {
        ModeDiscription(Menu menu, global::VirusX.InGame.GameMode gameMode) 
            : base(menu)
        {
            switch (gameMode)
            {
                case global::VirusX.InGame.GameMode.ARCADE:
                    break;

                default:
                    break;
            }
        }

        void ArcadeMode()
        {
            InterfaceButton goal;
            Interface.Add(new InterfaceButton("Goal",new Vector2(50,50),true));

            goal = new InterfaceButton("Survive as long as possible", new Vector2(50, 120));

            Interface.Add(goal);

            Interface.Add(new InterfaceButton("Got it!", new Vector2(50, 400)));
        }
    }
}
