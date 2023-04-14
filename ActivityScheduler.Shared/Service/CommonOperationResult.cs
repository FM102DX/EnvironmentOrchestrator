using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared
{
    public class CommonOperationResult
    {
        public bool _success;
        public string _msg;
        public object _returningValue;

        public static CommonOperationResult GetInstance(bool success, string msg, object returningValue = null)
        {
            CommonOperationResult c = new CommonOperationResult()
            {
                _msg = msg,
                _returningValue = returningValue,
                _success = success
            };
            return c;
        }

        public static CommonOperationResult SayFail(string _msg = "") { return GetInstance(false, _msg, null); }
        public static CommonOperationResult SayOk(string _msg = "") { return GetInstance(true, _msg, null); }
        public static CommonOperationResult SayItsNull(string _msg = "") { return GetInstance(true, _msg, null); }

        public string ShrotString() => $"Success: {_success} message: {_msg}";
    }
}
