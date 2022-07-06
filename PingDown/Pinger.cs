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
// https://stackoverflow.com/questions/13634868/get-the-default-gateway
#endregion

using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace PingDown
{
    public static class Pinger
    {
        private static readonly byte[] _buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static int _timeout;
        private static PingOptions _options;
        private static string _selectedHost;

        public static bool Ready()
        {
            bool result = false;
            var waiter = new ManualResetEvent(false);
            var ping = new Ping();

            ping.PingCompleted += (s, e) =>
            {
                ping.Dispose();

                if (e.Reply.Status == IPStatus.Success)
                {
                    Helpers.LogInteractive("Ready");
                    result = true;
                    waiter.Set();
                }
            };

            _timeout = Settings.Timeout;
            _options = new PingOptions(Settings.Gateways, true);
            ping.SendAsync("127.0.0.1", _timeout, _buffer, _options, waiter);
            waiter.WaitOne();
            return result;
        }

        public static bool Send()
        {
            _timeout = Settings.Timeout;
            _options = new PingOptions(Settings.Gateways, true);

            if (Send(_selectedHost ?? Settings.Hosts[0]))
            {
                return true;
            }

            if (Send(Settings.Hosts, Settings.AlarmRetries))
            {
                return true;
            }

            return false;
        }

        public static bool Send(string host)
        {
            bool result = false;
            var waiter = new ManualResetEvent(false);
            var ping = new Ping();

            ping.PingCompleted += (s, e) =>
            {
                ping.Dispose();

                if (e.Reply.Status == IPStatus.Success)
                {
                    Helpers.LogInteractive(host);
                    result = true;
                }
                else
                {
                    Helpers.LogInteractive(host + " failed!");
                }

                waiter.Set();
            };

            ping.SendAsync(host, _timeout, _buffer, _options, waiter);
            waiter.WaitOne();
            return result;
        }
        
        public static bool Send(string[] hosts, int retries)
        {
            bool result = false;

            for (int i = retries; i > 0; i--)
            {
                Helpers.LogInteractive($"Retries: {i}");
                var waiter = new ManualResetEvent(false);
                var sync = new object();
                int counter = hosts.Length;

                foreach (string h in hosts)
                {
                    string host = h; // required to pass the value to a delegate!
                    var ping = new Ping();

                    ping.PingCompleted += (s, e) =>
                    {
                        lock (sync)
                        {
                            ping.Dispose();

                            if (e.Reply.Status == IPStatus.Success)
                            {
                                Helpers.LogInteractive(host);

                                if (!host.StartsWith("127.") && !host.Equals("0.0.0.0"))
                                {
                                    _selectedHost = host;
                                }

                                result = true;
                                waiter.Set();
                            }
                            else
                            {
                                Helpers.LogInteractive(host + " fail!");
                            }

                            if (--counter == 0)
                            {
                                waiter.Set();
                            }
                        }
                    };

                    ping.SendAsync(host, _timeout, _buffer, _options, waiter);
                }

                waiter.WaitOne();

                if (result)
                {
                    return result;
                }

                Thread.Sleep(Settings.AlarmEvery);
            }

            return result;
        }

        public static string[] GetGateways()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .Select(a => a.ToString())
                .ToArray();
        }
    }
}
