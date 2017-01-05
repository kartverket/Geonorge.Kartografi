using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class Version
    {
        [Key]
        public Guid SystemId { get; set; }
        public Guid CurrentVersion { get; set; }
        public int LastVersionNumber { get; set; }
    }
}