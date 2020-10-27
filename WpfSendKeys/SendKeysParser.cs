using System.Collections.Generic;

namespace System.Windows.Input.Test
{
    public class SendKeysParser
    {
        string text;
        int current = 0;
        bool insideParentheses = false;
        ModifierKeys currentModifiers = ModifierKeys.None;
        List<KeyPressInfo> result = new List<KeyPressInfo>();

        public static IEnumerable<KeyPressInfo> Parse(string text)
        {
            return new SendKeysParser().ParseSendKeysString(text);
        }

        public IEnumerable<KeyPressInfo> ParseSendKeysString(string text)
        {
            this.text = text;
            current = 0;
            int oldCurrent = current;

            while (current < text.Length)
            {
                if (CurrentChar == '{')
                {
                    ParseCurly();
                    if (!insideParentheses && currentModifiers != ModifierKeys.None)
                    {
                        currentModifiers = ModifierKeys.None;
                    }
                }
                else if (CurrentChar == '~')
                {
                    current++;
                    Add(Key.Return);
                    if (!insideParentheses && currentModifiers != ModifierKeys.None)
                    {
                        currentModifiers = ModifierKeys.None;
                    }
                }
                else if (CurrentChar == '+')
                {
                    currentModifiers = currentModifiers | ModifierKeys.Shift;
                    current++;
                }
                else if (CurrentChar == '^')
                {
                    currentModifiers = currentModifiers | ModifierKeys.Control;
                    current++;
                }
                else if (CurrentChar == '%')
                {
                    currentModifiers = currentModifiers | ModifierKeys.Alt;
                    current++;
                }
                else if (CurrentChar == '(')
                {
                    if (insideParentheses)
                    {
                        Error("Unbalanced parentheses: unexpected second (");
                    }
                    insideParentheses = true;
                    current++;
                }
                else if (CurrentChar == ')')
                {
                    if (!insideParentheses)
                    {
                        Error("Unbalanced parentheses: unexpected closing )");
                    }
                    insideParentheses = false;
                    currentModifiers = ModifierKeys.None;
                    current++;
                }
                else if (IsPrintableChar())
                {
                    ParseChar();
                    if (!insideParentheses && currentModifiers != ModifierKeys.None)
                    {
                        currentModifiers = ModifierKeys.None;
                    }
                }
                else
                {
                    Error("Unexpected character: '" + CurrentChar + "'");
                }

                if (oldCurrent == current)
                {
                    Error("Didn't advance forward, stuck at parsing character '" + CurrentChar + "'");
                }
                oldCurrent = current;
            }

            return result;
        }

        private void Error(string p)
        {
            throw new Exception(p + Environment.NewLine + " at position " + current);
        }

