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
// http://www.codeproject.com/Articles/14353/Creating-a-Basic-Windows-Service-in-C
#endregion

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace PingDown
{
    [RunInstaller(true)]
    public class ServiceInstall : Installer
    {
        private readonly ServiceProcessInstaller serviceProcessInstaller;
        private readonly ServiceInstaller serviceInstaller;

        public ServiceInstall()
        {
            serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
                Username = null,
                Password = null
            };

            serviceInstaller = new ServiceInstaller
            {
                ServiceName = App.Name,
                DisplayName = App.DisplayName,
                Description = App.Description,
                StartType = ServiceStartMode.Automatic,
                DelayedAutoStart = true
            };

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
