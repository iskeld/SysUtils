using System.Diagnostics;

namespace EldSharp.SysUtils.Common
{
    [DebuggerDisplay("{Name}, Owner:{Owner}")]
    public class ProcessInfo
    {
        public string Name { get; private set; }
        public string Owner { get; private set; }

        public ProcessInfo(string name, string owner)
        {
            Name = name;
            Owner = owner;
        }
    }
}
