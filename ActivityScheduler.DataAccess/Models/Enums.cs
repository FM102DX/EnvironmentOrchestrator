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
        Periodic=1,
        PeriodicDaily=2
    }

    public enum ActivityParentRuleEnum
    {
        And = 0, //activity starts when all parent are successful
        Or = 1 //activity starts when one of parent is successful
    }

    public enum ActivityStatusEnum
    {
        Idle = 10,
        WaitingForParent=20,
        Waiting=30,
        Running=40,
        Completed = 50,
        Failed=60,
        WaitingForRetry=70
    }
    public enum BatchStartPointTypeEnum
    {
        StartFromNow=1,
        StartTodayFromSpecifiedTime=2,
        StartFromSpecifiedDateAndTime=3
    }



    public enum BatchStatusEnum
    {

    }

}
