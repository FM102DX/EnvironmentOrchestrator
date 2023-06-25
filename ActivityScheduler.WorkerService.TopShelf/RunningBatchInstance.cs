using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Activity = ActivityScheduler.Data.Models.Activity;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public  class RunningBatchInstance
    {
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private ActivityManager _activityManager;
        private string _batchNumber;
        private bool _stopMarker;
        private readonly System.Timers.Timer _timer;
        private Batch _batch;
        private List<ActivityRunningInfo> ActivityRunningData = new List<ActivityRunningInfo>();

        public RunningBatchInstance(string batchNumber, BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _activityManager=activityManager;
            _logger =logger;
            _batchNumber = batchNumber;
            var batchTmp = batchManager.GetByNumberOrNull(batchNumber).Result;
            if(batchTmp != null ) 
            {
                _batch = batchTmp;
            }
            else
            { 
                throw new ArgumentException($"Error reading batch number {batchNumber} from database while trying to run it"); 
            }


            //timer
            //_timer = new System.Timers.Timer(500) { AutoReset = true };
            //_timer.Elapsed += _timer_Elapsed;
            //_timer.Start();
        }

        
        private bool AreActivityParentsCompleted(Activity activity)
        {
            //get all parent activity numbers
            var parentIds = activity.GetParentActionIds();

            bool rez=true;

            foreach (var parentId in parentIds) 
            {
                var parentActivityRunningInfo = ActivityRunningData.Where(x=>x.Activity.ActivityId== parentId).FirstOrDefault();
                
                if (parentActivityRunningInfo==null)
                { 
                    throw new Exception("Parent activity exception"); 
                }
                 
                if (!parentActivityRunningInfo.ActivityRunningTask.IsCompleted || parentActivityRunningInfo.ActivityRunningTask.Result == 0)
                {
                    rez = false;
                }
            }
            return rez;
            
        }
        
        private void WaitForTimePoint(DateTime point)
        {
            //simply wait for timepoint
            bool canExit=false;
            do
            {
                canExit = DateTime.Now > point;

                Thread.Sleep(1000);

            }
            while (!canExit);
        }

        private void WaitForAnActiveDay() 
        {
            _batch.Status = BatchStatusEnum.WaitingForAnActiveDay;
            bool canExitActiveDayWaitingCycle = false;
            do
            {
                Thread.Sleep(1000);
                //todo here put possibility of exit when stopped
                canExitActiveDayWaitingCycle = _batch.IsDateAnActiveDay(DateTime.Now);
            }
            while(!canExitActiveDayWaitingCycle);
        }
        private DateTime GetBeginOfThisDay(DateTime dtNow) 
        {
            DateTime beginOfThisDay;
            var parseRez = DateTime.TryParse($"{dtNow.Day}.{dtNow.Month}.{dtNow.Year} 00:00:00", CultureInfo.CurrentCulture, DateTimeStyles.None, out beginOfThisDay);

            if (!parseRez)
            {
                throw new ArgumentException($"Error parsing datetime while running batch");
            }
            return beginOfThisDay;
        }
        private DateTime GetEndOfThisDay(DateTime dtNow)
        {
            DateTime beginOfThisDay;
            var parseRez = DateTime.TryParse($"{dtNow.Day}.{dtNow.Month}.{dtNow.Year} 23:59:59", CultureInfo.CurrentCulture, DateTimeStyles.None, out beginOfThisDay);

            if (!parseRez)
            {
                throw new ArgumentException($"Error parsing datetime while running batch");
            }
            return beginOfThisDay;
        }

        private DateTime DefineFirstStartDateTimePoint()
        {
            DateTime actualStartDateTime = DateTime.Now;
            //need to define startdatetime
            switch (_batch.StartPointType)
            {
                case BatchStartPointTypeEnum.StartFromNow:
                    actualStartDateTime = DateTime.Now;
                    break;

                case BatchStartPointTypeEnum.StartTodayFromSpecifiedTime:
                    DateTime dtNow = DateTime.Now;
                    DateTime beginOfThisDay = GetBeginOfThisDay(dtNow);
                    actualStartDateTime = beginOfThisDay.AddHours(_batch.StartTimeInADay.Hours);
                    actualStartDateTime = actualStartDateTime.AddMinutes(_batch.StartTimeInADay.Minutes);
                    actualStartDateTime = actualStartDateTime.AddSeconds(_batch.StartTimeInADay.Seconds);
                    break;

                case BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime:
                    actualStartDateTime = _batch.StartDateTime;
                    break;
            }
            return actualStartDateTime;
        }

        public CommonOperationResult Run() 
        {
            
            // Case 1: Simply run once and stop
            if (_batch.RunMode == BatchStartTypeEnum.Single)
            {
                RunBatchOnce(DefineFirstStartDateTimePoint(), _batch);
            }

            if (_batch.RunMode == BatchStartTypeEnum.Periodic)
            {
                // Case 2: Simply run once and stop, exactily as Case 1
                if (!_batch.HasInterval && !_batch.HasDuration)
                {
                    RunBatchOnce(DefineFirstStartDateTimePoint(), _batch);
                }
                
                // Case 3: Retry infinitely
                if (_batch.HasInterval && !_batch.HasDuration)
                {
                    var initialDt = DefineFirstStartDateTimePoint();
                    DateTime workingDtInitial;
                    DateTime workingDtCalculated=DateTime.Now;

                    workingDtInitial = DefineFirstStartDateTimePoint();
                    RunBatchOnce(workingDtInitial, _batch);
                    
                    do
                    {
                        //define next datetime
                        int n = 0;
                        do 
                        {
                            n++;
                            for(int i=1; i<=n;i++)
                            {
                                workingDtCalculated = workingDtInitial + _batch.Interval;
                            }

                        } while (workingDtCalculated < DateTime.Now);

                        RunBatchOnce(workingDtCalculated, _batch);
                    }
                    while (true);
                }

                // Case 4: Retry infinitely inside duration
                if (_batch.HasInterval && _batch.HasDuration)
                {
                    var initialDt = DefineFirstStartDateTimePoint();
                    DateTime workingDtInitial;
                    DateTime workingDtCalculated = DateTime.Now;
                    bool canExit=false;
                    do
                    {
                        workingDtInitial = DefineFirstStartDateTimePoint();
                        RunBatchOnce(workingDtInitial, _batch);

                        //define next datetime
                        int n = 0;
                        do
                        {
                            n++;
                            for (int i = 1; i <= n; i++)
                            {
                                workingDtCalculated = workingDtInitial + _batch.Interval;
                            }

                        } while (workingDtCalculated < DateTime.Now);

                        canExit = workingDtCalculated > workingDtInitial + _batch.Duration;

                        if (!canExit)
                            RunBatchOnce(workingDtCalculated, _batch);
                    }
                    while (!canExit);
                }
            }

            if (_batch.RunMode == BatchStartTypeEnum.PeriodicDaily)
            {
                // Case 5: Simply run once and stop, exactily as Case 1, but each active day of week
                if (!_batch.HasInterval && !_batch.HasDuration)
                {
                    bool doneForToday = false;
                    do
                    {
                        var dtStart = DefineFirstStartDateTimePoint();
                        if (_batch.IsDateAnActiveDay(dtStart))
                        {
                            if(!doneForToday)
                            {
                                var dtEndOfDay = GetEndOfThisDay(dtStart);
                                var targetTimeout = dtEndOfDay - dtStart;

                                //batch must stop at the end of day to be ran next day
                                if (_batch.HasTimeout || (_batch.Timeout > targetTimeout)) _batch.Timeout = targetTimeout;
                                RunBatchOnce(dtStart, _batch);
                                doneForToday = true;
                            }
                        }
                        else
                        {
                            //wait for an active day
                            WaitForAnActiveDay();
                            doneForToday = false;
                        }
                        Thread.Sleep(1000);
                    } while (true);
                }


                // Case 6: Retries infinitely, but indide each day, e.g. each day stops and begins from StartTime
                if (_batch.HasInterval && !_batch.HasDuration)
                {
                    bool doneForToday = false;
                    do
                    {
                        var dtStart = DefineFirstStartDateTimePoint();
                        if (_batch.IsDateAnActiveDay(dtStart))
                        {
                                var dtEndOfDay = GetEndOfThisDay(dtStart);
                                var targetTimeout = dtEndOfDay - dtStart;
                                if (_batch.HasTimeout || (_batch.Timeout > targetTimeout)) _batch.Timeout = targetTimeout;

                                DateTime workingDtCalculated = DateTime.Now;
                                RunBatchOnce(dtStart, _batch);
                                bool canExit = false;
                                do
                                {
                                    //define next datetime
                                    int n = 0;
                                    do
                                    {
                                        n++;
                                        for (int i = 1; i <= n; i++)
                                        {
                                            workingDtCalculated = dtStart + _batch.Interval;
                                        }

                                    } while (workingDtCalculated < DateTime.Now);

                                    canExit = workingDtCalculated > dtEndOfDay;
                                    if(!canExit)
                                    {
                                        RunBatchOnce(workingDtCalculated, _batch);
                                    }
                                }
                                while (!canExit);
                        }
                        else
                        {
                            //wait for an active day
                            WaitForAnActiveDay();
                            doneForToday = false;
                        }
                        Thread.Sleep(1000);
                    } while (true);
                }

                //Case 7: Each day begins from StartTime, retries infinitely inside duration, stops when execution goes outside duration. Duration is limited by [from starttime to 23:59:59]
                if (_batch.HasInterval && _batch.HasDuration)
                {
                    // TDOD: will do in future
                }
            }

            return CommonOperationResult.SayOk();
        }

        public CommonOperationResult Stop()
        {
            _stopMarker=true;
            _logger.Information($"010024 RunningBatchInstance: Stop command");
            return CommonOperationResult.SayOk();
        }

        private CommonOperationResult RunBatch0()
        {
            return CommonOperationResult.SayOk();
        }

        private void RunBatchOnce(DateTime startDateTime, Batch batch)
        {

            //wait for starttime moment
            WaitForTimePoint(startDateTime);

            var activities = _activityManager.GetAll(batch.Id).Result.ToList().OrderBy(x=>x.ActivityId).ToList();
            
            bool canExit=false;
            bool batchFailed = false;

            do 
            {
                //go through activities and run them if needed
                foreach (var activity in activities)
                {
                    if (activity.IsActive)
                    {
                        if (activity.Status == ActivityStatusEnum.Idle)
                        {
                            if (!activity.IsTimeDriven)
                            {
                                activity.Status = ActivityStatusEnum.Waiting;
                            }
                            else if (!activity.IsParentDriven)
                            {
                                activity.Status = ActivityStatusEnum.WaitingForParent;
                            }
                        }

                        if (activity.IsTimeDriven)
                        {
                            bool isActivityRunningTimeNow = DateTime.Now > startDateTime + activity.StartTime;

                            if (isActivityRunningTimeNow && activity.Status == ActivityStatusEnum.Waiting)
                            {
                                var rez= activity.Run();
                                ActivityRunningData.Add(new ActivityRunningInfo(activity, rez.ActivityStartTask, rez.Process));
                            }
                        }
                            
                        if (activity.IsParentDriven)
                        {
                            //if parent activities are completed, launch this activity
                            if (AreActivityParentsCompleted(activity))
                            {
                                var rez = activity.Run();
                                ActivityRunningData.Add(new ActivityRunningInfo(activity, rez.ActivityStartTask, rez.Process));
                            }
                        }

                    }
                }

                //go through launched activities and catch result
                canExit = true;
                foreach (var activityRunInfo in ActivityRunningData)
                {
                    
                    //if at least one uncompleted, cant exit
                    if(!activityRunInfo.ActivityRunningTask.IsCompleted)
                    {
                        canExit = false;
                    }
                    
                    //if at least one activity fails, batch fails and stops
                    if (activityRunInfo.ActivityRunningTask.Result==0)
                    {
                        batchFailed = true;
                        _batch.Status = BatchStatusEnum.Failed;
                        canExit = true; 
                        break;
                    }
                }
                
                if (canExit==true && !batchFailed && !_stopMarker)
                {
                    _batch.Status = BatchStatusEnum.CompletedSuccessfully;
                }

                if(_stopMarker)
                {
                    _batch.Status = BatchStatusEnum.StoppedByUser;
                }



                Thread.Sleep(500);
            }
            while (!canExit);
        }

        public class ActivityRunningInfo
        {
            public Activity Activity { get; set; }
            public Task<int> ActivityRunningTask { get; set; }

            public  System.Diagnostics.Process Process { get; set; }

            public ActivityRunningInfo(Activity activity, Task<int> activityRunningTask, System.Diagnostics.Process process) 
            {
                Activity=activity;
                ActivityRunningTask=activityRunningTask;
                Process=process;
            }    
        }
    }
}
