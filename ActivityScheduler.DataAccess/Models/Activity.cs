using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public  class Activity: BaseEntity
    {
        public string Name { get; set; }
        public Guid BatchId { get; set; }

        public int ActivityId { get; set; } // number like 10, 20, 30 ... 450, 460, ets
        
        [NotMapped]
        public List<int> ParentIds { get; set; }

        public TimeSpan StartTime { get; set; }

        public String TransactionId { get; set; } //number of transaction in this script / foreign script

        public bool IsDomestic { get; set; } //wether domestic transaction should be called
        public bool IsHub { get; set; } //wether this transaction is a hub (hubs do nothing but become successful to start child activities)

        public TimeSpan ChildDelay { get; set; } // starttime that's passed to another script as parameter, is for ChildDelay more than fact starttime

        public bool AlwaysSuccess { get; set; }

        public ActivityParentRuleEnum ActivityParentRule { get; set; }

        public ActivityGridViewModel AsViewModel()
        {
            ActivityGridViewModel rez= new ActivityGridViewModel();
            rez.Id = Id;
            rez.Name = Name;
            rez.ActivityId = ActivityId;
            rez.AlwaysSuccess = AlwaysSuccess;
            rez.StartTime = StartTime;
            rez.TransactionId = TransactionId;
            rez.ChildDelay = ChildDelay;
            rez.IsDomestic = IsDomestic;
            rez.IsHub = IsHub;
            return rez;
        }
        public override Activity Clone()
        {
            Activity acv = new Activity();
            acv.Id = Id;
            acv.Name = Name;
            acv.ActivityId = ActivityId;
            acv.AlwaysSuccess = AlwaysSuccess;
            acv.StartTime = StartTime;
            acv.TransactionId = TransactionId;
            acv.IsDomestic= IsDomestic;
            acv.IsHub= IsHub;
            acv.ChildDelay = ChildDelay;
            ParentIds.ForEach(x => acv.ParentIds.Add(x));
            return acv;
        }
    }
}
