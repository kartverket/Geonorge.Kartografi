using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.Services
{
    public interface ICartographyService
    {
        List<CartographyFile> GetCartography();
        CartographyFile GetCartography(int? id);
        void AddCartography(CartographyFile cartographyFile);
        void UpdateCartography(CartographyFile cartographyFile);
        void RemoveCartography(CartographyFile cartographyFile);
    }
}