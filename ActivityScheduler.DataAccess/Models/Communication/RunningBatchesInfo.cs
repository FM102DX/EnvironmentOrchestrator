using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models.Communication
{
    public class RunningBatchesInfo
    {
        public List<Batch> Batches { get; set; }=new List<Batch>();
        
    }
}
