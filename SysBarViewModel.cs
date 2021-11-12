//----------------------------------------------------
// Copyright 2021 Epic Systems Corporation
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TaskMaster
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class SysBarViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    //CanExecuteFunc = () => Application.Current.MainWindow == null,
                    CommandAction = () =>
                    {
                        //Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.Activate();
                    }
                };
            }
        }

        public ICommand CheckForUpdateCommand
        {
            get
            {
                return new DelegateCommand
                {
                    //CanExecuteFunc = () => Application.Current.MainWindow == null,
                    CommandAction = () =>
                    {
                        //Application.Current.MainWindow = new MainWindow();
                        HelperTags.InstallUpdateSyncWithInfo();
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Hide(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null
                };
            }
        }

        /// <summary>
        /// Hides the main window if open and opens the EOD window
        /// </summary>
        public ICommand EODWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => 
                    {
                        if (Application.Current.Windows.OfType<EODWindow>().Any()) { return; }
                        if (Application.Current.MainWindow != null) { Application.Current.MainWindow.Hide(); }
                        EODWindow window1 = new EODWindow();
                        window1.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Open settings menu
        /// </summary>
        public ICommand SettingsWindow
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Settings window1;
                        if (Application.Current.Windows.OfType<Settings>().Any())
                        {
                            window1 = Application.Current.Windows.OfType<Settings>().First();
                            window1.Show();
                            window1.Activate();
                        }
                        window1 = new Settings();
                        window1.Show();
                    }
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }
    }


    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

