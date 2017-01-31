using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.Services
{
    public interface ICartographyService
    {
        List<Dataset> GetDatasets();
        List<CartographyFile> GetCartography(string uuid = null);
        CartographyFile GetCartography(Guid? SystemId);
        void AddCartography(CartographyFile cartographyFile);
        void AddCartographyVersion(CartographyFile cartographyFile);
        void UpdateCartography(CartographyFile cartographyFile);
        void RemoveCartography(CartographyFile cartographyFile);
        VersionsItem Versions(Guid? SystemId);
    }
}