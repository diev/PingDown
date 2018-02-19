//using Microsoft.Win32;
using System;
//using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.IO;
//using System.Management;

namespace PingDown
{
    public static class Job
    {
        public struct Counters
        {
            public const int RUNDelayStart = 1000;
            public const int RUNRepeatEvery = 10000;

            public static int ReInitLimit = 90; // 250 (~1 hour at 1/15")
            public static int RetriesLimit = 3;

            public static int ReInit = 0;
            public static int Queue = 0;
            public static int Alarm = 0; // length of HOSTS
            public static int Retries = 0;
        }

        public struct States
        {
            public static bool NET = false; // network ready
            public static bool PNG = false; // ping wanted
            public static bool RUN = false; // processing
            public static bool WAR = false; // ping lost
        }

        private static string[] HOSTS = { };
        private static bool HostsFixed = false;

        public static bool Init(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("-"))
            {
                ReInit();
            }
            else
            {
                ReInit(args[0]);
                //States.NET = true;
                HostsFixed = true;
            }

            if (Program.TEST)
            {
                Counters.ReInitLimit /= 10;
            }

            return HOSTS.Length > 0;
        }

        public static void CheckState(Object stateInfo)
        {
            string who;
            if (States.NET)
            {
                if (++Counters.ReInit >= Counters.ReInitLimit)
                {
                    ReInit();
                    Counters.ReInit = 0;
                }

                if (States.PNG)
                {
                    CheckNext();
                }

                who = HOSTS[Counters.Queue];

                if (++Counters.Queue >= HOSTS.Length)
                {
                    Counters.Queue = 0;
                }

            }
            else
            {
                who = "127.0.0.1";
            }

            if (Program.TEST)
            {
                Program.Log("Ping " + who);
            }

            States.PNG = true;
            //AutoResetEvent waiter = (AutoResetEvent)stateInfo;
            AutoResetEvent waiter = new AutoResetEvent(false);

            // http://msdn.microsoft.com/en-us/library/ms229713.aspx

            Ping pingSender = new Ping();
            try
            {
                // When the PingCompleted event is raised,
                // the PingCompletedCallback method is called.
                pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                // Wait 4 seconds for a reply.
                const int timeout = 4000;

                // Set options for transmission:
                // The data can go through 4 gateways or routers
                // before it is destroyed, and the data packet
                // cannot be fragmented.
                PingOptions options = new PingOptions(4, true);

                // Send the ping asynchronously.
                // Use the waiter as the user token.
                // When the callback completes, it can wake up this thread.
                pingSender.SendAsync(who, timeout, buffer, options, waiter);
                waiter.WaitOne();
            }
            catch
            {
                States.NET = false;
                States.WAR = true;

                CheckWar();
            }
            finally
            {
                pingSender.Dispose();
                //waiter.Dispose();
            }
        }

        private static void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            States.PNG = false;
            //((Ping)sender).Dispose();

            // Let the main thread resume. 
            // UserToken is the AutoResetEvent object that the main thread 
            // is waiting for.
            ((AutoResetEvent)e.UserState).Set();

            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null || e.Reply == null)
            {
                CheckNext();
                return;
            }

            PingReply reply = e.Reply;
            if (reply.Status != IPStatus.Success)
            {
                CheckNext();
                return;
            }

            if (!States.NET)
            {
                States.NET = true; // Network becomes ready
            }

            if (States.RUN)
            {
                States.RUN = false;
                States.WAR = false;
                Counters.Alarm = 0;
                Counters.Retries = 0;

                CheckOk();
            }
        }

        public static void CheckNext()
        {
            if (!States.NET) // no Network ready
            {
                return;
            }

            if (Program.TEST)
            {
                Program.Log("Failed!");
            }

            if (!States.RUN)
            {
                States.RUN = true;
                Service.JobTimer.Change(Counters.RUNDelayStart, Counters.RUNRepeatEvery);
            }

            if (++Counters.Alarm >= HOSTS.Length)
            {
                Counters.Alarm = 0;
                ++Counters.Retries;

                Program.Log("Retries " + Counters.Retries.ToString());
                ReInit();
                Counters.ReInit = 0;
            }

            if (Counters.Retries >= Counters.RetriesLimit)
            {
                States.WAR = true;
                Counters.Alarm = 0;
                Counters.Retries = 0;

                CheckWar();
            }
        }

        public static void ReInit(string hosts = null)
        {
            if (HostsFixed)
            {
                return;
            }
            if (string.IsNullOrEmpty(hosts))
            {
                string hostsFile = Path.ChangeExtension(Program.AppExe, "hosts");
                if (File.Exists(hostsFile))
                {
                    Program.Log("Read hosts");
                    hosts = File.ReadAllText(hostsFile);
                }
            }
            char[] sep = { ',', ' ', '\r', '\n', '\t' };
            HOSTS = hosts.Split(sep, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void CheckOk()
        {
            if (Program.TEST)
            {
                Program.Log("OK");
                Service.FasterTimer(6);
            }
            else
            {
                Service.FasterTimer();
            }
        }

        public static void CheckWar()
        {
            //EventLog.WriteEntry("Failed cables", EventLogEntryType.Warning);

            if (Program.TEST)
            {
                Program.Log("WAR!");
            }

            if (Program.TESTonly)
            {
                Program.Log("Shutdown wanted");
                States.WAR = false;
            }

            if (LastCheck())
            {
                Program.Log("Passed");
                States.WAR = false;
            }

            if (States.WAR)
            {
                Program.Log("Shutdown started");
                ExitWindows.Shutdown(!Program.TEST);
            }
        }

        public static bool LastCheck()
        {
            string check = "P" + DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            try
            {
                //ConnectionOptions opts = new ConnectionOptions();
                //ManagementScope scope = new ManagementScope(@"\\.\root\cimv2", opts);
                //SelectQuery diskQuery = new SelectQuery("SELECT * FROM Win32_LogicalDisk WHERE (MediaType != 0)");
                //ManagementObjectSearcher searcher = new ManagementObjectSearcher(diskQuery);
                //ManagementObjectCollection diskObjColl = searcher.Get();
                //foreach (var disk in diskObjColl)
                //{
                //    string name = disk["VolumeName"].ToString();
                //    if (!string.IsNullOrEmpty(name) && name.Equals(check))

                foreach (var driveInfo in DriveInfo.GetDrives())
                {
                    if (driveInfo.DriveType == DriveType.Removable && driveInfo.IsReady)
                    {
                        string label = driveInfo.VolumeLabel;
                        if (!string.IsNullOrEmpty(label) && label.Equals(check))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log("Drives bad: " + ex.Message);
            }
            return false;
        }
    }
}
