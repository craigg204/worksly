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

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for EODWindow.xaml
    /// </summary>
    public partial class EODWindow : Window
    {
        public static RoutedCommand taskSubmit = new RoutedCommand();
        public EODWindow()
        {
            InitializeComponent();

            CommandBinding cb = new CommandBinding(taskSubmit, SubmitExecuted, SubmitCanExecute);
            this.CommandBindings.Add(cb);

            KeyGesture kg = new KeyGesture(Key.Enter);
            InputBinding ib = new InputBinding(taskSubmit, kg);
            this.InputBindings.Add(ib);

            submitButton.Command = taskSubmit;
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            selfWins.Focus();
        }
        private void SubmitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (selfWins.Text.Length != 0) { e.CanExecute = true; }
        }

        private void SubmitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string winsText = selfWins.Text;
            submitButton.Style = (Style)Application.Current.Resources["submitBtnPressed"];
            //if (selfWins.Text.Length != 0) { CreateTask(winsText); }
            //MessageBox.Show(taskText);
            submitButton.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
            selfWins.Text = null;
            this.Close();
        }
        private void CreateTask(string subject)
        {
            Outlook.ApplicationClass app = new Outlook.ApplicationClass();
            Outlook.TaskItem tsk = (Outlook.TaskItem)app.CreateItem(Outlook.OlItemType.olTaskItem);
            tsk.Subject = subject;
            tsk.Save();
        }
    }
}
