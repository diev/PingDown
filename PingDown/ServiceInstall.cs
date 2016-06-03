using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace PingDown
{
    [RunInstallerAttribute(true)]
    public class ServiceInstall : Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;

        public ServiceInstall()
        {
            // http://www.codeproject.com/Articles/14353/Creating-a-Basic-Windows-Service-in-C

            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem; //.LocalService;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = Program.AppName;
            serviceInstaller.DisplayName = Program.AppDisplayName;
            serviceInstaller.Description = Program.AppDescription;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
