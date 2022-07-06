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

namespace PingDown
{
    public readonly struct Messages
    {
        // Gray (default)
        public const string ServiceStarted = "Service started";
        public const string ServiceStopped = "Service stopped";
        public const string ShutdownReceived = "Shutdown received";

        // Yellow
        public const string NewSettings = "New settings";
        public const string ShutdownWanted = "Shutdown wanted";
        public const string PingFlag = "Ping flag";
        public const string DownFlag = "Down flag";
        public const string VersionFlag = "Version flag";
        public const string UpdateFlag = "Update flag";

        // Green
        public const string Passed = "Passed";
        public const string ServiceInstalled = "Service installed";
        public const string ServiceRemoved = "Service removed";

        // Red
        public const string ShutdownStarted = "Shutdown started!";
        public const string ParameterRequired = "Parameter required for option -";
        public const string UnrecognizedOption = "Unrecognized option -";

        public const string FailedInstall = "Failed to install service";
        public const string FailedRemove = "Failed to remove service";

        public const string ErrorWritting = "Error writting to ";
        public const string DrivesBad = "Failed drives: ";
    }
}
