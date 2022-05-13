//----------------------------------------------------
// Copyright 2021 Epic Systems Corporation
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for OutlookFolder.xaml
    /// </summary>
    public partial class OutlookFolder : Window
    {
        public static RoutedCommand taskSubmit = new RoutedCommand();
        public static RoutedCommand closeApp = new RoutedCommand();

        public OutlookFolder(int type) // 1=tasks, 2=follow up, 3=feedback
        {
            InitializeComponent();
            CreateFolderGrid();
            CommandBinding cb = new CommandBinding(taskSubmit, SubmitExecuted, SubmitCanExecute);
            this.CommandBindings.Add(cb);
            CommandBinding cb1 = new CommandBinding(closeApp, CloseExecuted, CloseCanExecute);
            this.CommandBindings.Add(cb1);

            submitButton.Command = taskSubmit;
            submitButton.CommandParameter = type;
            CloseButton.Command = closeApp;

            KeyGesture kg = new KeyGesture(Key.Enter);
            InputBinding ib = new InputBinding(taskSubmit, kg);
            this.InputBindings.Add(ib);

            KeyGesture kg1 = new KeyGesture(Key.Escape);
            InputBinding ib1 = new InputBinding(closeApp, kg1);
            this.InputBindings.Add(ib1);
            foldersListBox.SelectedIndex = 0;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void CreateFolderGrid()
        {
            Outlook.ApplicationClass oApp = new Outlook.ApplicationClass();

            Outlook.MAPIFolder oDefaultTaskFolder = oApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderTasks);
            if (oDefaultTaskFolder != null)
            {
                List<FolderListing> folderList = new List<FolderListing>();
                FolderListing folder;
                int cnt = 0;
                folder = new FolderListing(oDefaultTaskFolder.Name, "/Icons/lineArrow-White.png", oDefaultTaskFolder.FolderPath);
                folderList.Add(folder);
                foreach (Outlook.MAPIFolder subfolder in oDefaultTaskFolder.Folders) 
                {
                    cnt ++;
                    if (cnt == oDefaultTaskFolder.Folders.Count)
                    {
                        folder = new FolderListing(subfolder.Name, "/Icons/endArrow-White.png", subfolder.FolderPath);
                    }
                    else
                    {
                        folder = new FolderListing(subfolder.Name, "/Icons/lineArrow-White.png", subfolder.FolderPath);
                    }
                    
                    folderList.Add(folder);
                }
                foldersListBox.ItemsSource = folderList;
            }
        }
        private void SubmitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (foldersListBox.SelectedItem != null);
        }

        private void SubmitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            FolderListing selFolder = (FolderListing)foldersListBox.SelectedItem;
            string selString = selFolder.FolderPath;
#if DEBUG
            Console.WriteLine(selString);
#endif
            submitButton.Style = (Style)Application.Current.Resources["submitBtnPressed"];
            switch (e.Parameter) // 1=tasks, 2=follow up, 3=feedback
            {
                case 2: Settings1.Default.fuFolder = selString; break;
                case 3: Settings1.Default.feedbackFolder = selString; break;
                default: Settings1.Default.tasksFolder = selString; break;
            }
            Settings1.Default.Save();
            submitButton.Style = (Style)Application.Current.Resources["submitBtn"];
            e.Handled = true;
            Close();
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

        private void foldersListBox_Selected(object sender, RoutedEventArgs e)
        {
            submitButton.Focus();
        }
    }
    class FolderListing
    {
        public string FolderName { get; set; }
        public string IconName { get; set; }
        public string FolderPath { get; set; }
        public FolderListing(string folderName, string iconName, string folderPath)
        {
            FolderName = folderName; IconName = iconName; FolderPath = folderPath;
        }

        
    }
}
