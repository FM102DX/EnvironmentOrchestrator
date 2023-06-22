using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.WorkerService.TopShelf
{
    public  class RunningBatchInstance
    {
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private ActivityManager _activityManager;
        private string _batchNumber;
        bool _stopMarker;
        private readonly System.Timers.Timer _timer;
        private Batch _batch;

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

        public DateTime DefineFirstStartDateTimePoint() 
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
                    DateTime beginOfThisDay;

                    var parseRez = DateTime.TryParse($"{dtNow.Day}.{dtNow.Month}.{dtNow.Year} 00:00:00", CultureInfo.CurrentCulture, DateTimeStyles.None, out beginOfThisDay);

                    if (!parseRez)
                    {
                        throw new ArgumentException($"Error parsing datetime while running batch");
                    }
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

                    do
                    {
                        workingDtInitial = DefineFirstStartDateTimePoint();
                        RunBatchOnce(workingDtInitial, _batch);

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

            
            
            _stopMarker = false;
            
            Stopwatch stopwatch = new Stopwatch();  
            
            stopwatch.Start();

            do
            {
                Thread.Sleep(500);
                _stopMarker = stopwatch.Elapsed.TotalSeconds>10;
                _logger.Information($"010025 Stopwatch.Elapsed={stopwatch.Elapsed.TotalSeconds} value={stopwatch.Elapsed.TotalSeconds > 10}");
            }
            while (!_stopMarker);

            _logger.Information($"010024 RunningBatchInstance: Exiting run cycle");
            
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
            var activities = _activityManager.GetAll(batch.Id).Result.ToList().OrderBy(x=>x.ActivityId).ToList();
            
            bool canExit=false;

            do 
            {
                foreach (var activity in activities)
                {
                    if (activity.IsActive)
                    {
                        if (activity.Status == ActivityStatusEnum.Idle)
                        {
                            activity.Status = ActivityStatusEnum.Waiting;
                        }

                        bool isRunningTimeNow = DateTime.Now > startDateTime + activity.StartTime;

                        //get script path
                        string? scriptPath = batch.DefaultScriptPath;
                        if (!string.IsNullOrEmpty(activity.ScriptPath)) { scriptPath = activity.ScriptPath; }
                        if(string.IsNullOrEmpty(scriptPath))
                        {
                            throw new Exception($"No ScriptPath value specified for batch or activity, batch number={batch.Number}");
                        }

                        if (isRunningTimeNow  && (activity.Status == ActivityStatusEnum.Waiting))
                        {
                            //launch task
                            //run powershell with params
                            
                            string appName = "powershell.exe";
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                            startInfo.CreateNoWindow = false;
                            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                            startInfo.FileName = appName;
                            startInfo.Arguments = $"-file {scriptPath} -transactionId {activity.TransactionId}";
                            process.StartInfo = startInfo;
                            process.Start();
                        }

                    }
                }
                Thread.Sleep(500);
            }
            while (!canExit);
        }
    }
}
