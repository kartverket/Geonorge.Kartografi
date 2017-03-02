using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.Services
{
    public interface ICartographyService
    {
        List<Dataset> GetDatasets(string text = null);
        List<CartographyFile> GetCartography(string uuid = null);
        CartographyFile GetCartography(Guid? SystemId);
        void AddCartographyVersion(CartographyFile cartographyFile, HttpPostedFileBase uploadFile = null, HttpPostedFileBase uploadPreviewImage = null);
        void RemoveCartography(CartographyFile cartographyFile);
        VersionsItem Versions(Guid? SystemId);
        void AddCartography(CartographyFile cartographyFile, HttpPostedFileBase uploadFile = null, HttpPostedFileBase uploadPreviewImage = null);
        void UpdateCartography(CartographyFile originalFile, CartographyFile file = null, HttpPostedFileBase uploadPreviewImage = null);
    }
}