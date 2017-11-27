namespace Geonorge.Kartografi.Migrations
{
    using Geonorge.Kartografi.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Geonorge.Kartografi.Models.CartographyDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        internal class CartographyFileTranslationConfiguration : EntityTypeConfiguration<CartographyFile>
        {

            public CartographyFileTranslationConfiguration()
            {
                HasMany(x => x.Translations).WithRequired().HasForeignKey(x => x.CartographyFileId);
            }

        }

        protected override void Seed(Geonorge.Kartografi.Models.CartographyDbContext context)
        {
            //  This method will be called after migrating to the latest version.

        }
    }
}
