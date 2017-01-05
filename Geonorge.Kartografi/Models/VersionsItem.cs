using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class VersionsItem
    {
        public virtual CartographyFile CurrentVersion { get; set; }
        public virtual List<CartographyFile> Historical { get; set; }
        public virtual List<CartographyFile> Suggestions { get; set; }

        public VersionsItem()
        {

        }

        public VersionsItem(VersionsItem otherResult)
        {
            CurrentVersion = otherResult.CurrentVersion;
            Historical = otherResult.Historical;
            Suggestions = otherResult.Suggestions;
        }
    }
}