using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;

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
    }
}