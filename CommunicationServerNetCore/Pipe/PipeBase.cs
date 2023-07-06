using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared.Pipes
{
    public delegate void NewMessage(string Message);
    public delegate void ConnectFail();
    public class PipeBase
    {
        protected const int PipeInBufferSize = 1024*100;
        protected const int PipeOutBufferSize = 1024*100;
        protected const int MaxThreadServerCount = 1;
        protected Encoding encoding = Encoding.UTF8;
        protected byte[] data = new byte[1024*100];
        protected string PipeName;
        
        public virtual void Run()
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void SendMessage(string Message)
        {

        }
    }
}
