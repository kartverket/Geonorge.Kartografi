﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class SldRule
    {
        public string Name { get; set; }
        public string WellKnownName { get; set; }
        public string Fill { get; set; }
        public string Stroke { get; set; }
    }
}