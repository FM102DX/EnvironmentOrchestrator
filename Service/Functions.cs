using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityScheduler.Service
{
    public static class Functions
    {
        public static string GetNextFreeFileName(string path, string nameBase, string extention)
        {
            String s;
            int i = 0;
            do
            {
                s = Path.Combine(path, $"{nameBase}_{i}.{extention}");
                if (!File.Exists(s)) { return s; }
                i++;
                if (i > 10000) 
                { 
                    throw new Exception($"Too much log files in directory {path}, please consider clearing");
                }
            }
            while (true);
        }

        public static void LeaveLastNFilesOrFoldersInDirectory(string path, int leaveNumber)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            var files = di.GetFiles();
            files.ToList().OrderByDescending(x => x.CreationTime).Skip(leaveNumber).ToList().ForEach(file => file.Delete());
        }
    }
}
