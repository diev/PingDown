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

using System.ServiceProcess;

namespace PingDown
{
    public class Service : ServiceBase
    {
        public Service()
        {
            ServiceName = App.Name;
            //CanPauseAndContinue = true;
            //CanStop = true;
        }

        public void StartService()
        {
            JobTimer.Start(Settings.DelayStart, Settings.RepeatEvery);
            Helpers.Log(Messages.ServiceStarted);
        }

        public void StopService()
        {
            JobTimer.Stop();
            Helpers.Log(Messages.ServiceStopped);
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            StartService();
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopService();
        }

        protected override void OnPause()
        {
            base.OnPause();
            JobTimer.Pause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            JobTimer.Continue(0, Settings.RepeatEvery);
        }

        protected override void OnShutdown()
        {
            Helpers.Log(Messages.ShutdownReceived);
            base.OnShutdown();
            StopService();
        }
    }
}
