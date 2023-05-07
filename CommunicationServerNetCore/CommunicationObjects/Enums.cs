using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared.Pipes.CommunicationObjects
{
    public enum MessageTypeEnum
    {
        Info=1,
        Command=2,
        RunningBatchesInfo=3
    }
}
