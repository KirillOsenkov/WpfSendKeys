namespace System.Windows.Input.Test
{
    public class SendKeys
    {
        public static void Send(UIElement element, string text)
        {
            var sequence = SendKeysParser.Parse(text);
            foreach (var keyPressInfo in sequence)
            {
                Send(element, keyPressInfo.Key, keyPressInfo.Modifiers);
            }
        }

        public static void Send(UIElement element, Key key, ModifierKeys modifiers)
        {
            KeyboardDevice keyboardDevice = InputManager.Current.PrimaryKeyboardDevice;
            if (modifiers != ModifierKeys.None)
            {
                MockKeyboardDevice mockKeyboardDevice = MockKeyboardDevice.Instance;
                mockKeyboardDevice.Modifiers = modifiers;
                keyboardDevice = mockKeyboardDevice;
            }
            RaiseKeyEvent(element, key, modifiers, keyboardDevice);
        }

        public static void SendInput(UIElement element, string text)
        {
            InputManager inputManager = InputManager.Current;
            InputDevice inputDevice = inputManager.PrimaryKeyboardDevice;
            TextComposition composition = new TextComposition(inputManager, element, text);
            TextCompositionEventArgs args = new TextCompositionEventArgs(inputDevice, composition);
            args.RoutedEvent = UIElement.PreviewTextInputEvent;
            element.RaiseEvent(args);
            args.RoutedEvent = UIElement.TextInputEvent;
            element.RaiseEvent(args);
        }

        private static void RaiseKeyEvent(UIElement element, Key key, ModifierKeys modifiers, KeyboardDevice keyboardDevice)
        {
            PresentationSource presentationSource = PresentationSource.FromVisual(element);
            int timestamp = Environment.TickCount;
            KeyEventArgs args = new KeyEventArgs(keyboardDevice, presentationSource, timestamp, key);

            // 1) PreviewKeyDown
            args.RoutedEvent = Keyboard.PreviewKeyDownEvent;
            element.RaiseEvent(args);

            // 2) KeyDown
            args.RoutedEvent = Keyboard.KeyDownEvent;
            element.RaiseEvent(args);

            // 3) TextInput
            SendInputIfNecessary(element, key, modifiers, keyboardDevice);

            // 4) PreviewKeyUp
            args.RoutedEvent = Keyboard.PreviewKeyUpEvent;
            element.RaiseEvent(args);

            // 5) KeyUp
            args.RoutedEvent = Keyboard.KeyUpEvent;
            element.RaiseEvent(args);
        }

        private static void SendInputIfNecessary(UIElement element, Key key, ModifierKeys modifiers, KeyboardDevice keyboardDevice)
        {
            if (modifiers.HasFlag(ModifierKeys.Control) || modifiers.HasFlag(ModifierKeys.Alt))
            {
                return;
            }

            string input = "";

            input = KeyboardLayout.Instance.GetInputForGesture(new KeyPressInfo(key, modifiers));
            if (input == "")
            {
                input = GetInputFromKey(key);
            }

            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            if (modifiers == ModifierKeys.Shift)
            {
                input = input.ToUpperInvariant();
            }
            else
            {
                input = input.ToLowerInvariant();
            }

            SendInput(element, input);
        }

        private static string GetInputFromKey(Key key)
        {
            switch (key)
            {
                case Key.Left:
                case Key.Up:
                case Key.Right:
                case Key.Down:
                case Key.Home:
                case Key.End:
                case Key.Insert:
                case Key.Delete:
                case Key.PageUp:
                case Key.PageDown:
                case Key.Back:
                case Key.Escape:
                case Key.Enter:
                case Key.Tab:
                case Key.Space:
                    return "";
            }
            return new KeyConverter().ConvertToInvariantString(key);
        }
    }
}
