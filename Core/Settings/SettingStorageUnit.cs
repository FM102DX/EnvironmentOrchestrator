using ActivityScheduler.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Core.Settings
{
    public class SettingStorageUnit : BaseEntity
    {
        public string Name { get; set; }
        
        public string Value { get; set; }

    }
}
