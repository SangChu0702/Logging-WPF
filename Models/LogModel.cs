using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UARTLogging.Models
{
    public class LogModel : BindableBase
    {
        public string LogData { get; set; }
        public DateTime LogTimeStamp { get; set; }
        public int LogIndex { get; set; }
        public LogModel() { }
    }
}
