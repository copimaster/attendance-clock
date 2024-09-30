﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTACheckClock.Models
{
    /// <summary>
    /// Clase para encapsular la información de las huellas dactilares (FMD's).
    /// </summary>
    class FMDItem
    {
        public int idx { get; set; }
        public int offid { get; set; }
        public int empid { get; set; }
        public int fingid { get; set; }
        public string? empnum { get; set; }
        public string? empnom { get; set; }
        public string? emppass { get; set; }
        public byte[]? fmd { get; set; }
    }
}
