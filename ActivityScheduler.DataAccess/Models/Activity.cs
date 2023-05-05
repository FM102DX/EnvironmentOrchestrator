﻿using ActivityScheduler.Data.DataAccess;
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
        public Guid BatchId { get; set; }
        public string Name { get; set; }


        public int ActivityId { get; set; } // number like 10, 20, 30 ... 450, 460, ets
        
        public TimeSpan StartTime { get; set; }

        public String TransactionId { get; set; } //number of transaction in this script / foreign script

        public bool IsDomestic { get; set; } //wether domestic transaction should be called
        public bool IsHub { get; set; } //wether this transaction is a hub (hubs do nothing but become successful to start child activities)

        public TimeSpan ChildDelay { get; set; } // starttime that's passed to another script as parameter, is for ChildDelay more than fact starttime

        public bool AlwaysSuccess { get; set; }
        public string? ParentActivities { get; set; }

        public List<int> GetParentActionIds()
        {
            if (string.IsNullOrEmpty(ParentActivities)) { return new List<int>(); }
            
            var x = ParentActivities.Split(',').ToList();

            List<int> y;

            try
            {
                y = x.Select(i => Convert.ToInt32(i)).ToList();
            }
            
            catch { return new List<int>(); }
            
            return y;
        }

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
            rez.ParentActivities = ParentActivities;
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
            acv.ParentActivities = ParentActivities;
            return acv;
        }
    }
}
