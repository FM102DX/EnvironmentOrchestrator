using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Core
{
    public class TestService01
    {
        Serilog.ILogger Logger;

        public TestService01(Serilog.ILogger logger) 
        {
            Logger = logger;
        }

        public void Start() { Logger.Information("Starting"); }
        public void Stop() { Logger.Information("Stopping"); }
    }
}
