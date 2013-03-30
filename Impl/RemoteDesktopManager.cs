using System;
using System.Linq;
using System.Management;
using EldSharp.SysUtils.Common;

namespace EldSharp.SysUtils.Impl
{
    public class RemoteDesktopManager : IRemoteDesktopManager
    {
        private static bool IsVistaOr7
        {
            get { return Environment.OSVersion.Version.Major >= 6; }
        }

        private static string Namespace
        {
            get
            {
                return IsVistaOr7
                    ? "Root\\CIMV2\\TerminalServices"
                    : "Root\\CIMV2";
            }
        }

        public bool IsEnabled
        {
            get
            {
                using (ManagementObjectSearcher moSearcher = new ManagementObjectSearcher(
                        new ManagementScope(Namespace),
                        new WqlObjectQuery("select * from Win32_TerminalServiceSetting")))
                {
                    using (ManagementObject managementObject = moSearcher.Get().Cast<ManagementObject>().FirstOrDefault())
                    {
                        return managementObject.GetPropertyValue("AllowTSConnections").ToString().Equals("1");
                    }
                }
            }
        }

        public void Disable()
        {
            SetEnabled(false);
        }

        public void Enable()
        {
            SetEnabled(true);
        }

        private static void SetEnabled(bool enable)
        {
            using (ManagementObjectSearcher moSearcher = new ManagementObjectSearcher(
                        new ManagementScope(Namespace),
                        new WqlObjectQuery("select * from Win32_TerminalServiceSetting")))
            {
                using (ManagementObject managementObject = moSearcher.Get().Cast<ManagementObject>().FirstOrDefault())
                {
                    int value = enable ? 1 : 0;
                    managementObject.InvokeMethod("SetAllowTSConnections",
                        IsVistaOr7 ? new object[] { value, value } : new object[] { value });
                }
            }
        }
    }
}
