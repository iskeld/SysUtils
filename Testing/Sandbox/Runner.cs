using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EldSharp.SysUtils.Common;
using EldSharp.SysUtils.Impl;

namespace Sandbox
{
    public class Runner
    {
        public static void Main(string[] args)
        {
            ISystemManager manager = new SystemManager();
            Console.WriteLine(manager.Uptime);
            foreach (var pi in manager.GetProcesses())
            {
                Console.WriteLine(pi.Name + ", " + pi.Owner);
            }
            Console.ReadLine();
        }
    }
}
