using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models.Api
{
    public class CartographyResult
    {
        public int Count { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }

        public List<Models.Api.Cartography> Files { get; set; }
    }
}