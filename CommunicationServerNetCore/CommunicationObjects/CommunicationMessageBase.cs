using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared.Pipes
{
    public class CommunicationMessageBase
    {
        public Guid Id { get; set; }

        public string? Message { get; set; }

        public CommonOperationResult? Result { get; set; }

        public string? Command { get; set; }

        public CommunicationMessageBase()
        {
            Id= new Guid();
        }
    }
}
