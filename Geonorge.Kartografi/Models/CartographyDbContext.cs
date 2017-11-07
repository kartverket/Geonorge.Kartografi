using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Models.Translations;
using static Geonorge.Kartografi.Migrations.Configuration;

namespace Geonorge.Kartografi.Models
{
    public class CartographyDbContext : DbContext
    {
        public CartographyDbContext() : base("KartografiDbContext")
        {

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<CartographyDbContext, Geonorge.Kartografi.Migrations.Configuration>("KartografiDbContext"));
        }

        public virtual DbSet<CartographyFile> CartographyFiles { get; set; }
        public virtual DbSet<Version> Versions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new CartographyFileTranslationConfiguration());
        }
    }
}