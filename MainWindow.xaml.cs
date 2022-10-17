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
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Timers;

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public static RoutedCommand taskSubmit = new RoutedCommand();
        public static RoutedCommand closeApp = new RoutedCommand();
        public static RoutedCommand lookup = new RoutedCommand();
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

        private bool followUpTask = false;
        private bool lookupMode = false;
        private bool userFound = false;
        private bool apptCreation = true;

        Timer timer = new Timer(500);
        private string searchText = "";

        public MainWindow()
        {

            InitializeComponent();

            timer.AutoReset = false;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);


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

            this.PreviewKeyDown += MWPreviewKeyDown;

            taskEntry.Focus();
        }

        private void MWPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(taskEntry.IsFocused && e.Key == Key.Tab && userFound && lookupMode)
            {
                taskEntry.Text += " - ";
                taskEntry.CaretIndex = taskEntry.Text.Length;
                lookupMode = false;
                userFound = false;
                e.Handled = true;
            }
            if ((taskEntry.IsFocused == true) & (e.Key == Key.Tab) & (taskEntry.Text == Settings1.Default.feedbackTag) && !lookupMode)
            {
                EnableFBMode();
                e.Handled = true;
            }
            if ((taskEntry.IsFocused == true) & (e.Key == Key.Tab) & (taskEntry.Text == Settings1.Default.followUpTag))
            {
                taskEntry.Text = "F/U: ";
                taskEntry.CaretIndex = 5;
                followUpTask = true;
                e.Handled = true;
            }
            if ((taskEntry.Text.Length == 0) & (fbIcon.Visibility==Visibility.Visible) & (e.Key == Key.Escape))
            {
                DisableFBMode();
                e.Handled = true;
            }
            if ((taskEntry.Text == "F/U: ") && (e.Key == Key.Escape))
            {
                taskEntry.Text = "";
                followUpTask = false;
                e.Handled = true;
            }
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
            sdEntry.Text = RoundDown(DateTime.Now, TimeSpan.FromMinutes(15)).ToString("HH:mm");
            edEntry.Text = RoundDown(DateTime.Now, TimeSpan.FromMinutes(15)).AddMinutes(15).ToString("HH:mm");
            lengthEntry.Text = "0.25";
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
            if (taskEntry.Text.Length != 0) { 
                if (apptCreation) 
                {
                    //HelperTags.CreateAppt(taskText, );
                }
                else
                {
                    HelperTags.CreateTask(taskText, (fbIcon.Visibility == Visibility.Visible), followUpTask); //if in feedback mode submit as a feedback task
                }
            } 
            //MessageBox.Show(taskText);
            submitButton.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
            taskEntry.Text = null;
            DisableFBMode();
            Application.Current.MainWindow.Hide();
        }

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Application.Current.MainWindow != null;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            taskEntry.Text = null;
            DisableFBMode();
            Application.Current.MainWindow.Hide();
            
        }

        private void EnableFBMode()
        {
            lookupMode = true;
            fbIcon.Visibility = Visibility.Visible;
            taskEntry.Width = 410;
            Canvas.SetLeft(taskEntry, (double)55);
            taskEntry.Text = null;
        }

        private void DisableFBMode()
        {
            lookupMode = false;
            fbIcon.Visibility = Visibility.Hidden;
            taskEntry.Width = 430;
            Canvas.SetLeft(taskEntry, (double)35);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ADLookUP();
        }

        private void ADLookUP()
        {
            int searchLength = searchText.Length;
            if (searchLength < 2) { return; }
            SearchResult user;
            DirectorySearcher searcher = null;
            DirectoryEntry de = new DirectoryEntry("LDAP://epic.com");

            searcher = new DirectorySearcher(de);
            searcher.PropertiesToLoad.Add("name");
            searcher.Filter = "(&(objectCategory=User)(objectClass=person)(name=" + searchText + "*))";

            user = searcher.FindOne();
            if (null != user)
            {
                userFound = true;
                Dispatcher.Invoke(() => {
                    taskEntry.Text = user.Properties["name"][0].ToString();
                    taskEntry.CaretIndex = searchLength;
                });
            }
        }

        private void taskEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Settings1.Default.fbModeRequireTab == false)
            {
                if (taskEntry.Text == Settings1.Default.feedbackTag)
                {
                    EnableFBMode();
                    e.Handled = true;
                    return;
                }
                else if (taskEntry.Text == Settings1.Default.followUpTag)
                {
                    taskEntry.Text = "F/U: ";
                    taskEntry.CaretIndex = 5;
                    followUpTask = true;
                    e.Handled = true;
                }
            }

        }

        private void taskEntry_KeyDown(object sender, KeyEventArgs e)
        {     
            if (userFound && lookupMode)
            {
                string currText = taskEntry.Text;
                int len = taskEntry.CaretIndex;
                currText = currText.Substring(0, len);
                taskEntry.Text = currText;
                taskEntry.CaretIndex = len;
                userFound = false;
            } 
        }

        private void taskEntry_KeyUp(object sender, KeyEventArgs e)
        {
            if (lookupMode)
            {
                searchText = taskEntry.Text;
                timer.Stop();
                timer.Start();
            }
            if (String.IsNullOrWhiteSpace(taskEntry.Text))
            {
                userFound = false;
                followUpTask = false;
                taskEntry.Text = null;
            }
        }

        private void sdEntry_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void edEntry_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void lengthEntry_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private static DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            return new DateTime(dt.Ticks / d.Ticks * d.Ticks, dt.Kind);
        }

    }
}
