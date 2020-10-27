namespace System.Windows.Input.Test
{
    public class KeyPressInfo
    {
        public KeyPressInfo(Key key)
        {
            this.Key = key;
            this.Modifiers = ModifierKeys.None;
        }

        public KeyPressInfo(Key key, ModifierKeys modifierKeys)
        {
            this.Key = key;
            this.Modifiers = modifierKeys;
        }

        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }

        public override string ToString()
        {
            var result = Key.ToString();
            if (Modifiers != ModifierKeys.None)
            {
                result = Modifiers.ToString() + " + " + result;
            }
            return result;
        }
    }
}