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
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct LUID
        {
            public int LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct LUID_AND_ATTRIBUTES
        {
            public LUID pLuid;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        [DllImport("advapi32.dll", ExactSpelling = true)]
        internal static extern int OpenProcessToken(
            IntPtr ProcessHandle,
            int DesiredAccess,
            out IntPtr TokenHandle);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int LookupPrivilegeValue(
            string lpSystemName,
            string lpName,
            out LUID lpLuid);

        /// <summary>
        /// Initiates a shutdown and optional restart of the specified computer,
        /// and optionally records the reason for the shutdown.
        /// </summary>
        /// <param name="lpMachineName">The network name of the computer to be shut down.
        /// If lpMachineName is NULL or an empty string, the function shuts down the local computer.</param>
        /// <param name="lpMessage">The message to be displayed in the shutdown dialog box.
        /// This parameter can be NULL if no message is required.</param>
        /// <param name="dwTimeout">The length of time that the shutdown dialog box should be displayed,
        /// in seconds. 
        /// While this dialog box is displayed, shutdown can be stopped by the AbortSystemShutdown function.</param>
        /// <param name="bForceAppsClosed">If this parameter is TRUE, applications with unsaved changes
        /// are to be forcibly closed. If this parameter is FALSE, the system displays a dialog box 
        /// instructing the user to close the applications.</param>
        /// <param name="bRebootAfterShutdown">If this parameter is TRUE, the computer is to restart
        /// immediately after shutting down. If this parameter is FALSE, the system flushes all caches 
        /// to disk and safely powers down the system.</param>
        /// <param name="dwReason">The reason for initiating the shutdown. 
        /// This parameter must be one of the system shutdown reason codes.</param>
        /// <returns>If the function succeeds, the return value is nonzero. 
        /// If the function fails, the return value is zero.To get extended error information, 
        /// call GetLastError.</returns>
        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool InitiateSystemShutdownExW( // System.Diagnostics.Process.Start("Shutdown", "-s -f -t 0");
            string lpMachineName, // shut down local computer
            string lpMessage, // message for user
            uint dwTimeout, // time-out period, in seconds 
            [MarshalAs(UnmanagedType.Bool)] bool bForceAppsClosed, // ask user to close apps 
            [MarshalAs(UnmanagedType.Bool)] bool bRebootAfterShutdown, // reboot after shutdown 
            uint dwReason);

        //[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        //internal static extern int ExitWindowsEx(
        //    uint uFlags,
        //    uint dwReason);

        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002; //2

        internal const int TOKEN_QUERY = 0x00000008; //8
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020; //32

        //internal static bool GetPrivileges()
        //{
        //    // Get a token for this process.

        //    IntPtr hProc = Process.GetCurrentProcess().Handle;
        //    if (OpenProcessToken(hProc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr hToken) == 0)
        //    {
        //        return false;
        //    }

        //    // Get the LUID for the shutdown privilege.

        //    TOKEN_PRIVILEGES tkp;
        //    LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out tkp.Privileges.pLuid);

        //    tkp.PrivilegeCount = 1; // one privilege to set
        //    tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

        //    // Get the shutdown privilege for this process.

        //    return AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero);
        //}

        internal static bool InitialShutdown(uint timeout)
        {
            // Get a token for this process.

            IntPtr hProc = Process.GetCurrentProcess().Handle;
            if (OpenProcessToken(hProc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr hToken) == 0)
            {
                return false;
            }

            // Get the LUID for the shutdown privilege.

            TOKEN_PRIVILEGES tkp;
            LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out tkp.Privileges.pLuid);

            tkp.PrivilegeCount = 1; // one privilege to set
            tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;

            // Get the shutdown privilege for this process.

            if (!AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero))
            {
                return false;
            }

            // Display the shutdown dialog box and start the countdown (nothing, if timeout = 0).

            bool ok = InitiateSystemShutdownExW(null, null, timeout, true, false,
                SHTDN_REASON_MAJOR_SYSTEM | SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY);

            // Disable shutdown privilege. 

            tkp.Privileges.Attributes = 0;
            AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero, IntPtr.Zero);

            return ok;
        }
    }

    //internal const uint EWX_LOGOFF = 0x00000000; //0
    //internal const uint EWX_SHUTDOWN = 0x00000001;
    //internal const uint EWX_REBOOT = 0x00000002;
    //internal const uint EWX_FORCE = 0x00000004;
    //internal const uint EWX_POWEROFF = 0x00000008;
    //internal const uint EWX_FORCEIFHUNG = 0x00000010;

    // "Loss of network connectivity (Unplanned)" The computer needs to be shut down due to a network connectivity issue.

    internal const uint SHTDN_REASON_MAJOR_SYSTEM = 0x00050000;
    internal const uint SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY = 0x00000014;

    // Shut down the system and force all applications to close.

    public static bool Shutdown(uint timeout = 0)
    {
        return NativeMethods.InitialShutdown(timeout);
    }

    //public static bool Shutdown(bool force = false)
    //{
    //    uint forced = force ? EWX_FORCE | EWX_FORCEIFHUNG : 0;
    //    uint reason = SHTDN_REASON_MAJOR_SYSTEM | SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY;

    //    return
    //        NativeMethods.GetPrivileges() &&
    //        NativeMethods.ExitWindowsEx(EWX_SHUTDOWN | forced | EWX_POWEROFF, reason) != 0;
    //}

    //public static bool Reboot(bool force = false)
    //{
    //    uint forced = force ? EWX_FORCE | EWX_FORCEIFHUNG : 0;
    //    uint reason = SHTDN_REASON_MAJOR_SYSTEM | SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY;

    //    return
    //        NativeMethods.GetPrivileges() &&
    //        NativeMethods.ExitWindowsEx(EWX_REBOOT | forced, reason) != 0;
    //}

    //public static bool LogOff(bool force = false)
    //{
    //    uint forced = force ? EWX_FORCE | EWX_FORCEIFHUNG : 0;
    //    uint reason = SHTDN_REASON_MAJOR_SYSTEM | SHTDN_REASON_MINOR_NETWORK_CONNECTIVITY;

    //    return
    //        NativeMethods.GetPrivileges() &&
    //        NativeMethods.ExitWindowsEx(EWX_LOGOFF | forced, reason) != 0;
    //}
}
