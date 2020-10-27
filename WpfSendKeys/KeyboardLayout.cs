using System.Collections.Generic;

namespace System.Windows.Input.Test
{
    public class KeyboardLayout
    {
        public static readonly KeyboardLayout Instance = new KeyboardLayout();

        public KeyPressInfo GetKeyGestureForChar(char index)
        {
            KeyPressInfo result = null;
            printableChars.TryGetValue(index, out result);
            return result;
        }

        public string GetInputForGesture(KeyPressInfo gesture)
        {
            foreach (var item in printableChars)
            {
                if (item.Value.Key == gesture.Key && item.Value.Modifiers == gesture.Modifiers)
                {
                    return item.Key.ToString();
                }
            }
            return "";
        }

        private static readonly Dictionary<char, KeyPressInfo> printableChars = new Dictionary<char, KeyPressInfo> 
        { 
            {' ', new KeyPressInfo(Key.Space)},
            {',', new KeyPressInfo(Key.OemComma)},
            {'<', new KeyPressInfo(Key.OemComma, ModifierKeys.Shift)},
            {'.', new KeyPressInfo(Key.OemPeriod)},
            {'>', new KeyPressInfo(Key.OemPeriod, ModifierKeys.Shift)},
            {'/', new KeyPressInfo(Key.OemQuestion)},
            {'?', new KeyPressInfo(Key.OemQuestion, ModifierKeys.Shift)},
            {'-', new KeyPressInfo(Key.OemMinus)},
            {'_', new KeyPressInfo(Key.OemMinus, ModifierKeys.Shift)},
            {'=', new KeyPressInfo(Key.OemPlus)},
            {'+', new KeyPressInfo(Key.OemPlus, ModifierKeys.Shift)},
            {'[', new KeyPressInfo(Key.OemOpenBrackets)},
            {'{', new KeyPressInfo(Key.OemOpenBrackets, ModifierKeys.Shift)},
            {']', new KeyPressInfo(Key.Oem6)},
            {'}', new KeyPressInfo(Key.Oem6, ModifierKeys.Shift)},
            {'\\', new KeyPressInfo(Key.Oem5)},
            {'|', new KeyPressInfo(Key.Oem5, ModifierKeys.Shift)},
            {';', new KeyPressInfo(Key.Oem1)},
            {':', new KeyPressInfo(Key.Oem1, ModifierKeys.Shift)},
            {'`', new KeyPressInfo(Key.Oem3)},
            {'~', new KeyPressInfo(Key.Oem3, ModifierKeys.Shift)},
            {'\'', new KeyPressInfo(Key.OemQuotes)},
            {'"', new KeyPressInfo(Key.OemQuotes, ModifierKeys.Shift)},
            {'!', new KeyPressInfo(Key.D1, ModifierKeys.Shift)},
            {'@', new KeyPressInfo(Key.D2, ModifierKeys.Shift)},
            {'#', new KeyPressInfo(Key.D3, ModifierKeys.Shift)},
            {'$', new KeyPressInfo(Key.D4, ModifierKeys.Shift)},
            {'%', new KeyPressInfo(Key.D5, ModifierKeys.Shift)},
            {'^', new KeyPressInfo(Key.D6, ModifierKeys.Shift)},
            {'&', new KeyPressInfo(Key.D7, ModifierKeys.Shift)},
            {'*', new KeyPressInfo(Key.D8, ModifierKeys.Shift)},
            {'(', new KeyPressInfo(Key.D9, ModifierKeys.Shift)},
            {')', new KeyPressInfo(Key.D0, ModifierKeys.Shift)},
        };
    }
}
