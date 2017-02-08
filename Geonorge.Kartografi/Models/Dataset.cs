using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class Dataset
    {
        public string DatasetUuid { get; set; }
        [Display(Name = "Tittel")]
        public string DatasetName { get; set; }
        [Display(Name = "Tema")]
        public string Theme { get; set; }
        [Display(Name = "Eier")]
        public string OwnerDataset { get; set; }
    }
}