using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivityScheduler.Shared.Pipes;
using Newtonsoft.Json;

namespace ActivityScheduler.Shared.Pipes
{
    public class AppToWorkerMessage: CommunicationMessageBase, ISelfSerializableObject, ISelfDeSerializableObject<AppToWorkerMessage>
    {
        public AppToWorkerMessage() :base()
        { 

        }

        public AppToWorkerMessage? GetObject(string message)
        {
            AppToWorkerMessage? mgs = JsonConvert.DeserializeObject<AppToWorkerMessage>(message);
            return mgs;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
        public string? TransactionId { get; set; }
        public DateTime? StartTime { get; set; }
    }
}
