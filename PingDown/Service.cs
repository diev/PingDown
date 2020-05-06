// Copyright (c) 2012-2020 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/PingDown

using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace PingDown
{
    class Service : ServiceBase
    {
        public static Timer JobTimer;

        /// <summary>
        /// Delay to start the service
        /// </summary>
        public const int DelayStart = 120000; //ms
        /// <summary>
        /// Period to repeat the service
        /// </summary>
        public const int RepeatEvery = 120000; //ms
        /// <summary>
        /// Times to run faster during tests
        /// </summary>
        public const int TimesFaster = 12; //RepeatEvery / TimesFaster ~ 10 sec

        public Service()
        {
            this.ServiceName = Program.AppName;
            //this.AutoLog = true; // Program.TEST;
            //this.CanPauseAndContinue = true;
            //this.CanStop = true;
        }

        public static void FasterTimer(int faster = 1)
        {
            if (faster == 0)
            {
                JobTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                JobTimer.Change(RepeatEvery / faster, RepeatEvery / faster);
            }
        }

        public void StartService(string[] args)
        {
            //Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (Job.Init(args))
            {
                AutoResetEvent timerEvent = new AutoResetEvent(false);
                TimerCallback timerCallback = Job.CheckState;

                JobTimer = new Timer(timerCallback, timerEvent, DelayStart, RepeatEvery);
                if (Program.TestReal)
                {
                    FasterTimer(TimesFaster);
                }
            }
            else
            {
                EventLog.WriteEntry("No Init!", EventLogEntryType.Error);
                StopService();
                return;
            }
            Program.Log("Service started");
        }

        public void StopService()
        {
            if (Program.TestReal && Job.States.WAR)
            {
                EventLog.WriteEntry("Failed pings!", EventLogEntryType.Warning);
            }
            JobTimer.Dispose();
            Program.Log("Service stopped");
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            StartService(args);
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopService();
        }

        //protected override void OnPause()
        //{
        //    base.OnPause();
        //    FasterTimer(0);
        //}

        //protected override void OnContinue()
        //{
        //    base.OnContinue();
        //    FasterTimer();
        //}

        protected override void OnShutdown()
        {
            Program.Log("Shutdown received");
            base.OnShutdown();
            StopService();
        }
    }
}
