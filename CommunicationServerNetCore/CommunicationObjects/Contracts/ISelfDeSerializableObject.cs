using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Shared.Pipes
{
    public interface ISelfDeSerializableObject<T>
    {
        public T? GetObject(string message);
    }
}
