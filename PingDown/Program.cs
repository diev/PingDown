// Copyright (c) 2012-2020 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/PingDown

//using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
//using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace PingDown
{
    class Program
    {
        public static readonly string AppName = Assembly.GetCallingAssembly().GetName().Name;
        public static readonly string AppDisplayName = "Служба сетевых кабелей";
        public static readonly string AppDescription = AppDisplayName +
            " позволяет обнаруживать проблемы, связанные с работой компонентов сети.\n" +
            "Если отключить эту службу, сетевые кабели не смогут работать.";
        public static readonly string AppVersion = Assembly.GetCallingAssembly().GetName().Version.ToString(3);
        //public static readonly string AppRegistry = @"SYSTEM\CurrentControlSet\Services\" + AppName;
        //public static readonly string AppParameters = "Parameters";
        public static readonly string AppDir = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string AppExe = Assembly.GetCallingAssembly().Location;
        //public static readonly string LogFile = AppDomain.CurrentDomain.BaseDirectory + @"\" + AppName + ".log";
        //public static string AppLog = Path.ChangeExtension(AppExe, "log");
        public static string AppLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Path.GetFileNameWithoutExtension(AppExe) + ".log");
        //public static string AppLog = AppExe + ".log";

        /// <summary>
        /// Option: Ping only (safely)
        /// </summary>
        public static bool PingOnly = false;
        /// <summary>
        /// Option: Test as reality (danger to switch off!)
        /// </summary>
        public static bool TestReal = false;

        public static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                RunService(args);
                Environment.Exit(0);
            }

            string info = AppName + " Ver." + AppVersion;
            Console.WriteLine(info);

            if (args.Length == 0)
            {
                Usage();
            }

            string cmd = null;
            bool argreq = false;

            foreach (string arg in args)
            {
                if (arg.Length > 1 && (arg[0] == '-' || arg[0] == '/'))
                {
                    if (argreq)
                    {
                        Console.WriteLine("Parameter required for option -" + cmd);
                        Usage();
                    }

                    cmd = arg.Substring(1).ToLower();
                    switch (cmd)
                    {
                        case "help":
                        case "h":
                        case "?":
                            Usage();
                            break;

                        case "install":
                        case "i":
                            Log(ServiceInstallUtility.Install()
                                ? "Service installed"
                                : "Failed to install service");
                            break;

                        case "uninstall":
                        case "u":
                            Log(ServiceInstallUtility.Uninstall()
                                ? "Service removed"
                                : "Failed to remove service");
                            break;

                        //case "store":
                        //case "s":
                        //    cmd = "store";
                        //    argreq = true;
                        //    break;

                        case "ping":
                        case "p":
                            cmd = "ping";
                            argreq = true;
                            TestReal = true;
                            PingOnly = true;
                            break;

                        case "test":
                        case "t":
                            cmd = "test";
                            argreq = true;
                            TestReal = true;
                            break;

                        default:
                            Console.WriteLine("Unrecognized option -" + cmd);
                            Usage();
                            break;
                    }
                }
                else if (argreq)
                {
                    switch (cmd)
                    {
                        //case "store":
                        //    Log("Store parameters " + arg + " into registry");
                        //    SaveParameters(arg);
                        //    break;

                        case "ping":
                            Log("Option: Ping only");
                            string[] p = { arg };
                            RunService(p);
                            break;

                        case "test":
                            Log("Option: Test as reality");
                            string[] t = { arg };
                            RunService(t);
                            break;

                        default:
                            Console.WriteLine("Parameter required for option -" + cmd);
                            Usage();
                            break;
                    }

                    argreq = false;
                }
            }

            if (argreq)
            {
                Console.WriteLine("Parameter required for option -" + cmd);
                Usage();
            }

            Environment.Exit(0);
        }

        public static void Usage(int ExitCode = 1)
        {
            Console.WriteLine(AppDescription);
            Console.WriteLine();
            Console.WriteLine("List of options (with '/' or '-'):");
            Console.WriteLine();
            Console.WriteLine("\t-?|h|help\t - This help");
            Console.WriteLine("\t-i|install\t - Install as a service");
            Console.WriteLine("\t-u|uninstall\t - Remove this service");
            Console.WriteLine();
            //Console.WriteLine("\t-s|store host1,host2,host3\t - Store hosts into registry (admin rights required)");
            //Console.WriteLine();
            //Console.WriteLine("Test hosts specified here else taken from registry/config:");
            Console.WriteLine("Test hosts specified here else taken from config:");
            Console.WriteLine();
            Console.WriteLine("\t-p|ping host1,host2,host3|-\t - Ping only (safely)");
            Console.WriteLine("\t-t|test host1,host2,host3|-\t - Test as reality (danger to switch off!)");
            Console.WriteLine();
            Console.WriteLine("Expiration: " + Expiration());

            Environment.Exit(ExitCode);
        }

        public static void RunService(string[] args)
        {
            var service = new Service();
            var servicesToRun = new ServiceBase[] { service };

            if (Environment.UserInteractive)
            {
                service.StartService(args);
                Console.WriteLine("Logged to " + AppLog);
                Console.WriteLine("Press Enter to exit ...");
                Console.ReadLine();
                Console.WriteLine("Exit ...");
                service.StopService();
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }
 
        //public static void SaveParameters(string s)
        //{
        //    // http://www45.brinkster.com/b3ck/code/CreatingADotNetServiceBaseInstaller.html
        //    // http://stackoverflow.com/questions/255056/install-a-net-windows-service-without-installutil-exe
        //    // http://www.dotnet247.com/247reference/msgs/43/219565.aspx

        //    using (RegistryKey oKey = Registry.LocalMachine.OpenSubKey(AppRegistry, true))
        //    {
        //        if (oKey == null)
        //        {
        //            Log(@"No rights to write into HKLM\" + AppRegistry);
        //            return;
        //        }

        //        try
        //        {
        //            //Object sValue = oKey.GetValue("ImagePath");
        //            //oKey.SetValue("ImagePath", sValue);

        //            RegistryKey sParams = oKey.CreateSubKey(AppParameters);
        //            sParams.SetValue("Hosts", s, RegistryValueKind.String);
        //            Log("Hosts " + s + " stored to registry");
        //        }
        //        catch (Exception ex)
        //        {
        //            Log("Registry error: " + ex.Message);
        //        }
        //    }
        //}

        public static void Log(string s, bool stamped = true)
        {
            if (!Environment.UserInteractive)
            {
                return;
            }

            bool newln = s.StartsWith("\n");

            StringBuilder sb = new StringBuilder();
            if (newln)
            {
                sb.AppendLine();
            }
            if (stamped)
            {
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss " /*.ff"*/));
            }
            if (newln)
            {
                sb.Append(s.Substring(1));
            }
            else
            {
                sb.Append(s);
            }
            string info = sb.ToString();

            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppLog, true);
                if (Environment.UserInteractive)
                {
                    Console.WriteLine(info);
                }
                sw.WriteLine(info);
                sw.Flush();
                sw.Close();
                sw = null;
            }
            catch (Exception ex)
            {
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Error writting to Log " + AppLog);
                    Console.WriteLine(ex.Message);
                }
                //string oldLog = AppLog;
                //string newPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                //AppLog = Path.Combine(newPath, Path.GetFileName(AppLog));

                //if (Environment.UserInteractive)
                //{
                //    Console.WriteLine("Log changed to " + AppLog);

                //    Console.WriteLine("Fail to write into Log " + oldLog);
                //    Console.WriteLine(ex.Message);
                //    Console.WriteLine("Log changed to " + AppLog);
                //    Console.WriteLine(info);
                //    Console.WriteLine("Press Enter to exit...");
                //    Console.ReadLine();
                //}
                //else if (AppLog.Equals(oldLog))
                //{
                //    // No more ways to inform the user about this error...
                //}
                //else
                //{ 
                //    Log(s); // a second try to write into another log
                //}

                //EventLog.WriteEntry(ex.Message, EventLogEntryType.Error);

                /********************************************************************
                // Create the source, if it does not already exist.
                if (!EventLog.SourceExists("MySource"))
                {
                    //An event log source should not be created and immediately used.
                    //There is a latency time to enable the source, it should be created
                    //prior to executing the application that uses the source.
                    //Execute this sample a second time to use the new source.
                    EventLog.CreateEventSource("MySource", "MyNewLog");
                    Console.WriteLine("CreatedEventSource");
                    Console.WriteLine("Exiting, execute the application a second time to use the source.");
                    // The source is created.  Exit the application to allow it to be registered.
                    return;
                }

                // Create an EventLog instance and assign its source.
                EventLog myLog = new EventLog();
                myLog.Source = "MySource";

                // Write an informational entry to the event log.    
                myLog.WriteEntry("Writing to event log.");
                ********************************************************************/
            }

            // https://msdn.microsoft.com/library/ms182334.aspx

            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
        }

        public static string Expiration()
        {
            return "EXP" + DateTime.Now.AddDays(7).ToString("yy-MM-dd");
        }
    }
}
