using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public  class ActivityGridViewModel
    {
        public Guid Id { get; set; }

        public int ActivityId { get; set; }

        public string? ParentActivities { get; set; }
        
        public string Name { get; set; }

        public TimeSpan StartTime { get; set; }

        public String? TransactionId { get; set; } 

        public bool IsDomestic { get; set; } 
        
        public bool IsHub { get; set; }

        public TimeSpan ChildDelay { get; set; }

        public bool AlwaysSuccess { get; set; }

    }
}
