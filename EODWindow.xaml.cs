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
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for EODWindow.xaml
    /// </summary>
    public partial class EODWindow : Window
    {
        public static RoutedCommand taskSubmit = new RoutedCommand();
        public static RoutedCommand yesClick = new RoutedCommand();
        public static RoutedCommand noClick = new RoutedCommand();
        public EODWindow()
        {
            
            InitializeComponent();

            CommandBinding cb = new CommandBinding(taskSubmit, SubmitExecuted, SubmitCanExecute);
            this.CommandBindings.Add(cb);

            submitButton.Command = taskSubmit;

            CommandBinding cb1 = new CommandBinding(yesClick, YesExecuted, YesCanExecute);
            this.CommandBindings.Add(cb1);

            warningYes.Command = yesClick;

            CommandBinding cb2 = new CommandBinding(noClick, NoExecuted, NoCanExecute);
            this.CommandBindings.Add(cb2);

            warningNo.Command = noClick;

            if (!Settings1.Default.EODHardMode) { CloseButton.Visibility = Visibility.Visible; }
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            selfWins.Focus();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void SubmitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (selfWins.Text.Length > 10) { e.CanExecute = true; }
        }

        private void SubmitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            
            submitButton.Style = (Style)Application.Current.Resources["submitBtnPressed"];
            if (feedback.Text.Length == 0)
            {
                warningMask.Visibility = Visibility.Visible;
                warningMessage.Visibility = Visibility.Visible;
                warningLabel.Visibility = Visibility.Visible;
                warningYes.Visibility = Visibility.Visible;
                warningNo.Visibility = Visibility.Visible;
            }
            else { TextProcessing(); }
            submitButton.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
        }
        private void YesCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
           e.CanExecute = true;
        }

        private void YesExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            warningYes.Style = (Style)Application.Current.Resources["submitBtnPressed"];
            warningMask.Visibility = Visibility.Hidden;
            warningMessage.Visibility = Visibility.Hidden;
            warningLabel.Visibility = Visibility.Hidden;
            warningYes.Visibility = Visibility.Hidden;
            warningNo.Visibility = Visibility.Hidden;
            TextProcessing();
            warningYes.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
        }
        private void NoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NoExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            warningNo.Style = (Style)Application.Current.Resources["submitBtnPressed"];
            warningMask.Visibility = Visibility.Hidden;
            warningMessage.Visibility = Visibility.Hidden;
            warningLabel.Visibility = Visibility.Hidden;
            warningYes.Visibility = Visibility.Hidden;
            warningNo.Visibility = Visibility.Hidden;
            warningNo.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
        }
        private void TextProcessing()
        {
            string winsText = selfWins.Text;
            string feedbackText = feedback.Text;
            SaveWins(winsText);
            LogFeedback(feedbackText);
            selfWins.Text = null;
            feedback.Text = null;
            this.Close();
        }
        private void LogFeedback(string inputStr)
        {
            if (inputStr.Length == 0) { return; }
            while (Settings1.Default.feedbackFolder.Length == 0)
            {
                OutlookFolder window = new OutlookFolder();
                window.ShowDialog();
            }
            string[] feedbackArry = inputStr.Split('\n');
            string feedbackSubject;
            foreach (string i in feedbackArry)
            {
                feedbackSubject = i + " - " + DateTime.Now.ToString("dd/MM/yyy");
                HelperTags.CreateTask(feedbackSubject,true);
            }
        }
        private void SaveWins(string inputStr)
        {
            inputStr = inputStr.Replace("\r", "");
            string[] inputArry = inputStr.Split('\n');
            string FileName = Settings1.Default.winsSavePath + Settings1.Default.winsSaveFile;
            if (FileName.Length == 0)
            {
                Settings.ChangeWinsLocation();
                FileName = Settings1.Default.winsSavePath + Settings1.Default.winsSaveFile;
            }
            StreamWriter sw = File.AppendText(FileName);
            foreach (string i in inputArry)
            {
                sw.WriteLine((DateTime.Now.ToString("yyyy.MM.dd") + " - " + i));
                
            }
            sw.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            selfWins.Text = null;
            feedback.Text = null;
            this.Close();
        }
    }
}
