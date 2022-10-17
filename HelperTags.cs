//----------------------------------------------------
// Copyright 2021 Epic Systems Corporation
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Forms = System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    class HelperTags
    {

        public static void Schedule_Timer(TimerPlus timer)
        {
            if (Settings1.Default.enableEOD == false) { return; }
            TimeSpan settingsTime = Settings1.Default.EODTime;  //cache user setting
            DateTime now = DateTime.Now;
            DayOfWeek nowDOW = now.DayOfWeek; //current day of week
            TimeSpan nowTime = TimeSpan.Parse(now.ToString("HH:mm")); //current time in TimeSpan format
            DateTime scheduledTime;
            DateTime scheduledDay; //midnight of the day to next execute the timer
            DateTime today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0); //midnight of today

            //Logic block to set the correct scheduledTime TimeSpan based on day of week and time of day
            if (nowDOW == DayOfWeek.Saturday)
            {
                scheduledDay = today.AddDays(2); //want it scheduled for 2 days from now
            }
            if (nowDOW == DayOfWeek.Sunday)
            {
                scheduledDay = today.AddDays(1); //want it scheduled for 1 day from now
            }
            else
            {
                if (now.TimeOfDay < settingsTime) //logic when prior in the day to the desired time
                {
                    scheduledDay = today; //want scheduled for later today
                }
                else
                {
                    if (nowDOW == DayOfWeek.Friday)
                    {
                        scheduledDay = today.AddDays(3); //want scheduled for Monday
                    }
                    else
                    {
                        scheduledDay = today.AddDays(1); //want scheduled for tomorrow
                    }
                }
            }
            scheduledTime = scheduledDay.Add(settingsTime);
            scheduledTime = scheduledTime.AddSeconds(25); //adding 25 seconds to the scheduled time as a buffer to avoid display rounding issues

            //Now setup the timer
            double tickTime = (double)(scheduledTime-DateTime.Now).TotalMilliseconds;
            timer.Interval = tickTime;
            timer.Start();
#if DEBUG
            Console.WriteLine("### Timer Started ### \n");
            Console.WriteLine("Time Left: "+timer.Interval.ToString());
            Console.WriteLine("Scheduled Time " + (DateTime.Now.AddMilliseconds(timer.Interval).ToString()));
#endif

        }

        public static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            StopTimer(App.timer);
            Schedule_Timer(App.timer);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (Application.Current.Windows.OfType<EODWindow>().Any()) { return; }
                EODWindow window1 = new EODWindow();
                window1.Show();
            });
        }
        public static void StopTimer(TimerPlus timer)
        {
            timer.Stop();
        }

        public static DateTime NextTimerEvent(TimerPlus timer)
        {
                return (DateTime.Now.AddMilliseconds(timer.TimeLeft));
        }
        public static void CreateTask(string subject, bool feedbackTask, bool followupTask)
        {
            string body = "";
            if (subject.Length >= 100)
            {
                int index = subject.IndexOf(" ", 99);
                if (index > 0)
                {
                    body = subject;
                    subject = subject.Substring(0, index);
                    subject += "...";
                }
            }
            Outlook.ApplicationClass app = new Outlook.ApplicationClass();
            Outlook.TaskItem tsk = (Outlook.TaskItem)app.CreateItem(Outlook.OlItemType.olTaskItem);
            tsk.Subject = subject;
            tsk.Body = body;
            tsk.Save();
            if (feedbackTask == true)
            {
                Outlook.MAPIFolder folder = GetFolder(Settings1.Default.feedbackFolder);
                tsk.Move(folder);
            }
            if (followupTask == true)
            {
                Outlook.MAPIFolder folder = GetFolder(Settings1.Default.fuFolder);
                tsk.Move(folder);
            }
        }

        public static void CreateAppt(string title, DateTime startTime, DateTime endTime)
        {
            Outlook.ApplicationClass app = new Outlook.ApplicationClass();
            Outlook.AppointmentItem appt = (Outlook.AppointmentItem)app.CreateItem(Outlook.OlItemType.olAppointmentItem);
            appt.Subject = title;
            appt.Start = startTime;
            appt.End = endTime;
            appt.Save();
        }
        // Returns Folder object based on folder path
        private static Outlook.MAPIFolder GetFolder(string folderPath)
        {
            Outlook.MAPIFolder folder;
            Outlook.ApplicationClass app = new Outlook.ApplicationClass();
            string backslash = @"\";
            try
            {
                if (folderPath.StartsWith(@"\\"))
                {
                    folderPath = folderPath.Remove(0, 2);
                }
                String[] folders = folderPath.Split(backslash.ToCharArray());
                folder = app.Session.Folders[folders[0]];
                if (folder != null)
                {
                    for (int i = 1; i <= folders.GetUpperBound(0); i++)
                    {
                       Outlook.Folders subFolders = folder.Folders;
                        folder = subFolders[folders[i]];
                        if (folder == null)
                        {
                            return null;
                        }
                    }
                }
                return folder;
            }
            catch { return null; }
        }

        public static void InstallUpdateSyncWithInfo()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();

                }
                catch (DeploymentDownloadException dde)
                {
                    MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        MessageBoxResult dr = MessageBox.Show(Application.Current.MainWindow,"An update is available. Would you like to update the application now?", "Update Available", MessageBoxButton.OKCancel);
                        if (!(MessageBoxResult.OK == dr))
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        MessageBox.Show("This application has detected a mandatory update from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() +
                            ". The application will now install the update and restart.",
                            "Update Available", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            ad.Update();
                            MessageBox.Show("The application has been upgraded, and will now restart.");
                            Forms.Application.Restart();
                            Application.Current.Shutdown();
                        }
                        catch (DeploymentDownloadException dde)
                        {
                            MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                            return;
                        }
                    }
                }
            }
        }
    }
}
