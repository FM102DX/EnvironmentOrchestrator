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
    public class ServerCommunicationObjectT<T> where T : ISelfSerializableObject, ISelfDeSerializableObject<T>, new()
    {

        public Stack<T> Stack { get; private set; }
        private Serilog.ILogger _logger;
        private PipeServer _clientPipe;
        public string PipeName { get; set; }

        public ServerCommunicationObjectT(string pipeName, Serilog.ILogger logger)
        {
            _logger = logger;
            PipeName = pipeName;
            _clientPipe = new PipeServer(PipeName, _logger);
        }

        public void SendObject(T t)
        {
            var msg = t.Serialize();
            _clientPipe.SendMessage(msg);
            _logger.Debug($"Server of {PipeName} is sending message {msg}");
        }


        public void Run()
        {
            _clientPipe.Run();
        }
    }
}
