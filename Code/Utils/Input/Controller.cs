namespace MonoFrostTemplate.Code.Utils.Input
{
    /// <summary>
    /// <para>Basic class for managing keyboard and controller input</para>
    /// </summary>
    public class Controller
    {
        public int id;
        private const int KeyboardPort = 0;
        public bool keyboardPriority = false;

        private const int Inputbuffer = 3;
        private const float deadzone = 0.4f;

        public const int NumKeys = 9;

        public const int Key_MenuConfirm = 0;
        public const int Key_MenuBack = 1;
        public const int Key_Start = 2;
        public const int Key_Up = 3;
        public const int Key_Down = 4;
        public const int Key_Left = 5;
        public const int Key_Right = 6;
        public const int Key_Jump = 7;
        public const int Key_Interact = 8;

        public void SetBinds()
        {
            keyBind[Key_MenuConfirm] = Keys.Enter;
            keyBind[Key_MenuBack] = Keys.Escape;
            keyBind[Key_Start] = Keys.Escape;
            keyBind[Key_Up] = Keys.W;
            keyBind[Key_Down] = Keys.S;
            keyBind[Key_Left] = Keys.A;
            keyBind[Key_Right] = Keys.D;
            keyBind[Key_Jump] = Keys.Space;
            keyBind[Key_Interact] = Keys.E;

            padBind[Key_MenuConfirm] = Buttons.A;
            padBind[Key_MenuBack] = Buttons.B;
            padBind[Key_Start] = Buttons.Start;
            padBind[Key_Up] = Buttons.LeftThumbstickUp;
            padBind[Key_Down] = Buttons.LeftThumbstickDown;
            padBind[Key_Left] = Buttons.LeftThumbstickLeft;
            padBind[Key_Right] = Buttons.LeftThumbstickRight;
            padBind[Key_Jump] = Buttons.B;
            padBind[Key_Interact] = Buttons.A;
        }

        protected Keys[] keyBind;
        protected Buttons[] padBind;
        protected int[] keyPressed;
        protected bool[] keyDown;
        protected int[] keyReleased;

        public Keys[] GetKeyBind => (Keys[])keyBind.Clone();
        public Buttons[] GetPadBind => (Buttons[])padBind.Clone();

        KeyboardState prevKeyState;
        GamePadState prevPadState;
        MouseState prevMouseState;

        public Controller() { }

        public Controller(int ID)
        {
            keyBind = new Keys[NumKeys];
            padBind = new Buttons[NumKeys];
            keyPressed = new int[NumKeys];
            keyDown = new bool[NumKeys];
            keyReleased = new int[NumKeys];

            id = ID;
            prevKeyState = Keyboard.GetState();
            prevPadState = GamePad.GetState(id);
            prevMouseState = Mouse.GetState();

            SetBinds();
        }

        public Buttons CorrectedButton(Buttons originalButton)
        {
            if (GamePad.GetCapabilities(id).DisplayName.Contains("Nintendo"))
            {
                if (originalButton == Buttons.A) originalButton = Buttons.B;
                else if (originalButton == Buttons.B) originalButton = Buttons.A;
                else if (originalButton == Buttons.X) originalButton = Buttons.Y;
                else if (originalButton == Buttons.Y) originalButton = Buttons.X;
            }

            return originalButton;
        }

        public static Buttons CorrectedButton(int controllerID, Buttons originalButton)
        {
            if (GamePad.GetCapabilities(controllerID).DisplayName.Contains("Nintendo"))
            {
                if (originalButton == Buttons.A) originalButton = Buttons.B;
                else if (originalButton == Buttons.B) originalButton = Buttons.A;
                else if (originalButton == Buttons.X) originalButton = Buttons.Y;
                else if (originalButton == Buttons.Y) originalButton = Buttons.X;
            }

            return originalButton;
        }

        public virtual void UpdateKeys()
        {
            KeyboardState keyState = Keyboard.GetState();
            GamePadState padState = GamePad.GetState(id);
            MouseState mouseState = Mouse.GetState();

            for (int i = 0; i < keyPressed.Length; i++)
            {
                if (Game.Instance.Scene is InGame scene)
                {
                    if (scene.hitpause <= 0)
                    {
                        if (keyPressed[i] > 0) keyPressed[i]--;
                        if (keyReleased[i] > 0) keyReleased[i]--;
                    }
                }

                bool keyboardOverrride = false;

                if (keyBind[i] < 0)
                {
                    if (keyBind[i] == (Keys)(-1))
                    {
                        if (mouseState.LeftButton == ButtonState.Pressed)
                        {
                            keyDown[i] = true;
                            if (prevMouseState.LeftButton != ButtonState.Pressed) keyPressed[i] = Inputbuffer;
                            keyboardOverrride = true;
                            keyboardPriority = true;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if (prevMouseState.LeftButton == ButtonState.Pressed) keyReleased[i] = Inputbuffer;
                        }
                    }
                    if (keyBind[i] == (Keys)(-2))
                    {
                        if (mouseState.RightButton == ButtonState.Pressed)
                        {
                            keyDown[i] = true;
                            if (prevMouseState.RightButton != ButtonState.Pressed) keyPressed[i] = Inputbuffer;
                            keyboardOverrride = true;
                            keyboardPriority = true;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if (prevMouseState.RightButton == ButtonState.Pressed) keyReleased[i] = Inputbuffer;
                        }
                    }
                }
                else
                {
                    if (keyState.IsKeyDown(keyBind[i]))
                    {
                        keyDown[i] = true;
                        if (!prevKeyState.IsKeyDown(keyBind[i])) keyPressed[i] = Inputbuffer;
                        keyboardOverrride = true;
                        keyboardPriority = true;
                    }
                    else
                    {
                        keyDown[i] = false;
                        if (prevKeyState.IsKeyDown(keyBind[i])) keyReleased[i] = Inputbuffer;
                    }
                }

                List<Buttons> bindOverride = new List<Buttons>() { Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.RightThumbstickUp, Buttons.RightThumbstickDown, Buttons.RightThumbstickLeft, Buttons.RightThumbstickRight };

                if (GamePad.GetState(id).IsConnected && !(id == KeyboardPort && keyboardOverrride))
                {
                    if (!bindOverride.Contains(CorrectedButton(padBind[i])))
                    {
                        if (padState.IsButtonDown(CorrectedButton(padBind[i])))
                        {
                            keyDown[i] = true;
                            if (!prevPadState.IsButtonDown(CorrectedButton(padBind[i]))) keyPressed[i] = Inputbuffer;
                            if (id == KeyboardPort) keyboardPriority = false;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if (prevPadState.IsButtonDown(CorrectedButton(padBind[i]))) keyReleased[i] = Inputbuffer;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickUp)
                    {
                        if (padState.ThumbSticks.Left.Y > deadzone || padState.IsButtonDown(Buttons.DPadUp))
                        {
                            keyDown[i] = true;
                            if ((padState.ThumbSticks.Left.Y > deadzone && prevPadState.ThumbSticks.Left.Y <= deadzone) || (padState.IsButtonDown(Buttons.DPadUp) && !prevPadState.IsButtonDown(Buttons.DPadUp))) keyPressed[i] = Inputbuffer;
                            if (id == KeyboardPort) keyboardPriority = false;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if ((padState.ThumbSticks.Left.Y <= deadzone && prevPadState.ThumbSticks.Left.Y > deadzone) || (!padState.IsButtonDown(Buttons.DPadUp) && prevPadState.IsButtonDown(Buttons.DPadUp))) keyReleased[i] = Inputbuffer;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickDown)
                    {
                        if (padState.ThumbSticks.Left.Y < -deadzone || padState.IsButtonDown(Buttons.DPadDown))
                        {
                            keyDown[i] = true;
                            if ((padState.ThumbSticks.Left.Y < -deadzone && prevPadState.ThumbSticks.Left.Y >= -deadzone) || (padState.IsButtonDown(Buttons.DPadDown) && !prevPadState.IsButtonDown(Buttons.DPadDown))) keyPressed[i] = Inputbuffer;
                            if (id == KeyboardPort) keyboardPriority = false;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if ((padState.ThumbSticks.Left.Y >= -deadzone && prevPadState.ThumbSticks.Left.Y < -deadzone) || (!padState.IsButtonDown(Buttons.DPadDown) && prevPadState.IsButtonDown(Buttons.DPadDown))) keyReleased[i] = Inputbuffer;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickRight)
                    {
                        if (padState.ThumbSticks.Left.X > deadzone || padState.IsButtonDown(Buttons.DPadRight))
                        {
                            keyDown[i] = true;
                            if ((padState.ThumbSticks.Left.X > deadzone && prevPadState.ThumbSticks.Left.X <= deadzone) || (padState.IsButtonDown(Buttons.DPadRight) && !prevPadState.IsButtonDown(Buttons.DPadRight))) keyPressed[i] = Inputbuffer;
                            if (id == KeyboardPort) keyboardPriority = false;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if ((padState.ThumbSticks.Left.X <= deadzone && prevPadState.ThumbSticks.Left.X > deadzone) || (!padState.IsButtonDown(Buttons.DPadRight) && prevPadState.IsButtonDown(Buttons.DPadRight))) keyReleased[i] = Inputbuffer;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickLeft)
                    {
                        if (padState.ThumbSticks.Left.X < -deadzone || padState.IsButtonDown(Buttons.DPadLeft))
                        {
                            keyDown[i] = true;
                            if ((padState.ThumbSticks.Left.X < -deadzone && prevPadState.ThumbSticks.Left.X >= -deadzone) || (padState.IsButtonDown(Buttons.DPadLeft) && !prevPadState.IsButtonDown(Buttons.DPadLeft))) keyPressed[i] = Inputbuffer;
                            if (id == KeyboardPort) keyboardPriority = false;
                        }
                        else
                        {
                            keyDown[i] = false;
                            if ((padState.ThumbSticks.Left.X >= -deadzone && prevPadState.ThumbSticks.Left.X < -deadzone) || (!padState.IsButtonDown(Buttons.DPadLeft) && prevPadState.IsButtonDown(Buttons.DPadLeft))) keyReleased[i] = Inputbuffer;
                        }
                    }
                }
            }

            prevKeyState = Keyboard.GetState();
            prevPadState = GamePad.GetState(id);
            prevMouseState = Mouse.GetState();
        }

        public void ClearBuffer(int key)
        {
            keyPressed[key] = 0;
            keyReleased[key] = 0;
        }

        public void ClearAllBuffers()
        {
            for (int i = 0; i < NumKeys; i++)
            {
                keyPressed[i] = 0;
                keyReleased[i] = 0;
            }
        }

        public void ClearAllInputs()
        {
            for (int i = 0; i < NumKeys; i++)
            {
                keyPressed[i] = 0;
                keyDown[i] = false;
                keyReleased[i] = 0;
            }
        }

        public void PressKey(int key)
        {
            keyPressed[key] = Inputbuffer;
            keyDown[key] = true;
        }

        public bool KeyPressed(int key)
        {
            return keyPressed[key] > 0 && keyPressed[key] <= Inputbuffer;
        }

        /// <summary>
        /// Returns true if the key was just pressed. If true, clear the input buffer for the key
        /// </summary>
        public bool ReceiveKeyPress(int key)
        {
            bool check = keyPressed[key] > 0 && keyPressed[key] <= Inputbuffer;
            if (check)
            {
                keyPressed[key] = 0;
                keyReleased[key] = 0;
            }
            return check;
        }

        public bool KeyDown(int key)
        {
            return (keyDown[key] && keyPressed[key] <= Inputbuffer) || keyReleased[key] > Inputbuffer;
        }

        public bool KeyReleased(int key)
        {
            return keyReleased[key] > 0 && keyReleased[key] <= Inputbuffer;
        }

        /// <summary>
        /// Returns true if the key was just released. If true, clear the input buffer for the key
        /// </summary>
        public bool ReceiveKeyRelease(int key)
        {
            bool check = keyReleased[key] > 0 && keyReleased[key] <= Inputbuffer;
            if (check)
            {
                keyPressed[key] = 0;
                keyReleased[key] = 0;
            }
            return check;
        }

        public void RebindKeyboard(int input, Keys key)
        {
            keyBind[input] = key;
        }
        public void RebindController(int input, Buttons button)
        {
            padBind[input] = button;
        }
    }
}
