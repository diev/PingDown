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
using System.Configuration.Install;
using System.IO;

namespace PingDown
{
    public class ServiceInstallUtility
    {
        public static int Install()
        {
            App.Log = Path.ChangeExtension(App.Exe, "log");
            Helpers.Log($"Install {App.Exe} v{App.Version}");

            try
            {
                ManagedInstallerClass.InstallHelper(new[] 
                { 
                    "/AssemblyName=" + App.DisplayName,
                    "/LogToConsole=false", 
                    "/LogFile=",
                    App.Exe 
                });
            }
            catch (Exception ex)
            {
                Helpers.Log(ex.Message);
                Helpers.Log(Messages.FailedInstall);

                return 1;
            }

            string state = Path.ChangeExtension(App.Exe, "InstallState");

            if (File.Exists(state))
            {
                File.Delete(state);
            }

            Helpers.Log(Messages.ServiceInstalled);

            return 0;
        }

        public static int Uninstall()
        {
            App.Log = Path.ChangeExtension(App.Exe, "log");
            Helpers.Log($"Uninstall {App.Exe} v{App.Version}");

            try
            {
                ManagedInstallerClass.InstallHelper(new[] 
                {
                    "/u",
                    "/AssemblyName=" + App.DisplayName,
                    "/LogToConsole=false", 
                    "/LogFile=", 
                    App.Exe
                });
            }
            catch (Exception ex)
            {
                Helpers.Log(ex.Message);
                Helpers.Log(Messages.FailedRemove);

                return 1;
            }

            Helpers.Log(Messages.ServiceRemoved);

            return 0;
        }
    }
}
