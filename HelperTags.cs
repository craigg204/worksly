//----------------------------------------------------
// Copyright 2021 Epic Systems Corporation
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace TaskMaster
{
    class HelperTags
    {
        static Timer timer;

        public static void Schedule_Timer()
        {
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
                    scheduledDay = today.AddDays(1); //want scheduled for tomorrow
                }
            }
            scheduledTime = scheduledDay.Add(settingsTime);

            //Now setup the timer
            double tickTime = (double)(scheduledTime-DateTime.Now).TotalMilliseconds;
            timer = new Timer(tickTime);
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();
#if DEBUG
            Console.WriteLine("### Timer Started ### \n");
            Console.WriteLine("Time Left: "+timer.Interval.ToString());
            Console.WriteLine("Scheduled Time " + (DateTime.Now.AddMilliseconds(timer.Interval).ToString()));
#endif

        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                EODWindow window1 = new EODWindow();
                window1.Show();
            });
            
            Schedule_Timer();
        }
        public static void StopTimer()
        {
            timer.Dispose();
        }
    }
}
