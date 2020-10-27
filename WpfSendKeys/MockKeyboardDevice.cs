using System.Reflection;

namespace System.Windows.Input.Test
{
    /// <summary>
    /// This serves to mock the state of the modifier keys Shift, Alt and Ctrl.
    /// We pass this keyboard device to KeyEventArgs, so that whenever the user
    /// says args.KeyboardDevice.Modifiers, we can provide our own Modifiers value
    /// instead of going to the operating system and fetching the actual key states
    /// from the driver (normally done by KeyboardDevice and Win32KeyboardDevice).
    /// </summary>
    /// <remarks>
    /// An unfortunate side effect of creating our own derived KeyboardDevice is
    /// that we'll have to call the KeyboardDevice's protected ctor, which does
    /// a lot of damage by subscribing itself to a whole bunch of static events.
    /// Not only will these events prevent our MockKeyboardDevice from ever being 
    /// collected, but also every instance of us will start "typing" whenever real
    /// input from the user comes into the system. For example, by merely creating 
    /// an instance of MockKeyboardDevice, we achieve the effect that when the user
    /// types a key such as w, it will be duplicated and all the events will be called
    /// twice, resulting in a 'ww' on the screen.
    /// </remarks>
    internal class MockKeyboardDevice : KeyboardDevice
    {
        /// <summary>
        /// Fake state of the Shift, Alt and Ctrl keys desired by the test
        /// </summary>
        public new ModifierKeys Modifiers { get; set; }

        private static MockKeyboardDevice mInstance;
        public static MockKeyboardDevice Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new MockKeyboardDevice();
                }
                return mInstance;
            }
        }

        /// <summary>
        /// To prevent contaminating the system with test side effects
        /// (basically, to prevent our MockKeyboardDevice from listening and reacting to real
        /// keyboard input), we need to undo the damage done by the 
        /// base KeyboardDevice's constructor where it subscribes to some events.
        /// By unsubscribing from these events we make sure our KeyboardDevice
        /// won't respond to real input and will be used solely to provide the
        /// desired state of the Shift, Alt and Ctrl modifier keys when asked for.
        /// </summary>
        private MockKeyboardDevice()
            : base(InputManager.Current)
        {
            // There's only one global input manager in the system
            InputManager inputManager = InputManager.Current;

            // First, unsubscribe our 3 handlers from the InputManager.

            // 1) 
            // Undo the effect of the following line in the KeyboardDevice ctor:
            // this._inputManager.Value.PreProcessInput += new PreProcessInputEventHandler(this.PreProcessInput);
            PreProcessInputEventHandler preProcessInputHandler =
                GetDelegateForMethod<PreProcessInputEventHandler>(this, "PreProcessInput");
            inputManager.PreProcessInput -= preProcessInputHandler;

            // 2) 
            // Undo the effect of the following line in the KeyboardDevice ctor:
            // this._inputManager.Value.PreNotifyInput += new NotifyInputEventHandler(this.PreNotifyInput);
            NotifyInputEventHandler preNotifyInputHandler =
                GetDelegateForMethod<NotifyInputEventHandler>(this, "PreNotifyInput");
            inputManager.PreNotifyInput -= preNotifyInputHandler;

            // 3) 
            // Undo the effect of the following line in the KeyboardDevice ctor:
            // this._inputManager.Value.PostProcessInput += new ProcessInputEventHandler(this.PostProcessInput);
            ProcessInputEventHandler postProcessInputHandler =
                GetDelegateForMethod<ProcessInputEventHandler>(this, "PostProcessInput");
            inputManager.PostProcessInput -= postProcessInputHandler;

            // Now undo the damage done by creating a new TextServicesManager

            // this._TsfManager = new SecurityCriticalDataClass<TextServicesManager>(new TextServicesManager(inputManager));
            var textServicesManager = this.GetFieldValue("_TsfManager").GetFieldValue("_value");

            // this._inputManager.PreProcessInput += new PreProcessInputEventHandler(this.PreProcessInput);
            preProcessInputHandler = GetDelegateForMethod<PreProcessInputEventHandler>(textServicesManager, "PreProcessInput");
            inputManager.PreProcessInput -= preProcessInputHandler;

            // this._inputManager.PostProcessInput += new ProcessInputEventHandler(this.PostProcessInput);
            postProcessInputHandler = GetDelegateForMethod<ProcessInputEventHandler>(textServicesManager, "PostProcessInput");
            inputManager.PostProcessInput -= postProcessInputHandler;

            // And finally, revert the changes done by creating a new TextCompositionManager

            // this._textcompositionManager = new SecurityCriticalData<TextCompositionManager>(new TextCompositionManager(inputManager));
            var textCompositionManager = this.GetFieldValue("_textcompositionManager").GetFieldValue("_value");

            // this._inputManager.PreProcessInput += new PreProcessInputEventHandler(this.PreProcessInput);
            preProcessInputHandler = GetDelegateForMethod<PreProcessInputEventHandler>(textCompositionManager, "PreProcessInput");
            inputManager.PreProcessInput -= preProcessInputHandler;

            // his._inputManager.PostProcessInput += new ProcessInputEventHandler(this.PostProcessInput);
            postProcessInputHandler = GetDelegateForMethod<ProcessInputEventHandler>(textCompositionManager, "PostProcessInput");
            inputManager.PostProcessInput -= postProcessInputHandler;
        }

        T GetDelegateForMethod<T>(object target, string methodName)
        {
            object result = Delegate.CreateDelegate(typeof(T), target, methodName);
            return (T)result;
        }

        protected override KeyStates GetKeyStatesFromSystem(Key key)
        {
            return GetKeyStatesCore(key, this.Modifiers);
        }

        KeyStates GetKeyStatesCore(Key key, ModifierKeys Modifiers)
        {
            switch (key)
            {
                case Key.LeftAlt:
                case Key.RightAlt:
                    return (Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt ? KeyStates.Down : KeyStates.None;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return (Modifiers & ModifierKeys.Control) == ModifierKeys.Control ? KeyStates.Down : KeyStates.None;
                case Key.LeftShift:
                case Key.RightShift:
                    return (Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? KeyStates.Down : KeyStates.None;
            }

            return KeyStates.None;
        }
    }

    internal static class ReflectionExtensions
    {
        public static object GetFieldValue(this object instance, string fieldName)
        {
            Type type = instance.GetType();
            FieldInfo fieldInfo = null;
            while (type != null)
            {
                fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    break;
                }
                type = type.BaseType;
            }

            if (fieldInfo == null)
            {
                throw new FieldAccessException("Field " + fieldName + " was not found on type " + type.ToString());
            }
            object result = fieldInfo.GetValue(instance);
            return result; // you can place a breakpoint here (for debugging purposes)
        }
    }
}
