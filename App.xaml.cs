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
using System.Timers;

namespace TaskMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon tb;
        public static TimerPlus timer = new TimerPlus();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //initialize NotifyIcon
            tb = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)FindResource("MyTaskBarIcon");
            timer.Elapsed += new ElapsedEventHandler(HelperTags.Timer_Elapsed);
            timer.AutoReset = false;
            HelperTags.Schedule_Timer(timer);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            tb.Dispose(); //the icon would clean up automatically, but this is cleaner
            timer.Dispose();
            base.OnExit(e);
        }

    }
}
