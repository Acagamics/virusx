using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace ParticleStormControl
{
    public class InputManager
    {
        #region singleton
        private static readonly InputManager instance = new InputManager();
        public static InputManager Instance { get { return instance; } }
        private InputManager() { }
        #endregion

        #region enumerations
        /// <summary>
        /// Contains all possible control types of the game.
        /// </summary>
        public enum ControlType
        {
            KEYBOARD0,
            KEYBOARD1,
            GAMEPAD0,
            GAMEPAD1,
            GAMEPAD2,
            GAMEPAD3,
            NONE
        }
        /// <summary>
        /// Contains all possible player actions.
        /// </summary>
        public enum ControlActions
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            ACTION,
            HOLD
        }

        
        #endregion

        #region state
        private GamePadState[] currentGamePadStates = new GamePadState[4];
        private GamePadState[] oldGamePadStates = new GamePadState[4];
        private KeyboardState currentKeyboardState = new KeyboardState();
        private KeyboardState oldKeyboardState = new KeyboardState();
        
        private Vector2[] rightStickMovement = new Vector2[4];
        private Vector2[] leftStickMovement = new Vector2[4];

        private bool[] waitingForReconnect = { false, false, false, false };

        /// <summary>
        /// Matching of player to used controls of the player.
        /// </summary>
        private ControlType[] getControlTypeForPlayer = { ControlType.KEYBOARD0, ControlType.KEYBOARD1, ControlType.GAMEPAD0, ControlType.GAMEPAD1 };

        #endregion

        /// <summary>
        /// Resets for all players the Control Type to NONE
        /// </summary>
        public void resetAllControlTypes()
        {
            for (int i = 0; i < getControlTypeForPlayer.Length; ++i)
                getControlTypeForPlayer[i] = ControlType.NONE;
        }

        /// <summary>
        /// Set the control scheme for the given player. The control scheme will only be changed, if no other player uses the given scheme.
        /// If the control scheme to change is the same as the current scheme of the player nothing happens.
        /// </summary>
        /// <param name="index">the player</param>
        /// <param name="controlType">the control scheme</param>
        /// <returns>true if the control scheme changed, false otherwise</returns>
        public bool setControlType(PlayerIndex index, ControlType controlType)
        {
            //foreach (ControlType ct in playerToControl)
            //    if (ct == controlType) return false;
            getControlTypeForPlayer[(int)index] = controlType;
            return true;
        }

        public ControlType getControlType(PlayerIndex index)
        {
            return getControlTypeForPlayer[(int)index];
        }

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

        public bool ExitButton()
        {
            return PressedButton(Keys.Escape) || PressedButton(Buttons.Back);
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

        #region get specific actions

        public Vector2 GetMovement(PlayerIndex index)
        {
            Vector2 result = Vector2.Zero;
            if (Settings.Instance.NumPlayers <= (int)index) return result;
            switch(getControlTypeForPlayer[(int)index])
            {
                case ControlType.GAMEPAD0: result = currentGamePadStates[0].ThumbSticks.Left; break;
                case ControlType.GAMEPAD1: result = currentGamePadStates[1].ThumbSticks.Left; break;
                case ControlType.GAMEPAD2: result = currentGamePadStates[2].ThumbSticks.Left; break;
                case ControlType.GAMEPAD3: result = currentGamePadStates[3].ThumbSticks.Left; break;
                case ControlType.KEYBOARD0:
                    if (currentKeyboardState.IsKeyDown(Keys.W))
                        result.Y += 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.S))
                        result.Y -= 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.A))
                        result.X -= 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.D))
                        result.X += 1f;
                    break;
                case ControlType.KEYBOARD1:
                    if (currentKeyboardState.IsKeyDown(Keys.Up))
                        result.Y += 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.Down))
                        result.Y -= 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.Left))
                        result.X -= 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.Right))
                        result.X += 1f;
                    break;
                default: return result;
            }
            result.Y = -result.Y;
            return result;
        }

        public bool ActionButtonPressed(PlayerIndex index)
        {
            if (Settings.Instance.NumPlayers <= (int)index) return false;
            switch (getControlTypeForPlayer[(int)index])
            {
                case ControlType.GAMEPAD0: return (currentGamePadStates[0].IsButtonDown(Buttons.A) && oldGamePadStates[0].IsButtonUp(Buttons.A));
                case ControlType.GAMEPAD1: return (currentGamePadStates[1].IsButtonDown(Buttons.A) && oldGamePadStates[1].IsButtonUp(Buttons.A));
                case ControlType.GAMEPAD2: return (currentGamePadStates[2].IsButtonDown(Buttons.A) && oldGamePadStates[2].IsButtonUp(Buttons.A));
                case ControlType.GAMEPAD3: return (currentGamePadStates[3].IsButtonDown(Buttons.A) && oldGamePadStates[3].IsButtonUp(Buttons.A));
                case ControlType.KEYBOARD0: return (currentKeyboardState.IsKeyDown(Keys.E) && oldKeyboardState.IsKeyUp(Keys.E));
                case ControlType.KEYBOARD1: return (currentKeyboardState.IsKeyDown(Keys.RightShift) && oldKeyboardState.IsKeyUp(Keys.RightShift));
                default: return false;
            }
        }

        public List<ControlType> ContinueButtonsPressed()
        {
            List<ControlType> result = new List<ControlType>();

            if (PressedButton(Buttons.A, 0)) { result.Add(ControlType.GAMEPAD0); }
            if (PressedButton(Buttons.A, 1)) { result.Add(ControlType.GAMEPAD1); }
            if (PressedButton(Buttons.A, 2)) { result.Add(ControlType.GAMEPAD2); }
            if (PressedButton(Buttons.A, 3)) { result.Add(ControlType.GAMEPAD3); }
            if (PressedButton(Keys.Space)) { result.Add(ControlType.KEYBOARD0); }
            if (PressedButton(Keys.Enter)) { result.Add(ControlType.KEYBOARD1); }

            return result;
        }

        public bool HoldButtonPressed(PlayerIndex index)
        {
            if (Settings.Instance.NumPlayers <= (int)index) return false;
            switch (getControlTypeForPlayer[(int)index])
            {
                case ControlType.GAMEPAD0: return (currentGamePadStates[0].IsButtonDown(Buttons.B));
                case ControlType.GAMEPAD1: return (currentGamePadStates[1].IsButtonDown(Buttons.B));
                case ControlType.GAMEPAD2: return (currentGamePadStates[2].IsButtonDown(Buttons.B));
                case ControlType.GAMEPAD3: return (currentGamePadStates[3].IsButtonDown(Buttons.B));
                case ControlType.KEYBOARD0: return (currentKeyboardState.IsKeyDown(Keys.LeftShift));
                case ControlType.KEYBOARD1: return (currentKeyboardState.IsKeyDown(Keys.RightControl));
                default: return false;
            }
        }

        public bool DirectionButtonPressed(ControlActions action, ControlType type)
        {
            switch (type)
            {
                case ControlType.KEYBOARD0:
                    switch (action)
	                {
                        case ControlActions.UP:
                            return PressedButton(Keys.W);
                        case ControlActions.DOWN:
                            return PressedButton(Keys.S);
                        case ControlActions.LEFT:
                            return PressedButton(Keys.A);
                        case ControlActions.RIGHT:
                            return PressedButton(Keys.D);
	                }
                    break;
                case ControlType.KEYBOARD1:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return PressedButton(Keys.Up);
                        case ControlActions.DOWN:
                            return PressedButton(Keys.Down);
                        case ControlActions.LEFT:
                            return PressedButton(Keys.Left);
                        case ControlActions.RIGHT:
                            return PressedButton(Keys.Right);
                    }
                    break;
                case ControlType.GAMEPAD0:
                    switch (action)
	                {
                        case ControlActions.UP:
                            return PressedButton(Buttons.DPadUp, 0);
                        case ControlActions.DOWN:
                            return PressedButton(Buttons.DPadDown, 0);
                        case ControlActions.LEFT:
                            return PressedButton(Buttons.DPadLeft, 0);
                        case ControlActions.RIGHT:
                            return PressedButton(Buttons.DPadRight, 0);
	                }
                    break;
                case ControlType.GAMEPAD1:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return PressedButton(Buttons.DPadUp, 1);
                        case ControlActions.DOWN:
                            return PressedButton(Buttons.DPadDown, 1);
                        case ControlActions.LEFT:
                            return PressedButton(Buttons.DPadLeft, 1);
                        case ControlActions.RIGHT:
                            return PressedButton(Buttons.DPadRight, 1);
                    }
                    break;
                case ControlType.GAMEPAD2:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return PressedButton(Buttons.DPadUp, 2);
                        case ControlActions.DOWN:
                            return PressedButton(Buttons.DPadDown, 2);
                        case ControlActions.LEFT:
                            return PressedButton(Buttons.DPadLeft, 2);
                        case ControlActions.RIGHT:
                            return PressedButton(Buttons.DPadRight, 2);
                    }
                    break;
                case ControlType.GAMEPAD3:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return PressedButton(Buttons.DPadUp, 3);
                        case ControlActions.DOWN:
                            return PressedButton(Buttons.DPadDown, 3);
                        case ControlActions.LEFT:
                            return PressedButton(Buttons.DPadLeft, 3);
                        case ControlActions.RIGHT:
                            return PressedButton(Buttons.DPadRight, 3);
                    }
                    break;
            }

            return false;
        }

        #endregion
    }
}