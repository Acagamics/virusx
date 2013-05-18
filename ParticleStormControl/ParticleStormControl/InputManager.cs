using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace ParticleStormControl
{
    class InputManager
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
            "Numpad + 0",
            "Gamepad 1",
            "Gamepad 2",
            "Gamepad 3",
            "Gamepad 4",
            "Computer"
        };

        /// <summary>
        /// Contains all possible control types of the game.
        /// </summary>
        public enum ControlType
        {
            KEYBOARD0,
            KEYBOARD1,
            KEYBOARD2,
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
            HOLD,

            ADD_AI,
            REMOVE_AI,

            PAUSE,
            EXIT
        }


        private const float THUMBSTICK_DIRECTION_TRESHHOLD = 0.65f;
        private const float THUMBSTICK_DIRECTION_PAUSE_SECONDS = 0.2f;

        private float[] thumbStickButtonDownTimer = new float[4];
        private float[] thumbStickButtonUpTimer = new float[4];
        private float[] thumbStickButtonLeftTimer = new float[4];
        private float[] thumbStickButtonRightTimer = new float[4];

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
        //private ControlType[] getControlTypeForPlayer = { ControlType.KEYBOARD0, ControlType.KEYBOARD1, ControlType.GAMEPAD0, ControlType.GAMEPAD1 };

        #endregion

        /// <summary>
        /// Resets for all players the Control Type to NONE
        /// </summary>
        /*public void resetAllControlTypes()
        {
            for (int i = 0; i < getControlTypeForPlayer.Length; ++i)
                getControlTypeForPlayer[i] = ControlType.NONE;
        }*/

        /// <summary>
        /// Set the control scheme for the given player. The control scheme will only be changed, if no other player uses the given scheme.
        /// If the control scheme to change is the same as the current scheme of the player nothing happens.
        /// </summary>
        /// <param name="index">the player</param>
        /// <param name="controlType">the control scheme</param>
        /// <returns>true if the control scheme changed, false otherwise</returns>
        /*public bool setControlType(int index, ControlType controlType)
        {
            //foreach (ControlType ct in playerToControl)
            //    if (ct == controlType) return false;
            getControlTypeForPlayer[index] = controlType;
            return true;
        }*/

        /*public ControlType getControlType(int index)
        {
            return getControlTypeForPlayer[index];
        }*/

        /// <summary>
        /// is the game waiting for any reconnect
        /// </summary>
        /// <returns>true if waiting</returns>
        public bool IsWaitingForReconnect()
        {
            for (int controller = 0; controller < waitingForReconnect.Length; ++controller)
            {
                if (waitingForReconnect[controller])
                {
                    // relevant? ask settings
                    bool relevant = false;
                    for (int player = 0; player < Settings.Instance.NumPlayers; ++player)
                    {
                        if (Settings.Instance.GetPlayer(player).ControlType == ControlType.GAMEPAD0 + controller)
                        {
                            relevant = true;
                            break;
                        }
                    }
                    if(!relevant)
                        waitingForReconnect[controller] = false;
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
        
        public bool IsWaitingForReconnect(ControlType controlType)
        {
            if (InputManager.CanLooseConnection(controlType))
                return IsWaitingForReconnect((int)controlType - (int)ControlType.GAMEPAD0);
            else
                return false;
        }

        public static bool CanLooseConnection(ControlType controlType)
        {
            return controlType != ControlType.NONE &&
                    controlType != ControlType.KEYBOARD0 &&
                    controlType != ControlType.KEYBOARD1 &&
                    controlType != ControlType.KEYBOARD2;
        }

        public static bool IsKeyboardControlType(ControlType controlType)
        {
            return  controlType == ControlType.KEYBOARD0 ||
                    controlType == ControlType.KEYBOARD1 ||
                    controlType == ControlType.KEYBOARD2;
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
                    waitingForReconnect[i] = !currentGamePadStates[i].IsConnected;

                // rumble
                rumbleTime[i] -= timeSinceLastCall;
                if (rumbleTime[i] <= 0f)
                {
                    rumbleTime[i] = 0f;
                    GamePad.SetVibration((PlayerIndex)i, 0f, 0f);
                }
            }

        }

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
        public bool IsButtonPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyUp(key);
        }
        public bool ReleasedButton(Buttons button, int controller)
        {
            return currentGamePadStates[controller].IsButtonUp(button) && oldGamePadStates[controller].IsButtonDown(button);
        }
        public bool IsButtonPressed(Buttons button, int controller)
        {
            return currentGamePadStates[controller].IsButtonDown(button) && oldGamePadStates[controller].IsButtonUp(button);
        }
        public bool AnyReleasedButton(Buttons button)
        {
            for (int controller = 0; controller < 4; ++controller)
            {
                if (currentGamePadStates[controller].IsButtonUp(button) && oldGamePadStates[controller].IsButtonDown(button))
                    return true;
            }
            return false;
        }
        public bool AnyPressedButton(Buttons button)
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

        #region Input by Action

        public Vector2 GetMovement(int index)
        {
            Vector2 result = Vector2.Zero;
            if (Settings.Instance.NumPlayers <= index) return result;
            switch(Settings.Instance.GetPlayer(index).ControlType)//getControlTypeForPlayer[index])
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
                case ControlType.KEYBOARD2:
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad8))
                        result.Y += 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad5) || currentKeyboardState.IsKeyDown(Keys.NumPad2))
                        result.Y -= 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad4))
                        result.X -= 1f;
                    if (currentKeyboardState.IsKeyDown(Keys.NumPad6))
                        result.X += 1f;
                    break;
                default: return result;
            }
            result.Y = -result.Y;
            return result;
        }

        /// <summary>
        /// returns true if any input device pressed or hold down a certain action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="down">true if hold down is asked, false if "pressed-event" is needed</param>
        /// <returns>true for pressed/down, false otherwise</returns>
        public bool WasAnyActionPressed(ControlActions action, bool down = false)
        {
            ControlType type;
            return WasAnyActionPressed(action, out type, down);
        }

        /// <summary>
        /// returns true if any input device pressed or hold down a certain action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="typePressed">control type that did the given action</param>
        /// <param name="down">true if hold down is asked, false if "pressed-event" is needed</param>
        /// <returns>true for pressed/down, false otherwise</returns>
        public bool WasAnyActionPressed(ControlActions action, out ControlType typePressed, bool down = false)
        {
            foreach (ControlType control in typeof(ControlType).GetEnumValues())
            {
                if (SpecificActionButtonPressed(action, control, down))
                {
                    typePressed = control;
                    return true;
                }
            }
            typePressed = ControlType.NONE;
            return false;
        }

        /// <summary>
        /// returns true if a specific action button was pressed or hold down
        /// uses the settings singleton to lookup players
        /// </summary>
        /// <see cref="Settings"/>
        /// <param name="action"></param>
        /// <param name="playerIndex">-1 for any active</param>
        /// <param name="down">true if hold down is asked, false if "pressed-event" is needed</param>
        /// <returns>true for pressed/down, false otherwise</returns>
        public bool SpecificActionButtonPressed(ControlActions action, int playerIndex, bool down = false)
        {
            Debug.Assert(playerIndex < Settings.Instance.NumPlayers);

            if(playerIndex < 0)
            {
                for(int i=0; i<Settings.Instance.NumPlayers; ++i)
                {
                    if(SpecificActionButtonPressed(action, Settings.Instance.GetPlayer(i).ControlType, down))
                        return true;
                }
                return false;
            }
            else
                return SpecificActionButtonPressed(action, Settings.Instance.GetPlayer(playerIndex).ControlType, down);
        }

        /// <summary>
        /// returns true if a specific action button was pressed or hold down
        /// </summary>
        /// <param name="action"></param>
        /// <param name="type"></param>
        /// <param name="down">true if hold down is asked, false if "pressed-event" is needed</param>
        /// <returns>true for pressed/down, false otherwise</returns>
        public bool SpecificActionButtonPressed(ControlActions action, ControlType type, bool down = false)
        {
            switch (type)
            {
                case ControlType.KEYBOARD0:
                    switch (action)
	                {
                        case ControlActions.UP:
                            return down ? IsButtonDown(Keys.W) : IsButtonPressed(Keys.W);
                        case ControlActions.DOWN:
                            return down ? IsButtonDown(Keys.S) : IsButtonPressed(Keys.S);
                        case ControlActions.LEFT:
                            return down ? IsButtonDown(Keys.A) : IsButtonPressed(Keys.A);
                        case ControlActions.RIGHT:
                            return down ? IsButtonDown(Keys.D) : IsButtonPressed(Keys.D);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Keys.V) : IsButtonPressed(Keys.V);
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Keys.Space) : IsButtonPressed(Keys.Space);
                        case ControlActions.PAUSE:
                            return down ? (IsButtonDown(Keys.P) || IsButtonDown(Keys.Escape)) : (IsButtonPressed(Keys.P) || IsButtonPressed(Keys.Escape));
                        case ControlActions.EXIT:
                            return down ? IsButtonDown(Keys.Escape) : IsButtonPressed(Keys.Escape);

                        case ControlActions.ADD_AI:
                            return IsButtonPressed(Keys.OemPlus);
                        case ControlActions.REMOVE_AI:
                            return IsButtonPressed(Keys.OemMinus);
	                }
                    break;
                case ControlType.KEYBOARD1:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return down ? IsButtonDown(Keys.Up) : IsButtonPressed(Keys.Up);
                        case ControlActions.DOWN:
                            return down ? IsButtonDown(Keys.Down) : IsButtonPressed(Keys.Down);
                        case ControlActions.LEFT:
                            return down ? IsButtonDown(Keys.Left) : IsButtonPressed(Keys.Left);
                        case ControlActions.RIGHT:
                            return down ? IsButtonDown(Keys.Right) : IsButtonPressed(Keys.Right);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Keys.RightShift) : IsButtonPressed(Keys.RightShift);
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Keys.Enter) : IsButtonPressed(Keys.Enter);
                        case ControlActions.PAUSE:
                            return down ? (IsButtonDown(Keys.P) || IsButtonDown(Keys.Escape)) : (IsButtonPressed(Keys.P) || IsButtonPressed(Keys.Escape));
                        case ControlActions.EXIT:
                            return down ? IsButtonDown(Keys.Escape) : IsButtonPressed(Keys.Escape);

                        case ControlActions.ADD_AI:
                            return IsButtonPressed(Keys.OemPlus);
                        case ControlActions.REMOVE_AI:
                            return IsButtonPressed(Keys.OemMinus);
                    }
                    break;
                case ControlType.KEYBOARD2:
                    switch (action)
                    {
                        case ControlActions.UP:
                            return down ? IsButtonDown(Keys.NumPad8) : IsButtonPressed(Keys.NumPad8);
                        case ControlActions.DOWN:
                            return down ? IsButtonDown(Keys.NumPad5) || IsButtonDown(Keys.NumPad2) : 
                                            IsButtonPressed(Keys.NumPad5) || IsButtonPressed(Keys.NumPad2);
                        case ControlActions.LEFT:
                            return down ? IsButtonDown(Keys.NumPad4) : IsButtonPressed(Keys.NumPad4);
                        case ControlActions.RIGHT:
                            return down ? IsButtonDown(Keys.NumPad6) : IsButtonPressed(Keys.NumPad6);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Keys.NumPad7) || IsButtonDown(Keys.NumPad9) :
                                            IsButtonPressed(Keys.NumPad7) || IsButtonDown(Keys.NumPad9);
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Keys.NumPad0) : IsButtonPressed(Keys.NumPad0);
                        case ControlActions.PAUSE:
                            return down ? (IsButtonDown(Keys.P) || IsButtonDown(Keys.Escape)) : (IsButtonPressed(Keys.P) || IsButtonPressed(Keys.Escape));
                        case ControlActions.EXIT:
                            return down ? IsButtonDown(Keys.Escape) : IsButtonPressed(Keys.Escape);

                        case ControlActions.ADD_AI:
                            return IsButtonPressed(Keys.OemPlus);
                        case ControlActions.REMOVE_AI:
                            return IsButtonPressed(Keys.OemMinus);
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
                            return (down ? IsButtonDown(Buttons.DPadUp, controller) : IsButtonPressed(Buttons.DPadUp, controller)) || 
                                    (down ? IsThumbstickUp_Down(controller) : WasThumbstickUpPressed(controller));
                        case ControlActions.DOWN:
                            return (down ? IsButtonDown(Buttons.DPadDown, controller) : IsButtonPressed(Buttons.DPadDown, controller)) || 
                                     (down ? IsThumbstickDown_Down(controller) : WasThumbstickDownPressed(controller));
                        case ControlActions.LEFT:
                            return (down ? IsButtonDown(Buttons.DPadLeft, controller) : IsButtonPressed(Buttons.DPadLeft, controller)) || 
                                    (down ? IsThumbstickLeft_Down(controller) : WasThumbstickLeftPressed(controller));
                        case ControlActions.RIGHT:
                            return (down ? IsButtonDown(Buttons.DPadRight, controller) : IsButtonPressed(Buttons.DPadRight, controller)) ||
                                    (down ? IsThumbstickRight_Down(controller) : WasThumbstickRightPressed(controller));
                        case ControlActions.ACTION:
                            return down ? IsButtonDown(Buttons.A, controller) : IsButtonPressed(Buttons.A, controller);
                        case ControlActions.HOLD:
                            return down ? IsButtonDown(Buttons.B, controller) : IsButtonPressed(Buttons.B, controller);
                        case ControlActions.PAUSE:
                            return down ? IsButtonDown(Buttons.Start, controller) : IsButtonPressed(Buttons.Start, controller);
                        case ControlActions.EXIT:
                            return down ? IsButtonDown(Buttons.Back, controller) : IsButtonPressed(Buttons.Back, controller);

                        case ControlActions.ADD_AI:
                            return IsButtonPressed(Buttons.RightShoulder, controller);
                        case ControlActions.REMOVE_AI:
                            return IsButtonPressed(Buttons.LeftShoulder, controller);
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
            switch (Settings.Instance.GetPlayer(playerIndex).ControlType/* getControlTypeForPlayer[(int)playerIndex]*/)
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