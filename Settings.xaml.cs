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

            DateTime nextTimer = HelperTags.NextTimerEvent();
            EODHardMode_Box.IsChecked = EODHardMode;
            EODscheduledTime.Value = today.Add(scheduledTime);
            nextScheduledTimer.Text = timerMessage + nextTimer.ToString("dd.MM.yyyy HH:mm");
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
            Settings1.Default.Save();
            HelperTags.StopTimer();
            HelperTags.Schedule_Timer();
            this.Close();
        }
    }
}
