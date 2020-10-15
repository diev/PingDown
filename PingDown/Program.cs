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
using System.ServiceProcess;

namespace PingDown
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                RunService();
                return 0;
            }

            if (args.Length > 0)
            {
                string cmd = null;
                bool argreq = false;

                foreach (string arg in args)
                {
                    if (arg.Length > 1 && (arg[0] == '-' || arg[0] == '/'))
                    {
                        if (argreq)
                        {
                            return BadArg(Messages.ParameterRequired + cmd);
                        }

                        cmd = arg.Substring(1).ToLower();
                        switch (cmd)
                        {
                            case "help":
                            case "h":
                            case "?":
                                Usage();
                                return 0;

                            case "install":
                            case "i":
                                return ServiceInstallUtility.Install();

                            case "uninstall":
                            case "u":
                                return ServiceInstallUtility.Uninstall();

                            //case "ping":
                            //case "p":
                            //    cmd = "ping";
                            //    argreq = true;
                            //    break;

                            case "force":
                            case "f":
                                Settings.Force = true;
                                break;

                            default:
                                return BadArg(Messages.UnrecognizedOption + cmd);
                        }
                    }
                    else if (argreq)
                    {
                        switch (cmd)
                        {
                            case "ping":
                            //    Settings.Hosts = Helpers.ReadList(arg);
                            //    Settings.HostsFixed = true;
                                break;

                            default:
                                return BadArg(Messages.ParameterRequired + cmd);
                        }

                        argreq = false;
                    }
                }

                if (argreq)
                {
                    return BadArg(Messages.ParameterRequired + cmd);
                }
            }

            TestService();
            return 0;

            int BadArg(string message)
            {
                #region Output
                Console.WriteLine(message);
                #endregion

                Usage();
                return 1;
            }
        }

        public static void Usage()
        {
            #region Output
            Console.WriteLine(App.Info);
            Console.WriteLine(App.Description);
            Console.WriteLine();
            Console.WriteLine("List of options (with '/' or '-'):");
            Console.WriteLine();
            Console.WriteLine("  -?|h|help\t - This help");
            Console.WriteLine("  -i|install\t - Install as a service");
            Console.WriteLine("  -u|uninstall\t - Remove this service");
            Console.WriteLine();
            //Console.WriteLine("  -p|ping host1,host2\t - Ping these hosts");
            Console.WriteLine("  -f|force\t - Try to shutdown");
            Console.WriteLine();
            Console.WriteLine($"(Expiration: {Helpers.Expiration()})");
            Console.WriteLine();
            #endregion
        }

        public static void RunService()
        {
            var service = new Service
            {
                CanStop = Settings.Services
            };
            var servicesToRun = new ServiceBase[] { service };
            ServiceBase.Run(servicesToRun);
        }

        public static void TestService()
        {
            var service = new Service();

            #region Output
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(App.Banner);
            Console.WriteLine("Press any key to exit ...");
            Console.ResetColor();
            #endregion

            service.StartService();
            Console.ReadKey(true);

            #region Output
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Exit ...");
            Console.ResetColor();
            #endregion

            service.StopService();
        }
    }
}
