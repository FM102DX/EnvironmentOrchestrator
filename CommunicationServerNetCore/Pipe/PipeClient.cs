using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.IO;

namespace ActivityScheduler.Shared.Pipes
{
    
    public class PipeClient : PipeBase
    {
        public event NewMessage NewMessageEvent;
        public event ConnectFail ConnectFailEvent;
        private NamedPipeClientStream NamedPipeClientStream;
        private Serilog.ILogger _logger;

        public PipeClient(string PipeName, Serilog.ILogger logger)
        {
            this.PipeName = PipeName;
            _logger = logger;
        }

        public bool IsConnected
        {
            get
            {
                if(this.NamedPipeClientStream != null)
                {
                    return this.NamedPipeClientStream.IsConnected;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void Run()
        {
            try
            {
                this.NamedPipeClientStream = new NamedPipeClientStream(".", this.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                this.NamedPipeClientStream.Connect(200);
                this.NamedPipeClientStream.ReadMode = PipeTransmissionMode.Message;
                this.NamedPipeClientStream.BeginRead(data, 0, data.Length, new AsyncCallback(PipeReadCallback), this.NamedPipeClientStream);
            }
            catch
            {
                ConnectFailEvent?.Invoke();
            }
        }

        public override void Stop()
        {
            if (this.NamedPipeClientStream != null)
            {
                this.NamedPipeClientStream.Close();
            }
        }

        public override void SendMessage(string Message)
        {
            if (this.NamedPipeClientStream.IsConnected)
            {
                try
                {
                    byte[] data = encoding.GetBytes(Message);
                    this.NamedPipeClientStream.Write(data, 0, data.Length);
                    this.NamedPipeClientStream.Flush();
                    this.NamedPipeClientStream.WaitForPipeDrain();
                }
                catch
                {
                    //Запись журнала
                }
            }
        }

        private void PipeReadCallback(IAsyncResult ar)
        {
            this.NamedPipeClientStream = (NamedPipeClientStream)ar.AsyncState;
            var count = this.NamedPipeClientStream.EndRead(ar);
            if (count > 0)
            {
                string message = encoding.GetString(data, 0, count);
                NewMessageEvent?.Invoke(message);
                this.NamedPipeClientStream.BeginRead(data, 0, data.Length, new AsyncCallback(PipeReadCallback), this.NamedPipeClientStream);
            }
            else if (count == 0)
            {
                ConnectFailEvent?.Invoke();
            }
        }

    }
}
