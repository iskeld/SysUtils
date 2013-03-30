namespace EldSharp.SysUtils.Common
{
    public interface IRemoteDesktopManager
    {
        bool IsEnabled { get; }
        void Disable();
        void Enable();
    }
}
