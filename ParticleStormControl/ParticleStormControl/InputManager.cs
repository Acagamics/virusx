using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ParticleStormControl
{
    class InputManager
    {
        #region singleton
        private static readonly InputManager instance = new InputManager();
        public static InputManager Instance { get { return instance; } }
        private InputManager() { }
        #endregion

        #region state
        private GamePadState[] currentGamePadStates = new GamePadState[4];
        private GamePadState[] oldGamePadStates = new GamePadState[4];
        private KeyboardState currentKeyboardState = new KeyboardState();
        private KeyboardState oldKeyboardState = new KeyboardState();
        
        private Vector2[] rightStickMovement = new Vector2[4];
        private Vector2[] leftStickMovement = new Vector2[4];

        private bool[] waitingForReconnect = { false, false, false, false };
        #endregion

        /// <summary>
        /// is the game waiting for any reconnect
        /// </summary>
        /// <returns>true if waiting</returns>
        public bool IsWaitingForReconnect()
        {
            return waitingForReconnect.Any(x => x);
        }

        /// <summary>
        /// is the game waiting for a specific controller to reconnect?
        /// </summary>
        /// <param name="controller">controller to wait for</param>
        /// <returns>true if waiting</returns>
        public bool IsWaitingForReconnect(int controller)
        {
#if DEBUG
            if (controller > 4 || controller < 0)
                throw new Exception("There are only controller 0-3 - can't access controller " + controller);
#endif
            return waitingForReconnect[controller];
        }

        /// <summary>
        /// clears the waiting-for-reconnect table
        /// </summary>
        public void ResetWaitingForReconnect()
        {
            Array.Clear(waitingForReconnect, 0, 4);
        }

        public void Update(float timeSinceLastCall)
        {
            // keyboard
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            
            // mouse
            // ...

            // controller
            for (int i = 0; i < 4; ++i)
            {
                oldGamePadStates[i] = currentGamePadStates[i];
                currentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                // sticks
                leftStickMovement[i].X = currentGamePadStates[i].ThumbSticks.Left.X;
                leftStickMovement[i].Y = currentGamePadStates[i].ThumbSticks.Left.Y;
                rightStickMovement[i].X = currentGamePadStates[i].ThumbSticks.Right.X;
                rightStickMovement[i].Y = currentGamePadStates[i].ThumbSticks.Right.Y;

                // disconnect?
                if (waitingForReconnect[i] == false)
                {
                    // must change!
                    if (!currentGamePadStates[i].IsConnected && oldGamePadStates[i].IsConnected)
                        waitingForReconnect[i] = true;
                }
                else
                    waitingForReconnect[i] = currentGamePadStates[i].IsConnected;
            }


            thumbStickButtonDownTimer += timeSinceLastCall;
            thumbStickButtonUpTimer += timeSinceLastCall;
            thumbStickButtonLeftTimer += timeSinceLastCall;
            thumbStickButtonRightTimer += timeSinceLastCall;
        }

        #region Game-Specific Commands

        public bool PauseButton()
        {
            return PressedButton(Keys.P) || PressedButton(Buttons.Start);
        }

        public bool ContinueButton()
        {
            return PressedButton(Keys.Space) ||
                   PressedButton(Keys.Enter) ||
                    PressedButton(Buttons.A) ||
                    PressedButton(Buttons.Start);

        }

        private const float THUMBSTICK_DIRECTION_TRESHHOLD = 0.7f;
        private const float THUMBSTICK_DIRECTION_PAUSE_SECONDS = 0.2f;

        private float thumbStickButtonDownTimer = THUMBSTICK_DIRECTION_TRESHHOLD;
        private float thumbStickButtonUpTimer = THUMBSTICK_DIRECTION_TRESHHOLD;
        private float thumbStickButtonLeftTimer = THUMBSTICK_DIRECTION_TRESHHOLD;
        private float thumbStickButtonRightTimer = THUMBSTICK_DIRECTION_TRESHHOLD;

        public bool AnyDownButtonPressed()
        {
            foreach (GamePadState gstate in currentGamePadStates)
            {
                if (gstate.ThumbSticks.Left.Y < -THUMBSTICK_DIRECTION_TRESHHOLD && thumbStickButtonDownTimer > THUMBSTICK_DIRECTION_PAUSE_SECONDS)
                {
                    thumbStickButtonDownTimer = 0.0f;
                    return true;
                }
            }

            return PressedButton(Buttons.DPadDown) || PressedButton(Keys.Down);
        }

        public bool AnyUpButtonPressed()
        {
            foreach (GamePadState gstate in currentGamePadStates)
            {
                if (gstate.ThumbSticks.Left.Y > THUMBSTICK_DIRECTION_TRESHHOLD && thumbStickButtonUpTimer > THUMBSTICK_DIRECTION_PAUSE_SECONDS)
                {
                    thumbStickButtonUpTimer = 0.0f;
                    return true;
                }
            }

            return PressedButton(Buttons.DPadUp) || PressedButton(Keys.Up);
        }
        public bool AnyLeftButtonPressed()
        {
            foreach (GamePadState gstate in currentGamePadStates)
            {
                if (gstate.ThumbSticks.Left.X < -THUMBSTICK_DIRECTION_TRESHHOLD && thumbStickButtonLeftTimer > THUMBSTICK_DIRECTION_PAUSE_SECONDS)
                {
                    thumbStickButtonLeftTimer = 0.0f;
                    return true;
                }
            }

            return PressedButton(Buttons.DPadLeft) || PressedButton(Keys.Left);
        }

        public bool AnyRightButtonPressed()
        {
            foreach (GamePadState gstate in currentGamePadStates)
            {
                if (gstate.ThumbSticks.Left.X > THUMBSTICK_DIRECTION_TRESHHOLD && thumbStickButtonRightTimer > THUMBSTICK_DIRECTION_PAUSE_SECONDS)
                {
                    thumbStickButtonRightTimer = 0.0f;
                    return true;
                }
            }

            return PressedButton(Buttons.DPadRight) || PressedButton(Keys.Right);
        }

        #endregion

        #region Basic Input Commands

        public Vector2 GetLeftStickMovement(int controller)
        {
            return leftStickMovement[controller];
        }
        public Vector2 GetRightStickMovement(int controller)
        {
            return rightStickMovement[controller];
        }

        /// <summary>
        /// is this keyboard key down?
        /// </summary>
        public bool IsButtonDown(Keys button)
        {
            return currentKeyboardState.IsKeyDown(button);
        }
        /// <summary>
        /// is this keyboard key up?
        /// </summary>
        public bool IsButtonUp(Keys button)
        {
            return currentKeyboardState.IsKeyUp(button);
        }

        /// <summary>
        /// is the button of a specific gamepad down?
        /// </summary>
        public bool IsButtonDown(Buttons button, int controller)
        {
            return currentGamePadStates[controller].IsButtonDown(button);
        }
        /// <summary>
        /// is the button of a specific gamepad up?
        /// </summary>
        public bool IsButtonUp(Buttons button, int controller)
        {
            return currentGamePadStates[controller].IsButtonUp(button);
        }
        /// <summary>
        /// is the button of any gamepad down?
        /// </summary>
        public bool IsButtonDown(Buttons button)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (currentGamePadStates[i].IsButtonDown(button))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// is the button of any gamepad up?
        /// </summary>
        public bool IsButtonUp(Buttons button)
        {
            for (int i = 0; i < 4; ++i)
            {
                if (currentGamePadStates[i].IsButtonUp(button))
                    return true;
            }
            return false;
        }

        #endregion

        #region Advanced Input Commands

        public bool ReleasedButton(Keys key)
        {
            return currentKeyboardState.IsKeyUp(key) && oldKeyboardState.IsKeyDown(key);
        }
        public bool PressedButton(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyUp(key);
        }
        public bool ReleasedButton(Buttons button, int controller)
        {
            return currentGamePadStates[controller].IsButtonUp(button) && oldGamePadStates[controller].IsButtonDown(button);
        }
        public bool PressedButton(Buttons button, int controller)
        {
            return currentGamePadStates[controller].IsButtonDown(button) && oldGamePadStates[controller].IsButtonUp(button);
        }
        public bool ReleasedButton(Buttons button)
        {
            for (int controller = 0; controller < 4; ++controller)
            {
                if (currentGamePadStates[controller].IsButtonUp(button) && oldGamePadStates[controller].IsButtonDown(button))
                    return true;
            }
            return false;
        }
        public bool PressedButton(Buttons button)
        {
            for (int controller = 0; controller < 4; ++controller)
            {
                if (currentGamePadStates[controller].IsButtonDown(button) && oldGamePadStates[controller].IsButtonUp(button))
                    return true;
            }
            return false;
        }
        #endregion
    }
}