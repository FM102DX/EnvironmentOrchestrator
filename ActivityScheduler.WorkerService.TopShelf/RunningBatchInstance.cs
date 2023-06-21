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

        public void Run01() 
        {

        }

        public CommonOperationResult Run() 
        {

            if (_batch.RunMode == BatchStartTypeEnum.Single)
            {
                // Case 1: no interval, no duration
                // Just run once and stop

                if (!_batch.HasInterval && !_batch.HasDuration)
                {
                    DateTime dtNow = DateTime.Now;

                    DateTime beginOfThisDay;

                    var parseRez = DateTime.TryParse($"{dtNow.Day}.{dtNow.Month}.{dtNow.Year} 00:00:00", CultureInfo.CurrentCulture, DateTimeStyles.None, out beginOfThisDay);

                    if (!parseRez)
                    {
                        throw new ArgumentException($"Error parsing datetime while running batch");
                    }


                    DateTime actualStartDateTime = DateTime.Now;

                    if (_batch.StartPointType == BatchStartPointTypeEnum.StartFromNow)
                    {
                        actualStartDateTime = DateTime.Now;
                    }
                    else if (_batch.StartPointType == BatchStartPointTypeEnum.StartTodayFromSpecifiedTime)
                    {
                        actualStartDateTime = beginOfThisDay.AddHours(_batch.StartTimeInADay.Hours);
                        actualStartDateTime = actualStartDateTime.AddMinutes(_batch.StartTimeInADay.Minutes);
                        actualStartDateTime = actualStartDateTime.AddSeconds(_batch.StartTimeInADay.Seconds);
                    }
                    else if (_batch.StartPointType == BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime)
                    {
                        actualStartDateTime = _batch.StartDateTime;
                    }
                    RunBatchOnce(actualStartDateTime, _batch);
                }


                // Case 2: has interval, no duration
                // Retry forever

                if (_batch.HasInterval && !_batch.HasDuration)
                {

                    // 1) run according starttime
                    // 2) when finished, calculate next datetime
                    // 3) run form this time -> p.1

                    do
                    {



                    }
                    while (true);

                    DateTime dtNow = DateTime.Now;
                    DateTime beginOfThisDay;

                    var parseRez = DateTime.TryParse($"{dtNow.Day}.{dtNow.Month}.{dtNow.Year} 00:00:00", CultureInfo.CurrentCulture, DateTimeStyles.None, out beginOfThisDay);
                    if (!parseRez)
                    {
                        throw new ArgumentException($"Error parsing datetime while running batch");
                    }


                    DateTime actualStartDateTime = DateTime.Now;

                    if (_batch.StartPointType == BatchStartPointTypeEnum.StartFromNow)
                    {
                        actualStartDateTime = DateTime.Now;
                    }
                    else if (_batch.StartPointType == BatchStartPointTypeEnum.StartTodayFromSpecifiedTime)
                    {
                        actualStartDateTime = beginOfThisDay.AddHours(_batch.StartTimeInADay.Hours);
                        actualStartDateTime = actualStartDateTime.AddMinutes(_batch.StartTimeInADay.Minutes);
                        actualStartDateTime = actualStartDateTime.AddSeconds(_batch.StartTimeInADay.Seconds);
                    }
                    else if (_batch.StartPointType == BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime)
                    {
                        actualStartDateTime = _batch.StartDateTime;
                    }
                    RunBatchOnce(actualStartDateTime, _batch);
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
