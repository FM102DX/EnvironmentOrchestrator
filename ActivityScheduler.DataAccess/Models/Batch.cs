using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public class Batch:BaseEntity
    {
        public string Number { get; set; } //unique 6-digit number like 100101

        public string Name { get; set; } //Name - english alf without spaces, for example This.is.transaction

        public BatchStartTypeEnum RunMode { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsGroup { get; set; }


    }
}
