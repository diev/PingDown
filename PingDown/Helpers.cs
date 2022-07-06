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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PingDown
{
    public static class Helpers
    {
        public static string[] ReadList(string list)
        {
            char[] sep = { ' ', ',', ';', '\r', '\n', '\t' };
            return list.Replace("127.0.0.1", string.Empty).Split(sep, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void Log(string s)
        {
            LogToFile(s);

            if (Environment.UserInteractive)
            {
                LogToConsole(s);
            }
        }

        public static void LogInteractive(string s)
        {
            if (Environment.UserInteractive)
            {
                LogToFile(s);
                LogToConsole(s);
            }
        }

        private static void LogToFile(string s)
        {
            try
            {
                File.AppendAllText(App.Log,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {s}{Environment.NewLine}",
                    Encoding.GetEncoding(1251));
            }
            catch { }
        }

        private static void LogToConsole(string s)
        {
            switch (s)
            {
                case Messages.UnrecognizedOption:
                case Messages.ParameterRequired:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case Messages.NewSettings:
                case Messages.ShutdownWanted:
                case Messages.PingFlag:
                case Messages.DownFlag:
                case Messages.VersionFlag:
                case Messages.UpdateFlag:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case Messages.Passed:
                case Messages.ServiceInstalled:
                case Messages.ServiceRemoved:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                default:
                    if (s.Contains("!") || s.StartsWith("Failed "))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    break;
            }
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} {s}");
            Console.ResetColor();
        }

        public static string Expiration()
        {
            return $"EXP{DateTime.Now.AddDays(7):yy-MM-dd}";
        }

        public static void InstallUpdate(string update)
        {
            string exe = App.Exe;
            string cmd = Path.ChangeExtension(exe, "cmd");

            string text = string.Join(Environment.NewLine,
                "@echo off",
                $"net stop {App.Name}",
                $"{exe} -u",
                $"del {exe}",
                $"copy {update} {exe}",
                $"{exe} -i",
                $"net start {App.Name}",
                $"del {update}",
                $"del {cmd}",
                string.Empty);

            File.WriteAllText(cmd, text, Encoding.GetEncoding(866));

            Process.Start("cmd.exe", $"/c {cmd}");
        }
    }
}
