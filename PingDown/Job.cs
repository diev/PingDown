//using Microsoft.Win32;
using System;
//using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.IO;

namespace PingDown
{
    class Job
    {
        public struct Counters
        {
            public static int ReInit = 0; // 250 (~1 hour at 1/15")
            public static int Queue = 0;
            public static int Alarm = 0; // length of HOSTS
        }

        public struct States
        {
            public static bool PNG = false;
            public static bool RUN = false;
            public static bool WAR = false;
        }

        private static string[] HOSTS = { };

        private static int RUNDelayStart = 1000;
        private static int RUNRepeatEvery = 10000;

        public Job()
        {
            //
        }

        public static bool Init(string[] args)
        {
            string hosts = null;

            if (args.Length == 0 || args[0].Equals("-"))
            {
                //RegistryKey sKey = Registry.LocalMachine.OpenSubKey(Program.AppRegistry);
                //if (sKey != null)
                //{
                //    RegistryKey pKey = sKey.OpenSubKey(Program.AppParameters);
                //    if (pKey != null)
                //    {
                //        hosts = pKey.GetValue("Hosts").ToString();
                //    }
                //}

                //if (!string.IsNullOrWhiteSpace(hosts))
                //{
                //    Program.Log("Hosts (registry): " + hosts);
                //}
                //else
                //{ 
                //hosts = Properties.Settings.Default.Hosts;

                string hostsFile = Path.ChangeExtension(Program.AppExe, "hosts");
                if (File.Exists(hostsFile))
                {
                    hosts = File.ReadAllText(hostsFile);
                }
                else
                {
                    hosts = "8.8.8.8, 8.8.4.4"; // Google IP's by default
                }

                    //Program.Log("Hosts (config): " + hosts);
                //}
            }
            else
            {
                hosts = args[0];
                //Program.Log("Hosts (command): " + hosts);
            }

            if (string.IsNullOrEmpty(hosts))
            {
                Program.Log("Failed to get hosts");
                return false;
            }
            char[] sep = { ',', ' ', '\r', '\n', '\t' };
            HOSTS = hosts.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            return HOSTS.Length > 0;
        }

        public static void CheckState(Object stateInfo)
        {
            if (++Counters.ReInit >= 250)
            {
                Counters.ReInit = 0;
                ReInit();
            }

            if (States.PNG)
            {
                CheckNext();
            }
            string who = HOSTS[Counters.Queue];

            if (++Counters.Queue >= HOSTS.Length)
            {
                Counters.Queue = 0;
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
            pingSender.Dispose();
            //waiter.Dispose();
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

            if (States.RUN)
            {
                CheckOk();
            }
        }

        public static void CheckNext()
        {
            if (Program.TEST)
            {
                Program.Log("Failed!");
            }

            if (!States.RUN)
            {
                States.RUN = true;
                Service.JobTimer.Change(RUNDelayStart, RUNRepeatEvery);
            }

            ReInit();

            if (++Counters.Alarm >= HOSTS.Length)
            {
                States.WAR = true;
                Counters.Alarm = 0;
                CheckWar();
            }
        }

        public static void ReInit()
        {
            string hostsFile = Path.ChangeExtension(Program.AppExe, "hosts");
            if (File.Exists(hostsFile))
            {
                string hosts = File.ReadAllText(hostsFile);
                char[] sep = { ',', ' ', '\r', '\n', '\t' };
                HOSTS = hosts.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static void CheckOk()
        {
            States.RUN = false;
            States.WAR = false;
            Counters.Alarm = 0;
            if (Program.TEST)
            {
                Program.Log("OK");
                Service.FasterTimer(4);
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
            else
            {
                Program.Log("Shutdown started");
                ExitWindows.Shutdown(!Program.TEST);
            }
        }
    }
}
