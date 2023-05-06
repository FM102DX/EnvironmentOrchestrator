using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ActivityScheduler.Shared.Pipes
{
    public class WorkerToAppMessage : CommunicationMessageBase, ISelfSerializableObject, ISelfDeSerializableObject<WorkerToAppMessage>
    {


        public WorkerToAppMessage() : base()
        { 

        }

        public WorkerToAppMessage? GetObject(string message)
        {
            WorkerToAppMessage? mgs = JsonConvert.DeserializeObject<WorkerToAppMessage>(message);
            return mgs;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
