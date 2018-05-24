using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FindMyWindow
{ 
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern int SwitchToThisWindow(IntPtr hWnd);

        IntPtr targetWindow;
        enum State { TextEmpty, NoWindowsFound, FoundMultipleWindows, FoundWindow };
        State state = State.TextEmpty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && state == State.FoundWindow)
            {
                SwitchToThisWindow(targetWindow);
                textBox.Text = "";
            } else if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Process[] processlist = Process.GetProcesses();
            List<String> matchedWindows = new List<String>();

            if (String.IsNullOrEmpty(textBox.Text))
            {
                textBox.Background = Brushes.White;
                state = State.TextEmpty;
                textBlock.Text = "";
                return;
            }

            foreach (Process process in processlist)
            {
                if (process.Id == Process.GetCurrentProcess().Id)
                {
                    continue;
                }

                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (process.MainWindowTitle.ToLower().Contains(textBox.Text.ToLower()))
                    {
                        matchedWindows.Add(process.MainWindowTitle);
                        targetWindow = process.MainWindowHandle;
                    }
                }
            }

            switch(matchedWindows.Count)
            {
                case 0:
                    textBox.Background = Brushes.Red;
                    state = State.NoWindowsFound;
                    textBlock.Text = "No windows found.";
                    break;

                case 1:
                    textBox.Background = Brushes.LightGreen;
                    textBlock.Text = "";
                    textBlock.Inlines.Add(new Bold(new Run(matchedWindows[0])));
                    state = State.FoundWindow;
                    break;

                default:
                    textBox.Background = Brushes.Yellow;
                    textBlock.Text = String.Join(", ", matchedWindows);
                    state = State.FoundMultipleWindows;
                    break;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
