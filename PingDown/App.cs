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
using System.Configuration;
using System.IO;
using System.Reflection;

namespace PingDown
{
    public static class App
    {
        public static string Name { get; private set; }
        public static string Exe { get; private set; }
        public static string Config { get; private set; }
        public static string Version { get; private set; }
        public static string DisplayName { get; private set; }
        public static string Description { get; private set; }
        public static string Info { get; private set; }
        public static string Banner { get; private set; }
        public static string Dir { get; private set; }
        public static string Log { get; set; }

        static App()
        {
            var assembly = Assembly.GetCallingAssembly();
            var assemblyName = assembly.GetName();

            Name = assemblyName.Name;
            Exe = assembly.Location;
            Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            Dir = AppDomain.CurrentDomain.BaseDirectory;

            var version = assemblyName.Version; // Major.Minor.Build.Revision
            string build = (version.Revision > 0) ? $" build {version.Revision}" : string.Empty;

            Version = version.ToString(3);
            Info = $"{Name} v{Version}";

            if (Environment.UserInteractive)
            {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Log = Path.Combine(appdata, Path.GetFileNameWithoutExtension(Exe) + ".log");
            }
            else
            {
                Log = Path.ChangeExtension(Exe, "log");
            }

            var d = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
            DisplayName = d.Description;
            Description = DisplayName +
                " позволяет обнаруживать проблемы, связанные с работой компонентов сети.\n" +
                $"Если отключить эту службу, сетевые кабели не смогут работать. (v{Version})";

            //var c = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            //string C = c.Copyright.Replace("\u00a9", "(c)");

            //Banner = $"{Name} v{Version}{build} - {DisplayName}\n{C}\nUse -? to get Help\nLogged to {Log}";
            Banner = $"{Name} v{Version}{build} - {DisplayName}\nUse -? to get Help\nLogged to {Log}";
        }
    }
}
