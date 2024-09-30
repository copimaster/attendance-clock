using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTACheckClock.Models
{
    class Message : Office
    {
        public string? Text { get; set; }
        public string? Data { get; set; }
    }
}
