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
// https://msdn.microsoft.com/library/ms182161.aspx
// https://msdn.microsoft.com/library/ms182319.aspx

// http://miromannino.it/exitwindowsex-in-c/
// http://stackoverflow.com/questions/102567/how-to-shutdown-the-computer-from-c-sharp
// http://msdn.microsoft.com/en-us/library/windows/desktop/aa376868(v=vs.85).aspx
// http://www.gotdotnet.ru/files/160/

// http://rsdn.ru/article/baseserv/svcadmin-1.xml
// http://rsdn.ru/article/baseserv/svcadmin-2.xml
#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class ExitWindows
{
    internal static class NativeMethods
    {
        internal struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        internal struct LUID_AND_ATTRIBUTES
        {
            public LUID pLuid;
            public int Attributes;
        }

        internal struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        [DllImport("advapi32.dll")]
        internal static extern int OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState, UInt32 BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int ExitWindowsEx(uint uFlags, uint dwReason);

        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const short SE_PRIVILEGE_ENABLED = 2;
        internal const short TOKEN_ADJUST_PRIVILEGES = 32;
        internal const short TOKEN_QUERY = 8;

        internal static void GetPrivileges()
        {
            TOKEN_PRIVILEGES tkp;

            OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr hToken);
            tkp.PrivilegeCount = 1;
            tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
            LookupPrivilegeValue("", SE_SHUTDOWN_NAME, out tkp.Privileges.pLuid);
            AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero);
        }
    }

    const ushort EWX_LOGOFF = 0;
    const ushort EWX_POWEROFF = 0x00000008;
    const ushort EWX_REBOOT = 0x00000002;
    const ushort EWX_SHUTDOWN = 0x00000001;
    const ushort EWX_FORCE = 0x00000004;

    public static void Shutdown(bool force = false)
    {
        NativeMethods.GetPrivileges();
        NativeMethods.ExitWindowsEx(EWX_SHUTDOWN | (uint)(force ? EWX_FORCE : 0) | EWX_POWEROFF, 0);
    }

    public static void Reboot(bool force = false)
    {
        NativeMethods.GetPrivileges();
        NativeMethods.ExitWindowsEx(EWX_REBOOT | (uint)(force ? EWX_FORCE : 0), 0);
    }

    public static void LogOff(bool force = false)
    {
        NativeMethods.GetPrivileges();
        NativeMethods.ExitWindowsEx(EWX_LOGOFF | (uint)(force ? EWX_FORCE : 0), 0);
    }
}
