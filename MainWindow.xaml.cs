//----------------------------------------------------
// Copyright 2021 Epic Systems Corporation
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand taskSubmit = new RoutedCommand();
        public static RoutedCommand closeApp = new RoutedCommand();
        [DllImportAttribute("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImportAttribute("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        //Space Bar:
        private const uint VK_SPACE = 0x20;

        public MainWindow()
        {

            InitializeComponent();

            CommandBinding cb = new CommandBinding(taskSubmit, SubmitExecuted, SubmitCanExecute);
            this.CommandBindings.Add(cb);
            CommandBinding cb1 = new CommandBinding(closeApp, CloseExecuted, CloseCanExecute);
            this.CommandBindings.Add(cb1);

            submitButton.Command = taskSubmit;
            CloseButton.Command = closeApp;

            KeyGesture kg = new KeyGesture(Key.Enter);
            InputBinding ib = new InputBinding(taskSubmit, kg);
            this.InputBindings.Add(ib);

            KeyGesture kg1 = new KeyGesture(Key.Escape);
            InputBinding ib1 = new InputBinding(closeApp, kg1);
            this.InputBindings.Add(ib1);


            taskEntry.Focus();
        }

        private IntPtr _windowHandle;
        private HwndSource _source;
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_SPACE)
                            {
                                if(Application.Current.MainWindow.IsVisible)
                                {
                                    taskEntry.Text = null;
                                    Application.Current.MainWindow.Hide();
                                }
                                else
                                {
                                    taskEntry.Text = null;
                                    Application.Current.MainWindow.Show();
                                    Application.Current.MainWindow.Activate();
                                    taskEntry.Focus();
                                }
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            taskEntry.Focus();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_SPACE);
            Application.Current.MainWindow.Hide();
        }
        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void SubmitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SubmitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string taskText = taskEntry.Text;
            submitButton.Style = (Style)Application.Current.Resources["submitBtnPressed"];
            if (taskText == ";eod")
            {
                this.Hide();
                EODWindow window1 = new EODWindow();
                window1.Show();
                return;
            }
            if (taskEntry.Text.Length != 0) { EODWindow.CreateTask(taskText, false); }
            //MessageBox.Show(taskText);
            submitButton.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
            taskEntry.Text = null;
            Application.Current.MainWindow.Hide();
        }
        //private void CreateTask(string subject)
        //{
        //    Outlook.ApplicationClass app = new Outlook.ApplicationClass();
        //    Outlook.TaskItem tsk = (Outlook.TaskItem)app.CreateItem(Outlook.OlItemType.olTaskItem);
        //    tsk.Subject = subject;
        //    tsk.Save();
        //}

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Application.Current.MainWindow != null;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            taskEntry.Text = null;
            Application.Current.MainWindow.Hide();
        }
    }
}
