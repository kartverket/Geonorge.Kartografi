using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.Models
{
    public class KartografiDbContext : DbContext
    {
        public KartografiDbContext() : base("KartografiDbContext")
        {

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<KartografiDbContext, Geonorge.Kartografi.Migrations.Configuration>("KartografiDbContext"));
        }

        public DbSet<CartographyFile> CartographyFiles { get; set; }
        public DbSet<Symbol> Symbols { get; set; }

        public System.Data.Entity.DbSet<Geonorge.Kartografi.Models.SymbolFile> SymbolFiles { get; set; }
    }
}