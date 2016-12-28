using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;
using System.Data.Entity;

namespace Geonorge.Kartografi.Services
{
    public class CartographyService : ICartographyService
    {
        private readonly CartographyDbContext _dbContext;

        public CartographyService(CartographyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<CartographyFile> GetCartography()
        {
            return _dbContext.CartographyFiles.ToList();
        }

        public CartographyFile GetCartography(int? id)
        {
            return _dbContext.CartographyFiles.Find(id);
        }

        public void AddCartography(CartographyFile cartographyFile)
        {
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();
        }

        public void UpdateCartography(CartographyFile cartographyFile)
        {
            _dbContext.Entry(cartographyFile).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void RemoveCartography(CartographyFile cartographyFile)
        {
            _dbContext.CartographyFiles.Remove(cartographyFile);
            _dbContext.SaveChanges();
        }
    }
}