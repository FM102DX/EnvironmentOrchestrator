using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using Serilog;

namespace ActivityScheduler.Shared.Pipes
{
    
    public class PipeServer : PipeBase
    {
        private NamedPipeServerStream NamedPipeServerStream;
        public event NewMessage NewMessageEvent;
        private Serilog.ILogger _logger;

        public PipeServer(string PipeName, Serilog.ILogger logger)
        {
            this.PipeName = PipeName;
            _logger = logger;
        }

        public override void Run()
        {
            this.NamedPipeServerStream = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous
                );
            //SimpleLogHelper.Instance.WriteLog(LogType.Info, "Начните прослушивание клиентских подключений конвейера");
            this.NamedPipeServerStream.BeginWaitForConnection(new AsyncCallback(PipeServerStart), this.NamedPipeServerStream);
        }

        public override void Stop()
        {
            if (this.NamedPipeServerStream != null)
            {
                this.NamedPipeServerStream.Disconnect();
                this.NamedPipeServerStream.Close();
            }
        }

        public override void SendMessage(string Message)
        {
            if (this.NamedPipeServerStream.IsConnected)
            {
                try
                {
                    byte[] data = encoding.GetBytes(Message);
                    this.NamedPipeServerStream.Write(data, 0, data.Length);
                    this.NamedPipeServerStream.Flush();
                    this.NamedPipeServerStream.WaitForPipeDrain();
                }
                catch
                {
                   // SimpleLogHelper.Instance.WriteLog(LogType.Info, "Не удалось отправить сообщение по конвейеру");
                }
            }
        }

        private void PipeServerStart(IAsyncResult ar)
        {
            var pipeServer = (NamedPipeServerStream)ar.AsyncState;
            pipeServer.EndWaitForConnection(ar);
         //   SimpleLogHelper.Instance.WriteLog(LogType.Info, "Не удалось отправить сообщение по трубопроводу. Новое подключение к трубопроводу прошло успешно.");
            this.NamedPipeServerStream.BeginRead(data, 0, data.Length, new AsyncCallback(PipeReadCallback), this.NamedPipeServerStream);
        }

        private void PipeReadCallback(IAsyncResult ar)
        {
            var pipeServer = (NamedPipeServerStream)ar.AsyncState;
            var count = pipeServer.EndRead(ar);
            if (count > 0)
            {
                string message = encoding.GetString(data, 0, count);
                NewMessageEvent(message);
                pipeServer.BeginRead(data, 0, data.Length, new AsyncCallback(PipeReadCallback), pipeServer);
            }
            else if(count == 0)
            {
             //   SimpleLogHelper.Instance.WriteLog(LogType.Info, "Соединение труб было отсоединено");
                pipeServer.Close();
                pipeServer.Dispose();
                Run();
            }  
        }
    }
}
