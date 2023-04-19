using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public enum BatchStartTypeEnum
    {
        Single=0,
        Periodic=1
    }

    public enum ActivityParentRuleEnum
    {
        And = 0, //activity starts when all parent are successful
        Or = 1 //activity starts when one of parent is successful
    }
}
