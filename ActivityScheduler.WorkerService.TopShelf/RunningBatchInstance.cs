using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Data.Models.Settings;
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
        private int _successCout;
        private int _totalCount;
        private int _sleepDuration = 500;
        private SettingsData _settings;

        public RunningBatchInstance(string batchNumber, BatchManager batchManager, ActivityManager activityManager, SettingsManager settingsManager, Serilog.ILogger logger)
        {
            _batchManager=batchManager;
            _activityManager=activityManager;
            _logger =logger;
            _batchNumber = batchNumber;
            _settings = settingsManager.GetSettings();
            var batchTmp = batchManager.GetByNumberOrNull(batchNumber).Result;
            if(batchTmp != null ) 
            {
                _batch = batchTmp;
            }
            else
            { 
                throw new ArgumentException($"Error reading batch number {batchNumber} from database while trying to run it"); 
            }

            _logger.Information($"RunningBatchInstance: attempt to run batch {batchNumber}");
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
            try
            {
                //simply wait for timepoint

                bool canExit = false;
                do
                {
                    _logger.Information($"Waiting for timepoint {point}, left {(point - DateTime.Now)}");

                    canExit = DateTime.Now > point;

                    Thread.Sleep(1000);

                }
                while (!canExit);

            }
            catch (Exception ex) 
            { 
                _logger.Error($"ERROR message={ex.Message}, innerexception={ex.InnerException}");
            }
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
            _logger.Information($"RunningBatchInstance.Run Batch number={_batch.Number} RunMode={_batch.RunMode}");
            
            // Case 1: Simply run once and stop
            if (_batch.RunMode == BatchStartTypeEnum.Single)
            {
                var targetPoint = DefineFirstStartDateTimePoint();
                _logger.Information($"Starting batch {_batch.Number} in single run mode from timepoint: {targetPoint}");
                RunBatchOnce(targetPoint, _batch);
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

        private string? GetScriptPath(Activity activity)
        {
            if (!string.IsNullOrEmpty(activity.ScriptPath))
                return activity.ScriptPath;

            if(activity.ParentBatch!=null)
            {
                if (!string.IsNullOrEmpty(activity.ParentBatch.ScriptPath))
                    return activity.ParentBatch.ScriptPath;
            }

            return _settings.DefaultScriptPath;


        }

        private void RunBatchOnce(DateTime startDateTime, Batch batch)
        {

            //wait for starttime moment
            WaitForTimePoint(startDateTime);

            var activities = _activityManager.GetAll(batch.Id).Result.ToList().OrderBy(x=>x.ActivityId).ToList();
            
            bool canExit=false;
            bool batchFailed = false;
            
            _logger.Information($"[RunningBatchInstance.RunBatchOnce] {batch.Number}, activities.Count={activities.Count} ");

            do 
            {
                //go through activities and run them if needed
                foreach (var activity in activities)
                {
                    _logger.Information($"Enumerating activities, activity.ActivityId={activity.ActivityId} activity.IsActive={activity.IsActive} activity.Status={activity.Status} activity.IsTimeDriven={activity.IsTimeDriven} ");
                    if (activity.IsActive)
                    {
                        if (activity.Status == ActivityStatusEnum.Idle)
                        {
                            if (activity.IsTimeDriven)
                            {
                                activity.Status = ActivityStatusEnum.Waiting;
                                _logger.Information($"[RunningBatchInstance.RunBatchOnce] -- moving activity {activity.ActivityId} to status Waiting");
                            }
                            else if (activity.IsParentDriven)
                            {
                                activity.Status = ActivityStatusEnum.WaitingForParent;
                                _logger.Information($"[RunningBatchInstance.RunBatchOnce] -- moving activity {activity.ActivityId} to status WaitingForParent");
                            }
                        }

                        if (activity.IsTimeDriven && activity.Status == ActivityStatusEnum.Waiting)
                        {
                                bool isActivityRunningTimeNow = DateTime.Now > startDateTime + activity.StartTime;
                                _logger.Information($"[RunningBatchInstance.RunBatchOnce]: looking if have to run time-driven activity, {DateTime.Now - (startDateTime + activity.StartTime)} left to start, running time now: {isActivityRunningTimeNow}");

                                if (isActivityRunningTimeNow)
                                {
                                    _logger.Information($"[RunningBatchInstance.RunBatchOnce] -- running time driven activity {activity.ActivityId}");
                                    var rez = activity.Run(_logger, GetScriptPath(activity));
                                    ActivityRunningData.Add(new ActivityRunningInfo(activity, rez.ActivityStartTask, rez.Process));
                                    activity.Status = ActivityStatusEnum.Running;
                                }
                        }
                            
                        if (activity.IsParentDriven)
                        {
                            //if parent activities are completed, launch this activity
                            if (AreActivityParentsCompleted(activity))
                            {
                                _logger.Information($"[RunningBatchInstance.RunBatchOnce] -- running parent driven activity {activity.ActivityId}");
                                var rez = activity.Run(_logger, GetScriptPath(activity));
                                ActivityRunningData.Add(new ActivityRunningInfo(activity, rez.ActivityStartTask, rez.Process));
                                activity.Status = ActivityStatusEnum.Running;
                            }
                        }

                        if (_stopMarker)
                        {
                            break;
                        }
                    }
                }
                _logger.Information($"[RunningBatchInstance.RunBatchOnce]");
                _logger.Information($"[RunningBatchInstance.RunBatchOnce] ActivityRunningData dump");
                //ActivityRunningData.ForEach(x => _logger.Information($"{x.Activity.ActivityId}--{x.ActivityRunningTask.Status}"));
                


                //go through launched activities and catch result
                _totalCount = activities.Count;
                _successCout = 0;
                _logger.Information($"[RunningBatchInstance.RunBatchOnce]  Before -- Success/total {_successCout}/{_totalCount}");
                canExit=false;
                foreach (var activityRunInfo in ActivityRunningData)
                {
                        _logger.Information($"[RunningBatchInstance.RunBatchOnce]  Before--ActivityId={activityRunInfo.Activity.ActivityId}--status={activityRunInfo.ActivityRunningTask.Status}--result={activityRunInfo.ActivityRunningTask.Result}");

                        //if at least one uncompleted, cant exit
                        if (activityRunInfo.ActivityRunningTask.IsCompleted)
                        {
                            _successCout += 1;
                            activityRunInfo.Activity.Status = ActivityStatusEnum.Completed;
                        }

                        //if at least one activity fails, batch fails and stops
                        if (activityRunInfo.ActivityRunningTask.Result == 0)
                        {
                            batchFailed = true;
                            activityRunInfo.Activity.Status = ActivityStatusEnum.Failed;
                            _batch.Status = BatchStatusEnum.Failed;
                            break;
                        }
                    _logger.Information($"[RunningBatchInstance.RunBatchOnce] After--ActivityId={activityRunInfo.Activity.ActivityId}--status={activityRunInfo.ActivityRunningTask.Status}--result={activityRunInfo.ActivityRunningTask.Result}");
                }
                _logger.Information($"[RunningBatchInstance.RunBatchOnce] ");
                _logger.Information($"[RunningBatchInstance.RunBatchOnce] After-- Success/total {_successCout}/{_totalCount}");
                canExit = _successCout == _totalCount;
                if (batchFailed) 
                { 
                    canExit = true; 
                }

                if (canExit==true && !batchFailed && !_stopMarker)
                {
                    _batch.Status = BatchStatusEnum.CompletedSuccessfully;
                }

                if(_stopMarker)
                {
                    _batch.Status = BatchStatusEnum.StoppedByUser;
                }

                Thread.Sleep(_sleepDuration);
            }
            while (!canExit);
            
            _logger.Information($"[RunningBatchInstance.RunBatchOnce] Batch Stopped by reason: {_batch.Status}");
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
