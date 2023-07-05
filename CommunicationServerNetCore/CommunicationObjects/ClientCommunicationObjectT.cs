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
    public class ClientCommunicationObjectT<T> where T : ISelfSerializableObject, ISelfDeSerializableObject<T>, new()
    {
        private Stack<T> Stack { get; set; } = new Stack<T>();
        private Serilog.ILogger _logger;
        private PipeClient _clientPipe;
        public string PipeName { get; set; }

        public ClientCommunicationObjectT(string pipeName, Serilog.ILogger logger)
        {
            _logger = logger;
            PipeName = pipeName;
        }

        private void _clientPipe_ConnectFailEvent()
        {
            _logger.Error($"Failed to connect to server pipe {PipeName}");
            Thread.Sleep(3000);
            Run();
        }
        public int StackCount { get => Stack.Count; }
        private void _clientPipe_NewMessageEvent(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            T t = new T();
            T t1 = t.GetObject(message);
            Stack.Push (t1);
            _logger.Debug($"Got mail: new message from pipe {PipeName}, now {Stack.Count} messages in queue, message content is: {message}");
        }

        public void Run()
        {
            _clientPipe = new PipeClient(PipeName, _logger);
            _clientPipe.NewMessageEvent += _clientPipe_NewMessageEvent;
            _clientPipe.ConnectFailEvent += _clientPipe_ConnectFailEvent;
            _clientPipe.Run();
        }

        public T? Take()
        {
            _logger.Debug($"Taking incoming mail from stack, now {Stack.Count} messages in queue");

            if (Stack.Count == 0) return default(T);
            
            return Stack.Pop();
        }
    }
}
