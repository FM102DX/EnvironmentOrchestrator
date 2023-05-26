using ActivityScheduler.Data.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Data.Models
{
    public  class BatchListBoxViewModel
    {
        public Guid Id { get; set; }

        public bool IsGroup { get; set; }
        public string BatchNumber { get; set; }
        public string Text { get; set; }
        public string ImageSource { get; set; }

        public Batch? BatchObject { get; set; }

    }
}
