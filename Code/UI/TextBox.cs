using MonoFrostTemplate.Code.Utils.Visual;

namespace MonoFrostTemplate.Code.UI
{
    /// <summary>
    /// Allows keyboard input into the ui to ouput the entered text
    /// </summary>
    public class TextBox : TextBlock
    {
        public bool active;
        private int flashingBarTimer;
        public string promptText = "";
        public string entryText = "";

        /// <summary>
        /// Runs when the user presses ENTER while active
        /// </summary>
        public delegate void OnEnterText(TextBox textbox);
        public OnEnterText onEnter;

        private ButtonMatrix holdMatrix;
        private MenuCursor holdCursor;

        KeyboardState prevKeyState = Keyboard.GetState();

        List<Keys> availableKeys = new List<Keys>();

        public TextBox(string entryText, string promptText, string fontURL, Color textColor, string texture, float x = 0, float y = 0) : base(promptText + entryText, fontURL, textColor, texture, x, y)
        {
            for (Keys k = Keys.A; k <= Keys.Z; k++) availableKeys.Add(k);
            availableKeys.Add(Keys.Space);

            this.promptText = promptText;
            this.entryText = entryText;

            onClick = OpenText;
        }

        public TextBox(string entryText, string promptText, string fontURL, Color textColor, Animation animation, float x = 0, float y = 0) : base(promptText + entryText, fontURL, textColor, animation, x, y)
        {
            for (Keys k = Keys.A; k <= Keys.Z; k++) availableKeys.Add(k);
            availableKeys.Add(Keys.Space);

            this.promptText = promptText;
            this.entryText = entryText;

            onClick = OpenText;
        }

        public override void Update()
        {
            base.Update();

            if (active)
            {
                flashingBarTimer++;
                if (flashingBarTimer >= 60) flashingBarTimer = 0;

                foreach (Keys k in availableKeys)
                {
                    if (Keyboard.GetState().IsKeyDown(k) && !prevKeyState.IsKeyDown(k))
                    {
                        if (k == Keys.Space) entryText += ' ';
                        else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)) entryText += k.ToString().ToUpper();
                        else entryText += k.ToString().ToLower();
                    }
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Back) && !prevKeyState.IsKeyDown(Keys.Back) && entryText.Length > 0) entryText = entryText.Remove(entryText.Length - 1);
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyState.IsKeyDown(Keys.Enter))
                {
                    active = false;
                    if (onEnter != null) onEnter(this);
                    holdCursor.matrix = holdMatrix;
                }
                if (holdCursor != null && holdCursor.input.id != -1 && holdCursor.input.KeyPressed(Controller.Key_MenuConfirm) || holdCursor.input.KeyPressed(Controller.Key_MenuBack))
                {
                    holdCursor.input.ClearAllBuffers();
                    active = false;
                    if (onEnter != null) onEnter(this);
                    holdCursor.matrix = holdMatrix;
                }

                prevKeyState = Keyboard.GetState();
            }
            else flashingBarTimer = 0;

            text = promptText + entryText;
            if ((flashingBarTimer / 30) % 2 == 1) text = ' ' + text + '|';
        }

        void OpenText(MenuCursor cursor, ButtonMatrix matrix)
        {
            active = true;
            holdMatrix = matrix;
            holdCursor = cursor;

            cursor.matrix = null;
        }
    }
}
