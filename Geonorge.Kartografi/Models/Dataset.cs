using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class Dataset
    {
        public string DatasetUuid { get; set; }
        public string DatasetName { get; set; }
        public string Theme { get; set; }
        public string OwnerOrganization { get; set; }
    }
}