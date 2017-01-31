using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class Compatibility
    {
        public string Id { get; set; }
        public string Key { get; set; }

        public override string ToString()
        {
            return Key;
        }
    }
}