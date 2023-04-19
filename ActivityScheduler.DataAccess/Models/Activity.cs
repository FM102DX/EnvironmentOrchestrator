using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public  class Activity: BaseEntity
    {
        public string Name { get; set; }

        public int ActivityId { get; set; } // number like 10, 20, 30 ... 450, 460, ets

        public List<int> ParentIds { get; set; }

        public TimeSpan StartTime { get; set; }

        public String TransactionId { get; set; } //number of transaction in this script / foreign script

        public bool IsDomestic { get; set; } //wether domestic transaction should be called
        public bool IsHub { get; set; } //wether this transaction is a hub (hubs do nothing but become successful to start child activities)

        public TimeSpan ChildDelay { get; set; } // starttime that's passed to another script as parameter, is for ChildDelay more than fact starttime

        public bool AlwaysSuccess { get; set; }

        public ActivityParentRuleEnum ActivityParentRule { get; set; }

    }
}
