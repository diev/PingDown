// Copyright (c) 2012-2020 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/PingDown

using System;
using System.Configuration.Install;

namespace PingDown
{
    class ServiceInstallUtility
    {
        //private static readonly string Exe = Assembly.GetExecutingAssembly().Location;
        //private static readonly string Log = Path.ChangeExtension(Exe, "log");

        public static bool Install()
        {
            try
            {
                Program.Log("Install " + Program.AppExe + " Ver." + Program.AppVersion);
                ManagedInstallerClass.InstallHelper(new[] { 
                    "/AssemblyName=" + Program.AppDisplayName,
                    "/LogToConsole=false", 
                    "/LogFile=" + Program.AppLog, 
                    Program.AppExe 
                });
            }
            catch (Exception ex)
            {
                Program.Log(ex.Message);
                return false;
            }
            return true;
        }

        public static bool Uninstall()
        {
            try
            {
                Program.Log("Uninstall " + Program.AppExe + " Ver." + Program.AppVersion);
                ManagedInstallerClass.InstallHelper(new[] { "/u",
                    "/AssemblyName=" + Program.AppDisplayName,
                    "/LogToConsole=false", 
                    "/LogFile=" + Program.AppLog, 
                    Program.AppExe 
                });
            }
            catch (Exception ex)
            {
                Program.Log(ex.Message);
                return false;
            }
            return true;
        }
    }
}
