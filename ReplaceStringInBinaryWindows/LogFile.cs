using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceString
{
    public class LogFile
    {
        private string fileName;
        private static Object lockOBJ = new Object();
        public LogFile()
        {

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fileName = string.Format(@"{0}\logfile.log", path);
        }
        public LogFile(string fileName)
        {
            this.fileName = fileName;
        }

        public void WriteToLog(string strMessage)
        {
            lock (lockOBJ)
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(fileName, FileMode.Append)))
                {
                    writer.WriteLine("{0}", strMessage);
                }
            }
        }
    }
}
