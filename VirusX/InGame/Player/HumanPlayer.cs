using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirusX
{
    class HumanPlayer : Player
    {
        #region Control

        public InputManager.ControlType Controls
        {
            get { return Settings.Instance.GetPlayer(Index).ControlType;}// InputManager.Instance.getControlType(playerIndex); }
            set { Settings.Instance.GetPlayer(Index).ControlType = value; }// InputManager.Instance.setControlType(playerIndex, value); }
        }

        #endregion

        public HumanPlayer(int playerIndex, VirusSwarm.VirusType virusIndex, int colorIndex, Teams team, InGame.GameMode gameMode, GraphicsDevice device, ContentManager content, Texture2D noiseTexture,
                                InputManager.ControlType controlType) :
            base(playerIndex, virusIndex, colorIndex, team, gameMode, device, content, noiseTexture)
        {
            Controls = controlType;
        }

        override public void UserControl(float frameTimeInterval, Level level)
        {
            Vector2 cursorMove = InputManager.Instance.GetMovement(playerIndex);
            cursorMove *= frameTimeInterval * CURSOR_SPEED;

            float len = cursorMove.Length();
            if (len > 1.0f) cursorMove /= len;
            cursorPosition += (cursorMove * 0.65f);

            cursorPosition.X = MathHelper.Clamp(cursorPosition.X, 0.0f, Level.RELATIVE_MAX.X);
            cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, 0.0f, Level.RELATIVE_MAX.Y);

            // hold move
            if (Alive && !InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.HOLD, playerIndex, true))
                particleAttractionPosition = cursorPosition;

            // action
            if (Alive && InputManager.Instance.SpecificActionButtonPressed(InputManager.ControlActions.ACTION, playerIndex) && ItemSlot != Item.ItemType.NONE)
            {
                level.PlayerUseItem(this);
                ItemSlot = Item.ItemType.NONE;
            }
        }
    }
}
