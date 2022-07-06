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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;

namespace PingDown
{
    public static class Settings
    {
        /// <summary>
        /// Time to stop monitoring.
        /// </summary>
        public static TimeSpan Sleep { get; private set; }

        /// <summary>
        /// Time to start monitoring.
        /// </summary>
        public static TimeSpan Wakeup { get; private set; }

        /// <summary>
        /// Delay to start the service.
        /// </summary>
        public static int DelayStart { get; private set; }

        /// <summary>
        /// Period to repeat the service.
        /// </summary>
        public static int RepeatEvery { get; private set; }

        /// <summary>
        /// Period to repeat the service if alarm.
        /// </summary>
        public static int AlarmEvery { get; private set; }

        /// <summary>
        /// Number or retries if alarm.
        /// </summary>
        public static int AlarmRetries { get; private set; }

        /// <summary>
        /// Period to wait for every ping.
        /// </summary>
        public static int Timeout { get; private set; }

        /// <summary>
        /// Number of routers and gateways for ping TTL.
        /// </summary>
        public static int Gateways { get; private set; }

        /// <summary>
        /// Period to reinit settings.
        /// </summary>
        public static TimeSpan ReloadEvery { get; private set; }

        /// <summary>
        /// Service can pause, continue, stop by MMC.
        /// </summary>
        public static bool Services { get; private set; } = false;

        /// <summary>
        /// Hosts to ping.
        /// </summary>
        public static string[] Hosts { get; private set; }

        /// <summary>
        /// Force (it is danger to switch off!)
        /// </summary>
        public static bool Force { get; set; } = false;

        private static DateTime _configTime;
        private static NameValueCollection _settings;

        static Settings()
        {
            Load();
        }

        public static void Load()
        {
            DateTime configTime = File.GetLastWriteTime(App.Config);

            if (configTime == _configTime)
            {
                return;
            }

            if (_configTime.Ticks > 0) // Refresh config in memory
            {
                Helpers.Log(Messages.NewSettings);
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }

            _settings = ConfigurationManager.AppSettings;

            Sleep = GetTimeSpan(nameof(Sleep), "22:00");
            Wakeup = GetTimeSpan(nameof(Wakeup), "6:00");
            DelayStart = GetMilliseconds(nameof(DelayStart), "0:02", "0:00:02");
            RepeatEvery = GetMilliseconds(nameof(RepeatEvery), "0:01", "0:00:05");
            AlarmEvery = GetMilliseconds(nameof(AlarmEvery), "0:00:10", "0:00:05");
            AlarmRetries = GetInt(nameof(AlarmRetries), "5");
            ReloadEvery = GetTimeSpan(nameof(ReloadEvery), "0:30", "0:00:30");
            Timeout = GetMilliseconds(nameof(Timeout), "0:00:10");
            Gateways = GetInt(nameof(Gateways), "4");

            string value = _settings["Hosts"];
            Hosts = value == null
                ? Pinger.GetGateways()
                : Helpers.ReadList(value);

            _configTime = configTime;
        }

        private static TimeSpan GetTimeSpan(string name, string workValue, string testValue = null)
        {
            string value = _settings[name] ?? (Environment.UserInteractive ? testValue ?? workValue : workValue);

            if (TimeSpan.TryParse(value, out TimeSpan time))
            {
                return time;
            }

            Helpers.Log($"Failed config:{name}");
            return TimeSpan.Zero;
        }

        private static int GetMilliseconds(string name, string workValue, string testValue = null)
        {
            string value = _settings[name] ?? (Environment.UserInteractive ? testValue ?? workValue : workValue);

            if (TimeSpan.TryParse(value, out TimeSpan time))
            {
                return (int)time.TotalMilliseconds;
            }

            Helpers.Log($"Failed config:{name}");
            return 0;
        }

        private static int GetInt(string name, string workValue, string testValue = null)
        {
            string value = _settings[name] ?? (Environment.UserInteractive ? testValue ?? workValue : workValue);

            if (int.TryParse(value, out int num))
            {
                return num;
            }

            Helpers.Log($"Failed config:{name}");
            return 0;
        }
    }
}
