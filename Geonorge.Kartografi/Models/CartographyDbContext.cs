using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.Models
{
    public class CartographyDbContext : DbContext
    {
        public CartographyDbContext() : base("KartografiDbContext")
        {

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<CartographyDbContext, Geonorge.Kartografi.Migrations.Configuration>("KartografiDbContext"));
        }

        public virtual DbSet<CartographyFile> CartographyFiles { get; set; }
        public DbSet<Symbol> Symbols { get; set; }

        public System.Data.Entity.DbSet<Geonorge.Kartografi.Models.SymbolFile> SymbolFiles { get; set; }
    }
}