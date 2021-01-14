//----------------------------------------------------
// Copyright 2021 Epic Systems Corporation
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private TaskbarIcon tb;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //initialize NotifyIcon
            tb = (TaskbarIcon)FindResource("TaskBarIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //tb.Dispose; //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
