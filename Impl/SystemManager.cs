using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using EldSharp.SysUtils.Common;

namespace EldSharp.SysUtils.Impl
{
    public class SystemManager : ISystemManager
    {
        public TimeSpan Uptime
        {
            get
            {
                using (PerformanceCounter uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();       //Call this an extra time before reading its value
                    return TimeSpan.FromSeconds(uptime.NextValue());
                }
            }
        }

        public ProcessInfo[] GetProcesses()
        {
            ProcessInfo[] result;
            using (ManagementObjectSearcher moSearcher = new ManagementObjectSearcher(
                        new WqlObjectQuery("select * from Win32_Process")))
            {
                using (ManagementObjectCollection processes = moSearcher.Get())
                {
                    result = new ProcessInfo[processes.Count];
                    int i = 0;
                    foreach (ManagementObject process in processes)
                    {
                        object[] arglist = new object[] { String.Empty, String.Empty, String.Empty };
                        process.InvokeMethod("GetOwner", arglist);
                        string name = process.GetPropertyValue("Name").ToString();
                        string owner = arglist[0] != null ? arglist[0].ToString() : String.Empty;
                        result[i++] = new ProcessInfo(name, owner);
                    }
                }
            }

            return result;
        }

        public void Reboot()
        {
            ExitWindowsInternal(ExitMode.Reboot | ExitMode.ForceIfHung,
                ShutdownReason.MajorOther | ShutdownReason.MinorOther | ShutdownReason.FlagPlanned);
        }

        public void Shutdown()
        {
            ExitWindowsInternal(ExitMode.ShutDown | ExitMode.ForceIfHung,
                ShutdownReason.MajorOther | ShutdownReason.MinorOther | ShutdownReason.FlagPlanned);
        }

        private static void ExitWindowsInternal(ExitMode mode, ShutdownReason reason)
        {
            IntPtr tokenHandle = IntPtr.Zero;

            try
            {
                // get process token
                if (!Interop.OpenProcessToken(Process.GetCurrentProcess().Handle,
                    Interop.TOKEN_QUERY | Interop.TOKEN_ADJUST_PRIVILEGES,
                    out tokenHandle))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to open process token handle");
                }

                // lookup the shutdown privilege
                var tokenPrivs = new Interop.TOKEN_PRIVILEGES();
                tokenPrivs.PrivilegeCount = 1;
                tokenPrivs.Privileges = new Interop.LUID_AND_ATTRIBUTES[1];
                tokenPrivs.Privileges[0].Attributes = Interop.SE_PRIVILEGE_ENABLED;

                if (!Interop.LookupPrivilegeValue(null,
                    Interop.SE_SHUTDOWN_NAME,
                    out tokenPrivs.Privileges[0].Luid))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to open lookup shutdown privilege");
                }

                // add the shutdown privilege to the process token
                if (!Interop.AdjustTokenPrivileges(tokenHandle,
                    false,
                    ref tokenPrivs,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to adjust process token privileges");
                }

                // reboot
                if (!Interop.ExitWindowsEx(mode, reason))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to reboot system");
                }
            }
            finally
            {
                // close the process token
                if (tokenHandle != IntPtr.Zero)
                {
                    Interop.CloseHandle(tokenHandle);
                }
            }
        }
    }
}
