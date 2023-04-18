using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Pipes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ActivityScheduler.Shared.Pipes
{
    public class PipeListener
    {

        private Serilog.ILogger _logger;

        private PipeClientHelper _clientPipe;
        private string _pipeName;

        public PipeListener(string pipeName, Serilog.ILogger logger)
        {
            _logger = logger;
            _pipeName = pipeName;
            _clientPipe = new PipeClientHelper(_pipeName);
            _clientPipe.NewMessageEvent += _clientPipe_NewMessageEvent;
            _clientPipe.ConnectFailEvent += _clientPipe_ConnectFailEvent;

        }

        private void _clientPipe_ConnectFailEvent()
        {
            _logger.Error($"Failed to connect to server pipe");
            Thread.Sleep(3000);
            Run();
        }

        private void _clientPipe_NewMessageEvent(string Message)
        {
           _logger.Information($"Got mail: {Message}");
        }

        public void Run() 
        {
            _clientPipe.Run();
        }
    }
}
