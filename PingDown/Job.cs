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
using System.Threading;

namespace PingDown
{
    public static class Job
    {
        private static DateTime _timeToReload = DateTime.Now;
        private static States _currentState = States.NOT;
        private static string _selectedHost;

        public static void ReInit()
        {
            Settings.Load();

            _timeToReload = DateTime.Now + Settings.ReloadEvery;
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
                CheckWar();
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

            string update = app + ".update";
            if (File.Exists(update))
            {
                if (Environment.UserInteractive)
                {
                    Helpers.Log(Messages.UpdateFlag);
                }
                Helpers.InstallUpdate(update);
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

            switch (_currentState)
            {
                case States.NOT:
                    if (SendPing("127.0.0.1"))
                    {
                        _selectedHost = Settings.Hosts[0];
                        SendPings(Settings.Hosts);
                    }
                    break;

                case States.NET:
                    _currentState = States.RUN;
                    if (!SendPing(_selectedHost))
                    {
                        SendPings(Settings.Hosts);
                    }
                    break;

                case States.RUN:
                case States.DOWN:
                    _currentState = States.DOWN;
                    if (!SendPings(Settings.Hosts))
                    {
                        CheckWar();
                    }
                    break;

                default:
                    SendPings(Settings.Hosts);
                    break;
            }
        }

        private static bool SendPing(string host)
        {
            return SendPings(new string[] { host });
        }

        private static bool SendPings(string[] hosts)
        {
            const int timeout = 1000;

            int counter = hosts.Length;
            var sync = new object();
            var isReady = new ManualResetEvent(false);

            foreach (string host in hosts)
            {
                Ping ping = new Ping();
                ping.PingCompleted += delegate (object sender, PingCompletedEventArgs e)
                {
                    lock (sync)
                    {
                        ping.Dispose();

                        PingReply reply = e.Reply;
                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            _currentState = States.NET;
                            _selectedHost = host;

                            if (Environment.UserInteractive)
                            {
                                Helpers.Log(host);
                            }
                        }
                        else if (Environment.UserInteractive)
                        {
                            Helpers.Log("Failed " + host);
                        }

                        if (--counter == 0)
                        {
                            isReady.Set();
                        }
                    }
                };

                ping.SendAsync(host, timeout, null);
            }

            isReady.WaitOne();
            return _currentState == States.NET;
        }

        private static void CheckWar()
        {
            if (Environment.UserInteractive)
            {
                if (CheckBypass())
                {
                    Helpers.Log(Messages.Passed);
                    _currentState = States.NET;
                    return;
                }

                if (!Settings.Force)
                {
                    Helpers.Log(Messages.ShutdownWanted);
                    _currentState = States.NET;
                    return;
                }

                Helpers.Log(Messages.ShutdownStarted);
            }
            else if (CheckBypass())
            {
                _currentState = States.NET;
                return;
            }

            ExitWindows.Shutdown(true);
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
