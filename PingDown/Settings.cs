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
using System.Configuration;
using System.IO;

namespace PingDown
{
    public static class Settings
    {
        /// <summary>
        /// Time to start monitoring
        /// </summary>
        public static TimeSpan Wakeup;

        /// <summary>
        /// Delay to start the service
        /// </summary>
        public static int DelayStart;

        /// <summary>
        /// Period to repeat the service
        /// </summary>
        public static int RepeatEvery;

        /// <summary>
        /// Period to reinit settings
        /// </summary>
        public static TimeSpan ReloadEvery;

        /// <summary>
        /// Service can pause, continue, stop by MMC
        /// </summary>
        public static bool Services = false;

        /// <summary>
        /// Hosts to ping
        /// </summary>
        public static string[] Hosts;

        /// <summary>
        /// Force (it is danger to switch off!)
        /// </summary>
        public static bool Force = false;

        private static DateTime _configTime;

        static Settings()
        {
            Load();
        }

        public static void Load()
        {
            DateTime configTime = File.GetLastWriteTime(App.Config);
            if (configTime != _configTime)
            {
                if (_configTime.Ticks > 0) // Refresh config in memory
                {
                    Helpers.Log(Messages.NewSettings);
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                }

                var settings = ConfigurationManager.AppSettings;
                string value;

                value = settings["Wakeup"] ?? "6:00";
                if (TimeSpan.TryParse(value, out TimeSpan wakeup))
                {
                    Wakeup = wakeup;
                }
                else
                {
                    Helpers.Log("Failed config:Wakeup");
                }

                value = settings["DelayStart"] ?? "0:02";
                if (TimeSpan.TryParse(value, out TimeSpan delayStart))
                {
                    DelayStart = (int)delayStart.TotalMilliseconds;
                }
                else
                {
                    Helpers.Log("Failed config:DelayStart");
                }

                value = settings["RepeatEvery"] ?? "0:01";
                if (TimeSpan.TryParse(value, out TimeSpan repeatEvery))
                {
                    RepeatEvery = (int)repeatEvery.TotalMilliseconds;
                }
                else
                {
                    Helpers.Log("Failed config:RepeatEvery");
                }

                value = settings["ReloadEvery"] ?? "0:05";
                if (TimeSpan.TryParse(value, out TimeSpan reloadEvery))
                {
                    ReloadEvery = reloadEvery;
                }
                else
                {
                    Helpers.Log("Failed config:ReloadEvery");
                }

                value = settings["Hosts"] ?? "10.0.2.2";
                Hosts = Helpers.ReadList(value);

                if (Environment.UserInteractive)
                {
                    value = settings["TestWakeup"] ?? "6:00";
                    if (TimeSpan.TryParse(value, out wakeup))
                    {
                        Wakeup = wakeup;
                    }
                    else
                    {
                        Helpers.Log("Failed config:TestWakeup");
                    }

                    value = settings["TestDelayStart"] ?? "0:00:02";
                    if (TimeSpan.TryParse(value, out delayStart))
                    {
                        DelayStart = (int)delayStart.TotalMilliseconds;
                    }
                    else
                    {
                        Helpers.Log("Failed config:TestDelayStart");
                    }

                    value = settings["TestRepeatEvery"] ?? "0:00:10";
                    if (TimeSpan.TryParse(value, out repeatEvery))
                    {
                        RepeatEvery = (int)repeatEvery.TotalMilliseconds;
                    }
                    else
                    {
                        Helpers.Log("Failed config:TestRepeatEvery");
                    }

                    value = settings["TestReloadEvery"] ?? "0:00:30";
                    if (TimeSpan.TryParse(value, out reloadEvery))
                    {
                        ReloadEvery = reloadEvery;
                    }
                    else
                    {
                        Helpers.Log("Failed config:TestReloadEvery");
                    }

                    value = settings["TestHosts"] ?? "10.0.2.2";
                    Hosts = Helpers.ReadList(value);
                }

                _configTime = configTime;
            }
        }
    }
}
