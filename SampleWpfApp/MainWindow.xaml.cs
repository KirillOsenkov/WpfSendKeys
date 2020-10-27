using System;
using System.Threading;
using System.Windows;
using System.Windows.Input.Test;
using System.Windows.Threading;
using System.Windows.Input;

namespace SampleWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                textbox1.Clear();
            }
        }

        void SendToUIThread(UIElement element, string text)
        {
            element.Dispatcher.BeginInvoke(new Action(() =>
            {
                SendKeys.Send(element, text);
            }), DispatcherPriority.Input);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // run this on a background thread to not block the main window's message loop
            ThreadPool.QueueUserWorkItem(_ =>
            {
                // post from the background thread to the UI thread
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    textbox1.Focus();
                }), DispatcherPriority.Input);

                SendToUIThread(textbox1, "Hello World!");

                // let the background thread sleep a little to let the UI display the text
                // and to let the user see it
                Thread.Sleep(2000);

                // now send Ctrl+Z <-- this is what you couldn't mock before
                SendToUIThread(this, "^z");

                Thread.Sleep(2000);

                // again post this on the UI thread to send the click to the button
                SendToUIThread(textbox1, "{TAB}");
                SendToUIThread(OKButton, "{ENTER}");
            });
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
