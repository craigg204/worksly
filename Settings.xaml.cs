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


namespace TaskMaster
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
            
            EODHardMode_Box.IsChecked = EODHardMode;
            EODscheduledTime.Value = today.Add(scheduledTime);
            nextScheduledTimer.Text = timerMessage + nextTimerText;
            winsSavePathTB.Text = Settings1.Default.winsSavePath + Settings1.Default.winsSaveFile;
            feedbackSavePathTB.Text = Settings1.Default.feedbackFolder;
            TvFToggle.IsChecked = Settings1.Default.enableFBTasks;
            fbToggleString.Text = Settings1.Default.feedbackTag;
            fbTABCheck.IsChecked = Settings1.Default.fbModeRequireTab;
            enableEODCheck.IsChecked = Settings1.Default.enableEOD;
            TvFToggleChecked();
            EnableEODClicked();            
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
            Settings1.Default.EODHardMode = (bool)EODHardMode_Box.IsChecked;
            Settings1.Default.EODTime = (TimeSpan)(EODscheduledTime.Value - today);
            Settings1.Default.enableEOD = (bool)enableEODCheck.IsChecked;
            Settings1.Default.enableFBTasks = (bool)TvFToggle.IsChecked;
            if (fbToggleString.Text.Length > 0) { Settings1.Default.feedbackTag = fbToggleString.Text; }
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
            OutlookFolder window = new OutlookFolder();
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
                fbTABCheckLabel.Foreground = Brushes.White;
                fbTABCheck.IsEnabled = true;
                fbSaveLabel.Foreground = Brushes.White;
                feedbackLocationChange.IsEnabled = true;
            }
            else
            {
                toggleSetText.Foreground = disabledText;
                fbToggleString.IsEnabled = false;
                fbToggleString.Text = Settings1.Default.feedbackTag;
                fbTABCheckLabel.Foreground = disabledText;
                fbTABCheck.IsEnabled = false;
                fbTABCheck.IsChecked = Settings1.Default.fbModeRequireTab;
                if (enableEODCheck.IsChecked == false)
                {
                    feedbackLocationChange.IsEnabled = false;
                    fbSaveLabel.Foreground = disabledText;
                }

            }
        }
        private void EnableEODCheck_Click(object sender, RoutedEventArgs e)
        {
            EnableEODClicked();
        }
        private void EnableEODClicked()
        {
            if (enableEODCheck.IsChecked == true)
            {
                hardmodeLabel.Foreground = Brushes.White;
                EODHardMode_Box.IsEnabled = true;
                EODTimeLabel.Foreground = Brushes.White;
                EODscheduledTime.IsEnabled = true;
                WinsLabel.Foreground = Brushes.White;
                winsLocationChange.IsEnabled = true;
                fbSaveLabel.Foreground = Brushes.White;
                feedbackLocationChange.IsEnabled = true;
            }
            else
            {
                hardmodeLabel.Foreground = disabledText;
                EODHardMode_Box.IsEnabled = false;
                EODHardMode_Box.IsChecked = Settings1.Default.EODHardMode;
                EODTimeLabel.Foreground = disabledText;
                EODscheduledTime.IsEnabled = false;
                WinsLabel.Foreground = disabledText;
                winsLocationChange.IsEnabled = false;
                if (TvFToggle.IsChecked == false)
                {
                    feedbackLocationChange.IsEnabled = false;
                    fbSaveLabel.Foreground = disabledText;
                }

            }
        }
    }
}
