using System;

namespace EldSharp.SysUtils.Common
{
    public interface ISystemManager
    {
        TimeSpan Uptime { get; }

        ProcessInfo[] GetProcesses();
        void Reboot();
        void Shutdown();
    }
}
