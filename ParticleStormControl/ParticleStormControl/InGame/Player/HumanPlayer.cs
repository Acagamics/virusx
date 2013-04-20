using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ParticleStormControl
{
    class HumanPlayer : Player
    {
        #region Control

        public InputManager.ControlType Controls
        {
            get { return InputManager.Instance.getControlType(playerIndex); }
            set { InputManager.Instance.setControlType(playerIndex, value); }
        }

        #endregion

        public HumanPlayer(int playerIndex, int virusIndex, int colorIndex, GraphicsDevice device, ContentManager content, Texture2D noiseTexture,
                                InputManager.ControlType controlType) :
            base(playerIndex, virusIndex, colorIndex, device, content, noiseTexture)
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
            /*      
      #if DEBUG
                  // save particle textures on pressing space
                  if (InputManager.Instance.PressedButton(Keys.Tab))
                  {
                      using (var file = new System.IO.FileStream("position target " + playerIndex + ".png", System.IO.FileMode.Create))
                          positionTargets[currentTargetIndex].SaveAsPng(file, maxParticlesSqrt, maxParticlesSqrt);
                      using (var file = new System.IO.FileStream("info target " + playerIndex + ".png", System.IO.FileMode.Create))
                          infoTargets[currentTargetIndex].SaveAsPng(file, maxParticlesSqrt, maxParticlesSqrt);
                      using (var file = new System.IO.FileStream("movement target " + playerIndex + ".png", System.IO.FileMode.Create))
                          movementTexture[currentTargetIndex].SaveAsPng(file, maxParticlesSqrt, maxParticlesSqrt);
                  }
      #endif     */
        }
    }
}
