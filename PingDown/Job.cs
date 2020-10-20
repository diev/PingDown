#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace PingDown
{
    public static class Job
    {
        const int _timeout = 4000;

        private static DateTime _timeToReload = DateTime.Now;
        private static States _currentState = States.NOT;
        private static string _selectedHost = Settings.Hosts[0];

        public static void ReInit()
        {
            Settings.Load();

            _timeToReload = DateTime.Now + Settings.ReloadEvery;
            _selectedHost = Settings.Hosts[0];

            JobTimer.Continue(Settings.RepeatEvery, Settings.RepeatEvery);

            string app = Path.ChangeExtension(App.Exe, null);

            string ping = app + ".ping";
            if (File.Exists(ping))
            {
                if (Environment.UserInteractive)
                {
                    Helpers.Log(Messages.PingFlag);
                }
                File.Delete(ping);
            }

            string down = app + ".down";
            if (File.Exists(down))
            {
                if (Environment.UserInteractive)
                {
                    Helpers.Log(Messages.DownFlag);
                }
                File.Delete(down);
                CheckDown();
            }

            string update = app + ".update";
            if (File.Exists(update))
            {
                if (Environment.UserInteractive)
                {
                    Helpers.Log(Messages.UpdateFlag);
                }
                Helpers.InstallUpdate(update);
            }

            string ver = app + ".ver";
            if (File.Exists(ver))
            {
                if (Environment.UserInteractive)
                {
                    Helpers.Log(Messages.VersionFlag);
                }
                File.Delete(ver);
                File.WriteAllText(app + ".v" + App.Version, DateTime.Now.ToString());
            }
        }

        public static void CheckState()
        {
            DateTime time = DateTime.Now;

            if (time.TimeOfDay < Settings.Wakeup)
            {
                return;
            }

            if (time > _timeToReload)
            {
                ReInit();
            }

            JobTimer.Pause();

            if ((_currentState == States.NOT) && SendPing("127.0.0.1"))
            {
                Thread.Sleep(_timeout);
                _currentState = States.NET;
            }

            if ((_currentState == States.NET) && !SendPing(_selectedHost))
            {
                _currentState = States.RUN;
            }

            if (_currentState == States.RUN)
            {
                _currentState = States.DOWN;

                int retries = 4;
                do
                {
                    Thread.Sleep(_timeout);
                    if (--retries == 0)
                    {
                        break;
                    }
                }
                while (!SendPing(_selectedHost));
            }

            if ((_currentState == States.DOWN) && !CheckDown())
            {
                _currentState = States.NET;
            }

            JobTimer.Continue();
        }

        private static bool SendPing(string host)
        {
            const int routers = 4;
            const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            bool result = false;

            Ping ping = null;
            try
            {
                ping = new Ping();
                PingOptions options = new PingOptions(routers, true);
                byte[] buffer = Encoding.ASCII.GetBytes(data);

                PingReply reply = ping.Send(host, _timeout, buffer, options);

                if (reply != null)
                {
                    result = reply.Status == IPStatus.Success;
                }
            }
            catch
            {
            }
            finally
            {
                if (ping != null)
                {
                    ping.Dispose();
                }
            }

            if (Environment.UserInteractive)
            {
                Helpers.Log(result ? host : "Failed " + host);
            }

            return result;
        }

        private static bool SendPings()
        {
            string[] hosts = Settings.Hosts;
            int counter = hosts.Length;
            var sync = new object();
            var isReady = new ManualResetEvent(false);
            bool result = false;

            foreach (string host in hosts)
            {
                Ping ping = null;
                try
                {
                    ping = new Ping();
                    ping.PingCompleted += delegate (object sender, PingCompletedEventArgs e)
                    {
                        lock (sync)
                        {
                            ping.Dispose();

                            PingReply reply = e.Reply;
                            if (reply != null && reply.Status == IPStatus.Success)
                            {
                                result = true;
                                _selectedHost = host;
                            }

                            if (--counter == 0)
                            {
                                isReady.Set();
                            }
                        }
                    };

                    ping.SendAsync(host, _timeout, null);
                }
                catch
                {
                    if (Environment.UserInteractive)
                    {
                        Helpers.Log("Failed ping " + host);
                    }
                }
                finally
                {
                    if (ping != null)
                    {
                        ping.Dispose();
                    }
                }
            }

            isReady.WaitOne();
            return result;
        }

        private static bool CheckDown()
        {
            if (Environment.UserInteractive)
            {
                if (CheckBypass())
                {
                    Helpers.Log(Messages.Passed);
                    return false;
                }

                if (!Settings.Force)
                {
                    Helpers.Log(Messages.ShutdownWanted);
                    return false;
                }

                Helpers.Log(Messages.ShutdownStarted);
            }
            else if (CheckBypass())
            {
                return false;
            }

            ExitWindows.Shutdown(true);
            return true;
        }

        private static bool CheckBypass()
        {
            string check = Helpers.Expiration();
            try
            {
                foreach (var driveInfo in DriveInfo.GetDrives())
                {
                    if (driveInfo.DriveType == DriveType.Removable && driveInfo.IsReady)
                    {
                        string label = driveInfo.VolumeLabel.ToUpper();
                        if (!string.IsNullOrEmpty(label) && label.Equals(check))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                {
                    Helpers.Log(Messages.DrivesBad + ex.Message);
                }
            }

            return false;
        }
    }
}
