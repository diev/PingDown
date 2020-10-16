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

using System.Threading;

namespace PingDown
{
    public class JobTimer
    {
        private static Timer _timer;

        private static int _savedDueTime = 0;
        private static int _savedPeriod = 0;

        public static void Start(int dueTime, int period)
        {
            AutoResetEvent timerEvent = new AutoResetEvent(false);
            TimerCallback timerCallback = Elapsed;

            _timer = new Timer(timerCallback, timerEvent, dueTime, period);

            _savedDueTime = dueTime;
            _savedPeriod = period;
        }

        public static void Stop()
        {
            _timer.Dispose();
        }

        public static void Pause()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public static void Continue()
        {
            _timer.Change(_savedDueTime, _savedPeriod);
        }

        public static void Continue(int dueTime, int period)
        {
            _timer.Change(dueTime, period);

            _savedDueTime = dueTime;
            _savedPeriod = period;
        }

        private static void Elapsed(object stateInfo)
        {
            Job.CheckState();
        }
    }
}
