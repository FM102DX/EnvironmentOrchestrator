using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public class Batch:BaseEntity
    {
        public string Number { get; set; } = "000000"; //unique 6-digit number like 100101

        public string Name { get; set; } = "DefaultBatchName"; //Name - english alf without spaces, for example This.is.transaction

        [NotMapped]
        public List<Activity>? Activities { get; set; }

        public BatchStartTypeEnum RunMode { get; set; } = BatchStartTypeEnum.Single;
        public BatchStartPointTypeEnum StartPointType { get; set; } = BatchStartPointTypeEnum.StartFromNow;
        public DateTime StartDateTime { get; set; }

        public TimeSpan StartTimeInADay { get; set; }

        public TimeSpan Interval { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan Timeout { get; set; }
        public bool IsDateAnActiveDay (DateTime date)
        {
            switch(date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return ActiveDaysOfWeek[6] == '1';
                case DayOfWeek.Monday:
                    return ActiveDaysOfWeek[0] == '1';
                case DayOfWeek.Tuesday:
                    return ActiveDaysOfWeek[1] == '1';
                case DayOfWeek.Wednesday:
                    return ActiveDaysOfWeek[2] == '1';
                case DayOfWeek.Thursday:
                    return ActiveDaysOfWeek[3] == '1';
                case DayOfWeek.Friday:
                    return ActiveDaysOfWeek[4] == '1';
                case DayOfWeek.Saturday:
                    return ActiveDaysOfWeek[5] == '1';
                default:
                    return false;
            }
        }
        public bool IsGroup { get; set; }

        public string ActiveDaysOfWeek { get; set; } = "0000000";

        public string? ScriptPath { get; set; }

        public bool HasInterval => Interval.TotalMilliseconds == 0;
        public bool HasDuration => Duration.TotalMilliseconds == 0;
        public bool HasTimeout => Timeout.TotalMilliseconds == 0;

        public BatchStatusEnum Status { get; set; }

    }
}
