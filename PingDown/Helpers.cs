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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PingDown
{
    public static class Helpers
    {
        public static string[] ReadList(string list)
        {
            char[] sep = { ',', ' ', '\r', '\n', '\t' };
            return list.Replace("127.0.0.1", "").Split(sep, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void Log(string s)
        {
            DateTime time = DateTime.Now;

            // Log to file
            try
            {
                File.AppendAllText(App.Log, time.ToString("yyyy-MM-dd HH:mm:ss ") + s + Environment.NewLine, Encoding.GetEncoding(1251));
            }
            catch { }

            // Log to console
            if (Environment.UserInteractive)
            {
                #region Output
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
                Console.WriteLine(time.ToString("HH:mm:ss ") + s);
                Console.ResetColor();
                #endregion
            }
        }

        public static string Expiration()
        {
            return "EXP" + DateTime.Now.AddDays(7).ToString("yy-MM-dd");
        }

        public static void InstallUpdate(string update)
        {
            string exe = App.Exe;
            string cmd = Path.ChangeExtension(exe, "cmd");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("net stop " + App.Name);
            sb.AppendLine(exe + " -u");
            sb.AppendLine("del " + exe);
            sb.AppendLine("copy " + update + " " + exe);
            sb.AppendLine(exe + " -i");
            sb.AppendLine("net start " + App.Name);
            sb.AppendLine("del " + update);
            sb.AppendLine("del " + cmd);

            File.WriteAllText(cmd, sb.ToString(), Encoding.GetEncoding(866));

            Process.Start("cmd.exe", "/c " + cmd);
        }
    }
}