        Dictionary<string, KeyPressInfo> specialValues = new Dictionary<string, KeyPressInfo>()
        {
            {"+", new KeyPressInfo(Key.OemPlus, ModifierKeys.Shift)},
            {"^", new KeyPressInfo(Key.D6, ModifierKeys.Shift)},
            {"%", new KeyPressInfo(Key.D5, ModifierKeys.Shift)},
            {"{", new KeyPressInfo(Key.OemOpenBrackets, ModifierKeys.Shift)},
            {"}", new KeyPressInfo(Key.Oem6, ModifierKeys.Shift)},
            {"[", new KeyPressInfo(Key.OemOpenBrackets)},
            {"]", new KeyPressInfo(Key.Oem6)},
            {"(", new KeyPressInfo(Key.D9, ModifierKeys.Shift)},
            {")", new KeyPressInfo(Key.D0, ModifierKeys.Shift)},
            {"~", new KeyPressInfo(Key.Oem3, ModifierKeys.Shift)},
            {"BACKSPACE", new KeyPressInfo(Key.Back)},
            {"BS", new KeyPressInfo(Key.Back)},
            {"BKSP", new KeyPressInfo(Key.Back)},
            {"CAPSLOCK", new KeyPressInfo(Key.CapsLock)},
            {"DEL", new KeyPressInfo(Key.Delete)},
            {"DELETE", new KeyPressInfo(Key.Delete)},
            {"DOWN", new KeyPressInfo(Key.Down)},
            {"END", new KeyPressInfo(Key.End)},
            {"ENTER", new KeyPressInfo(Key.Enter)},
            {"ESC", new KeyPressInfo(Key.Escape)},
            {"HELP", new KeyPressInfo(Key.Help)},
            {"HOME", new KeyPressInfo(Key.Home)},
            {"INSERT", new KeyPressInfo(Key.Insert)},
            {"INS", new KeyPressInfo(Key.Insert)},
            {"LEFT", new KeyPressInfo(Key.Left)},
            {"NUMLOCK", new KeyPressInfo(Key.NumLock)},
            {"PGDN", new KeyPressInfo(Key.PageDown)},
            {"PGUP", new KeyPressInfo(Key.PageUp)},
            {"PRTSC", new KeyPressInfo(Key.PrintScreen)},
            {"RIGHT", new KeyPressInfo(Key.Right)},
            {"SCROLLOCK", new KeyPressInfo(Key.Scroll)},
            {"TAB", new KeyPressInfo(Key.Tab)},
            {"UP", new KeyPressInfo(Key.Up)},
            {"F1", new KeyPressInfo(Key.F1)},
            {"F2", new KeyPressInfo(Key.F2)},
            {"F3", new KeyPressInfo(Key.F3)},
            {"F4", new KeyPressInfo(Key.F4)},
            {"F5", new KeyPressInfo(Key.F5)},
            {"F6", new KeyPressInfo(Key.F6)},
            {"F7", new KeyPressInfo(Key.F7)},
            {"F8", new KeyPressInfo(Key.F8)},
            {"F9", new KeyPressInfo(Key.F9)},
            {"F10", new KeyPressInfo(Key.F10)},
            {"F11", new KeyPressInfo(Key.F11)},
            {"F12", new KeyPressInfo(Key.F12)},
            {"F13", new KeyPressInfo(Key.F13)},
            {"F14", new KeyPressInfo(Key.F14)},
            {"F15", new KeyPressInfo(Key.F15)},
            {"F16", new KeyPressInfo(Key.F16)},
        };

        private void ParseCurly()
        {
            foreach (var specialValue in specialValues)
            {
                var name = specialValue.Key;
                if (current + name.Length + 2 > text.Length)
                {
                    continue;
                }
                if (text.Substring(current + 1, name.Length + 1).Equals(name + "}", StringComparison.OrdinalIgnoreCase))
                {
                    current += name.Length + 2;
                    var gesture = specialValue.Value;
                    Add(gesture);
                    return;
                }
            }
            int closing = text.IndexOf('}', current);
            if (closing == -1)
            {
                Error("Closing curly missing");
            }
            Error("Unknown key: " + text.Substring(current, closing - current + 1));
        }

        private void Add(KeyPressInfo gesture)
        {
            if (currentModifiers != ModifierKeys.None)
            {
                gesture = new KeyPressInfo(gesture.Key, gesture.Modifiers | currentModifiers);
            }
            result.Add(gesture);
        }

        private void Add(Key key)
        {
            Add(new KeyPressInfo(key));
        }

        private void ParseChar()
        {
            var key = Key.None;
            var modifiers = ModifierKeys.None;

            var ch = CurrentChar.ToString();

            KeyPressInfo knownKeyPress = KeyboardLayout.Instance.GetKeyGestureForChar(ch[0]);
            if (knownKeyPress != null)
            {
                key = knownKeyPress.Key;
                modifiers = knownKeyPress.Modifiers;
            }
            else
            {
                if (char.IsUpper(ch, 0))
                {
                    ch = ch.ToLower();
                    modifiers = ModifierKeys.Shift;
                }
                key = (Key)new KeyConverter().ConvertFromInvariantString(ch);
            }

            if (key != Key.None)
            {
                Add(new KeyPressInfo(key, modifiers));
                current++;
            }
        }

        private bool IsPrintableChar()
        {
            return char.IsLetterOrDigit(CurrentChar)
                || KeyboardLayout.Instance.GetKeyGestureForChar(CurrentChar) != null;
        }

        private char CurrentChar
        {
            get
            {
                return text[current];
            }
        }
    }
}
