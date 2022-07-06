#region License
//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Open source software https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Threading;

namespace PingDown
{
    public static class Job
    {
        private static readonly string PingFlag = Path.ChangeExtension(App.Exe, "ping");
        private static readonly string DownFlag = Path.ChangeExtension(App.Exe, "down");
        private static readonly string UpdateFlag = Path.ChangeExtension(App.Exe, "update");
        private static readonly string VersionFlag = Path.ChangeExtension(App.Exe, "ver");
        private static readonly string PassFlag = Path.ChangeExtension(App.Exe, "pass");

        private static DateTime _timeToReload = DateTime.Now;
        private static States _currentState = States.NOT;

        public static void ReInit()
        {
            Settings.Load();

            _timeToReload = DateTime.Now + Settings.ReloadEvery;

            if (File.Exists(PingFlag))
            {
                Helpers.LogInteractive(Messages.PingFlag);
                File.Delete(PingFlag);
            }

            if (File.Exists(DownFlag))
            {
                Helpers.LogInteractive(Messages.DownFlag);
                File.Delete(DownFlag);

                CheckDown();
            }

            if (File.Exists(UpdateFlag))
            {
                Helpers.LogInteractive(Messages.UpdateFlag);
                //File.Delete(UpdateFlag); //NO, it's the update file itself!

                Helpers.InstallUpdate(UpdateFlag);
            }

            if (File.Exists(VersionFlag))
            {
                Helpers.LogInteractive(Messages.VersionFlag);
                File.Delete(VersionFlag);

                string app = Path.ChangeExtension(App.Exe, $".v{App.Version}");
                File.WriteAllText(app, DateTime.Now.ToString());
            }

            JobTimer.Continue(Settings.RepeatEvery, Settings.RepeatEvery);
        }

        public static void CheckState()
        {
            DateTime time = DateTime.Now;

            if (time.TimeOfDay > Settings.Sleep || time.TimeOfDay < Settings.Wakeup)
            {
                return;
            }

            if (time > _timeToReload)
            {
                ReInit();
            }

            JobTimer.Pause();

            if (_currentState == States.NOT && Pinger.Ready())
            {
                Thread.Sleep(Settings.DelayStart);
                _currentState = States.NET;
            }

            if (_currentState == States.NET && !Pinger.Send())
            {
                _currentState = States.DOWN;
            }

            if (_currentState == States.DOWN && !CheckDown())
            {
                _currentState = States.NET;
            }

            JobTimer.Continue();
        }

        private static bool CheckDown()
        {
            if (Environment.UserInteractive && !Settings.Force)
            {
                Helpers.Log(Messages.ShutdownWanted);
                return false;
            }
            
            if (CheckBypass())
            {
                Helpers.LogInteractive(Messages.Passed);
                return false;
            }

            Helpers.Log(Messages.ShutdownStarted);
            ExitWindows.Shutdown();

            return true;
        }

        private static bool CheckBypass()
        {
            if (File.Exists(PassFlag))
            {
                return true;
            }

            string pass = Helpers.Expiration();

            try
            {
                foreach (var driveInfo in DriveInfo.GetDrives())
                {
                    if (driveInfo.DriveType == DriveType.Removable &&
                        driveInfo.IsReady &&
                        driveInfo.VolumeLabel.ToUpper().Equals(pass))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.LogInteractive(Messages.DrivesBad + ex.Message);
            }

            return false;
        }
    }
}
