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
        public string Number { get; set; } = "000000"; //unique 6-digit number like 100101

        public string Name { get; set; } = "DefaultBatchName"; //Name - english alf without spaces, for example This.is.transaction

        public BatchStartTypeEnum RunMode { get; set; } = BatchStartTypeEnum.Single;
        public BatchStartPointTypeEnum StartPointType { get; set; } = BatchStartPointTypeEnum.StartFromNow;
        public DateTime StartDateTime { get; set; }

        public TimeSpan StartTimeInADay { get; set; }

        public TimeSpan Interval { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsGroup { get; set; }

        public string? ActiveDaysOfWeek { get; set; }

        public string? DefaultScriptPath { get; set; }

    }
}
