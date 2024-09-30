using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTACheckClock.Models
{
    class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string? Message { get; set; }
    }
}
