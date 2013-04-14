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

        static public readonly String[] CONTROL_NAMES = new String[]
        {
            "WASD + SPACE",
            "Arrows + ENTER",
            "Gamepad 1",
            "Gamepad 2",
            "Gamepad 3",
            "Gamepad 4",
            "No Control"
        };

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
            for (int i = 0; i < waitingForReconnect.Length; ++i)
            {
                if (waitingForReconnect[i])
                {
                    // relevant? ask settings
                    bool relevant = false;
                    for (int player = 0; player < Settings.Instance.NumPlayers; ++player)
                    {
                        if (Settings.Instance.PlayerControls[player] == ControlType.GAMEPAD0 + i)
                        {
                            relevant = true;
                            break;
                        }
                    }
                    if(!relevant)
                        waitingForReconnect[i] = false;
                    else
                        return true;
                }
            }
            return false;
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
                thumbStickButtonDownTimer[i] += timeSinceLastCall;
                thumbStickButtonUpTimer[i] += timeSinceLastCall;
                thumbStickButtonLeftTimer[i] += timeSinceLastCall;
                thumbStickButtonRightTimer[i] += timeSinceLastCall;

                // pressed
                if (WasThumbstickLeftPressed(i))
                    thumbStickButtonLeftTimer[i] = 0.0f;
                if (WasThumbstickRightPressed(i))
                    thumbStickButtonRightTimer[i] = 0.0f;
                if (WasThumbstickDownPressed(i))
                    thumbStickButtonDownTimer[i] = 0.0f;
                if (WasThumbstickUpPressed(i))
                    thumbStickButtonUpTimer[i] = 0.0f;

                if (!IsThumbstickLeft_Down(i))
                    thumbStickButtonLeftTimer[i] = THUMBSTICK_DIRECTION_TRESHHOLD;
                if (!IsThumbstickRight_Down(i))
                    thumbStickButtonRightTimer[i] = THUMBSTICK_DIRECTION_TRESHHOLD;
                if (!IsThumbstickUp_Down(i))
                    thumbStickButtonUpTimer[i] = THUMBSTICK_DIRECTION_TRESHHOLD;
                if (!IsThumbstickDown_Down(i))
                    thumbStickButtonDownTimer[i] = THUMBSTICK_DIRECTION_TRESHHOLD;
                

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

                // rumble
                rumbleTime[i] -= timeSinceLastCall;
                if (rumbleTime[i] <= 0f)
                {
                    rumbleTime[i] = 0f;
                    GamePad.SetVibration((PlayerIndex)i, 0f, 0f);
                }
            }

        }

        #region Game-Specific Commands

        public bool WasPauseButtonPressed()
        {
            return PressedButton(Keys.P) || PressedButton(Buttons.Start);
        }

        public bool WasContinueButtonPressed()
        {
            return PressedButton(Keys.Space) ||
                   PressedButton(Keys.Enter) ||
                    PressedButton(Buttons.A) ||
                    PressedButton(Buttons.Start);
        }

        public bool WasExitButtonPressed()
        {
            return PressedButton(Keys.Escape) || PressedButton(Buttons.Back);
        }

        private const float THUMBSTICK_DIRECTION_TRESHHOLD = 0.65f;
        private const float THUMBSTICK_DIRECTION_PAUSE_SECONDS = 0.2f;

        private float[] thumbStickButtonDownTimer = new float[4];
        private float[] thumbStickButtonUpTimer = new float[4];
        private float[] thumbStickButtonLeftTimer = new float[4];
        private float[] thumbStickButtonRightTimer = new float[4];

        public bool AnyDownButtonPressed()
        {
            for(int i=0; i<4; ++i)
            {
                if (WasThumbstickDownPressed(i))
                    return true;
            }

            return PressedButton(Buttons.DPadDown) || PressedButton(Keys.Down);
        }

        public bool AnyUpButtonPressed()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (WasThumbstickUpPressed(i))
                    return true;
            }

            return PressedButton(Buttons.DPadUp) || PressedButton(Keys.Up);
        }
        public bool AnyLeftButtonPressed()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (WasThumbstickLeftPressed(i))
                    return true;
            }

            return PressedButton(Buttons.DPadLeft) || PressedButton(Keys.Left);
        }

        public bool AnyRightButtonPressed()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (WasThumbstickRightPressed(i))
                    return true;
            }

            return PressedButton(Buttons.DPadRight) || PressedButton(Keys.Right);
        }

        public bool AnyDownButtonDown()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (IsThumbstickDown_Down(i))
                    return true;
            }

            return IsButtonDown(Buttons.DPadDown) || IsButtonDown(Keys.Down);
        }

        public bool AnyUpButtonDown()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (IsThumbstickUp_Down(i))
                    return true;
            }

            return IsButtonDown(Buttons.DPadUp) || IsButtonDown(Keys.Up);
        }
        public bool AnyLeftButtonDown()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (IsThumbstickLeft_Down(i))
                    return true;
            }

            return IsButtonDown(Buttons.DPadLeft) || IsButtonDown(Keys.Left);
        }

        public bool AnyRightButtonDown()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (IsThumbstickRight_Down(i))
                    return true;
            }

            return IsButtonDown(Buttons.DPadRight) || IsButtonDown(Keys.Right);
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

        private bool IsThumbstickLeft_Down(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.X < -THUMBSTICK_DIRECTION_TRESHHOLD;
        }

        private bool IsThumbstickRight_Down(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.X > THUMBSTICK_DIRECTION_TRESHHOLD;
        }

        private bool IsThumbstickUp_Down(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.Y > THUMBSTICK_DIRECTION_TRESHHOLD;
        }

        private bool IsThumbstickDown_Down(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.Y < -THUMBSTICK_DIRECTION_TRESHHOLD;
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
        public bool WasThumbstickLeftPressed(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.X < -THUMBSTICK_DIRECTION_TRESHHOLD && 
                        thumbStickButtonLeftTimer[controller] > THUMBSTICK_DIRECTION_PAUSE_SECONDS;
        }

        public bool WasThumbstickRightPressed(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.X > THUMBSTICK_DIRECTION_TRESHHOLD &&
                        thumbStickButtonRightTimer[controller] > THUMBSTICK_DIRECTION_PAUSE_SECONDS;
        }

        public bool WasThumbstickUpPressed(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.Y > THUMBSTICK_DIRECTION_TRESHHOLD &&
                         thumbStickButtonUpTimer[controller] > THUMBSTICK_DIRECTION_PAUSE_SECONDS;
        }

        public bool WasThumbstickDownPressed(int controller)
        {
            return currentGamePadStates[controller].ThumbSticks.Left.Y < -THUMBSTICK_DIRECTION_TRESHHOLD &&
                        thumbStickButtonDownTimer[controller] > THUMBSTICK_DIRECTION_PAUSE_SECONDS;
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
                case ControlType.KEYBOARD0: return (currentKeyboardState.IsKeyDown(Keys.Space) && oldKeyboardState.IsKeyUp(Keys.Space));
                case ControlType.KEYBOARD1: return (currentKeyboardState.IsKeyDown(Keys.Enter) && oldKeyboardState.IsKeyUp(Keys.Enter));
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
                case ControlType.KEYBOARD0: return (currentKeyboardState.IsKeyDown(Keys.V));
                case ControlType.KEYBOARD1: return (currentKeyboardState.IsKeyDown(Keys.RightShift));
                default: return false;
            }
        }

        public bool SpecificActionButtonPressed(ControlActions action, ControlType type, bool down)
        {
            switch (type)
            {
                case ControlType.KEYBOARD0:
                    switch (action)
	                {
                        case ControlActions.UP:
                            return down ? IsButtonDown(Keys.W) : PressedButton(Keys.W);
                        case ControlActions.DOWN:
                            return down ? IsButtonDown(Keys.S) : PressedButton(Keys.S);
                        case ControlActions.LEFT:
                            return down ? IsButtonDown(Keys.A) : PressedButton(Keys.A);
                        case ControlActions.RIGHT:
                            return down ? IsButtonDown(Keys.D) : PressedButton(Keys.D);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Keys.V) : PressedButton(Keys.V);
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Keys.Space) : PressedButton(Keys.Space);
	                }
                    break;
                case ControlType.KEYBOARD1:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return down ? IsButtonDown(Keys.Up) : PressedButton(Keys.Up);
                        case ControlActions.DOWN:
                            return down ? IsButtonDown(Keys.Down) : PressedButton(Keys.Down);
                        case ControlActions.LEFT:
                            return down ? IsButtonDown(Keys.Left) : PressedButton(Keys.Left);
                        case ControlActions.RIGHT:
                            return down ? IsButtonDown(Keys.Right) : PressedButton(Keys.Right);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Keys.RightShift) : PressedButton(Keys.RightShift);
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Keys.Enter) : PressedButton(Keys.Enter);
                    }
                    break;
                case ControlType.GAMEPAD0:
                case ControlType.GAMEPAD1:
                case ControlType.GAMEPAD2:
                case ControlType.GAMEPAD3:
                    int controller = (int)type - (int)ControlType.GAMEPAD0;
                    switch (action)
	                {
                        case ControlActions.UP:
                            return (down ? IsButtonDown(Buttons.DPadUp, controller) : PressedButton(Buttons.DPadUp, controller)) || 
                                    (down ? IsThumbstickUp_Down(controller) : WasThumbstickUpPressed(controller));
                        case ControlActions.DOWN:
                            return (down ? IsButtonDown(Buttons.DPadDown, controller) : PressedButton(Buttons.DPadDown, controller)) || 
                                     (down ? IsThumbstickDown_Down(controller) : WasThumbstickDownPressed(controller));
                        case ControlActions.LEFT:
                            return (down ? IsButtonDown(Buttons.DPadLeft, controller) : PressedButton(Buttons.DPadLeft, controller)) || 
                                    (down ? IsThumbstickLeft_Down(controller) : WasThumbstickLeftPressed(controller));
                        case ControlActions.RIGHT:
                            return (down ? IsButtonDown(Buttons.DPadRight, controller) : PressedButton(Buttons.DPadRight, controller)) ||
                                    (down ? IsThumbstickRight_Down(controller) : WasThumbstickRightPressed(controller));
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Buttons.A, controller) : PressedButton(Buttons.A, controller);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Buttons.B, controller) : PressedButton(Buttons.B, controller);
	                }
                    break;
            }

            return false;
        }

        #endregion

        #region rumble

        public bool ActivateRumble { get; set; }

        private float[] rumbleTime = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
        public void StartRumble(int playerIndex, float time, float strenght)
        {
            if (!ActivateRumble) return;
            switch (getControlTypeForPlayer[(int)playerIndex])
            {
                case ControlType.GAMEPAD0: GamePad.SetVibration(PlayerIndex.One, strenght, strenght); rumbleTime[0] = time; break;
                case ControlType.GAMEPAD1: GamePad.SetVibration(PlayerIndex.Two, strenght, strenght); rumbleTime[1] = time; break;
                case ControlType.GAMEPAD2: GamePad.SetVibration(PlayerIndex.Three, strenght, strenght); rumbleTime[2] = time; break;
                case ControlType.GAMEPAD3: GamePad.SetVibration(PlayerIndex.Four, strenght, strenght); rumbleTime[3] = time; break;
            }
        }
        #endregion
    }
}