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
using System.Windows.Shapes;


namespace Worksly
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public static RoutedCommand saveSettings = new RoutedCommand();
        public static RoutedCommand closeApp = new RoutedCommand();
        private readonly TimeSpan scheduledTime = Settings1.Default.EODTime;
        private readonly bool EODHardMode = Settings1.Default.EODHardMode;
        private readonly DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        const string timerMessage = "Next scheduled end of day pop-up: ";
        SolidColorBrush disabledText = new SolidColorBrush(Color.FromRgb(112, 112, 112));

        public Settings()
        {
            InitializeComponent();

            CommandBinding cb = new CommandBinding(saveSettings, SaveExecuted, SaveCanExecute);
            this.CommandBindings.Add(cb);
            CommandBinding cb1 = new CommandBinding(closeApp, CloseExecuted, CloseCanExecute);
            this.CommandBindings.Add(cb1);

            saveButton.Command = saveSettings;
            CloseButton.Command = closeApp;

            KeyGesture kg = new KeyGesture(Key.Enter);
            InputBinding ib = new InputBinding(saveSettings, kg);
            this.InputBindings.Add(ib);

            KeyGesture kg1 = new KeyGesture(Key.Escape);
            InputBinding ib1 = new InputBinding(closeApp, kg1);
            this.InputBindings.Add(ib1);
            LoadSettings();
            
        }

        private void LoadSettings()
        {
            string nextTimerText;
            if ( App.timer.Enabled == true)
            {
                DateTime nextTimer = HelperTags.NextTimerEvent(App.timer);
                nextTimerText = nextTimer.ToString("dd.MM.yyyy HH:mm");
            }
            else { nextTimerText = "N/A"; }
            
            feedbackSavePathTB.Text = Settings1.Default.feedbackFolder;
            TvFToggle.IsChecked = Settings1.Default.enableFBTasks;
            fbToggleString.Text = Settings1.Default.feedbackTag;
            fuToggleString.Text = Settings1.Default.followUpTag;
            fbTABCheck.IsChecked = Settings1.Default.fbModeRequireTab;
            taskSavePathTB.Text = Settings1.Default.tasksFolder;
            followupSavePathTB.Text = Settings1.Default.fuFolder;
            TvFToggleChecked();        
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            this.Close();
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Settings1.Default.enableFBTasks = (bool)TvFToggle.IsChecked;
            if (fbToggleString.Text.Length > 0) { Settings1.Default.feedbackTag = fbToggleString.Text; }
            if (fuToggleString.Text.Length > 0) { Settings1.Default.followUpTag = fuToggleString.Text; }
            Settings1.Default.fbModeRequireTab = (bool)fbTABCheck.IsChecked;
            Settings1.Default.Save();
            HelperTags.StopTimer(App.timer);
            HelperTags.Schedule_Timer(App.timer);
            this.Close();
        }

        private void winsLocationChange_Click(object sender, RoutedEventArgs e)
        {
            ChangeWinsLocation();
            LoadSettings();
            saveButton.Focus();
        }

        private void feedbackLocationChange_Click(object sender, RoutedEventArgs e)
        {
            OutlookFolder window = new OutlookFolder(3); // 1=tasks, 2=follow up, 3=feedback
            window.Closed += new EventHandler(window_Closed);
            window.ShowDialog();
            saveButton.Focus();
        }
        void window_Closed(object sender, EventArgs e)
        {
            LoadSettings();
        }
        public static void ChangeWinsLocation()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            if (Settings1.Default.winsSavePath.Length > 0)
            {
                dlg.FileName = Settings1.Default.winsSaveFile;
                dlg.InitialDirectory = Settings1.Default.winsSavePath;
            }
            else { dlg.FileName = "Personal Wins"; }
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            dlg.Title = "Daily Wins Save Location";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.SafeFileName;
                string fullName = dlg.FileName;
                int fileNameLen = fileName.Length;
                Settings1.Default.winsSaveFile = fileName;
                Settings1.Default.winsSavePath = fullName.Remove(fullName.Length - fileNameLen, fileNameLen);
                Settings1.Default.Save();
            }
        }

        private void TvFToggle_Click(object sender, RoutedEventArgs e)
        {
            TvFToggleChecked();
        }
        private void TvFToggleChecked()
        {
            if (TvFToggle.IsChecked == true)
            {
                toggleSetText.Foreground = Brushes.White;
                fbToggleString.IsEnabled = true;
                fuToggleString.IsEnabled = true;
                fbTABCheckLabel.Foreground = Brushes.White;
                fbTABCheck.IsEnabled = true;
                fbSaveLabel.Foreground = Brushes.White;
                toggleFUSetText.Foreground = Brushes.White;
                feedbackLocationChange.IsEnabled = true;
                fuSaveLabel.Foreground = Brushes.White;
                followupLocationChange.IsEnabled = true;
            }
            else
            {
                toggleSetText.Foreground = disabledText;
                toggleFUSetText.Foreground = disabledText;
                fbToggleString.IsEnabled = false;
                fuToggleString.IsEnabled = false;
                fbToggleString.Text = Settings1.Default.feedbackTag;
                fuToggleString.Text = Settings1.Default.followUpTag;
                fbTABCheckLabel.Foreground = disabledText;
                fbTABCheck.IsEnabled = false;
                fbTABCheck.IsChecked = Settings1.Default.fbModeRequireTab;
                feedbackLocationChange.IsEnabled = false;
                fbSaveLabel.Foreground = disabledText;
                fuSaveLabel.Foreground = disabledText;
                followupLocationChange.IsEnabled = false;
            }
        }

        private void followupLocationChange_Click(object sender, RoutedEventArgs e)
        {
            OutlookFolder window = new OutlookFolder(2); // 1=tasks, 2=follow up, 3=feedback
            window.Closed += new EventHandler(window_Closed);
            window.ShowDialog();
            saveButton.Focus();
        }

        private void taskLocationChange_Click(object sender, RoutedEventArgs e)
        {
            OutlookFolder window = new OutlookFolder(1); // 1=tasks, 2=follow up, 3=feedback
            window.Closed += new EventHandler(window_Closed);
            window.ShowDialog();
            saveButton.Focus();
        }
    }
}
