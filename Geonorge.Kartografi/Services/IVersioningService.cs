using Geonorge.Kartografi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Services
{
    public interface IVersioningService
    {
        VersionsItem Versions(Guid? SystemId);
        Guid GetVersioningId(CartographyFile cartographyFile, CartographyFile originalCartographyFile);
    }
}