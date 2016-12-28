namespace Geonorge.Kartografi.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Geonorge.Kartografi.Models.CartographyDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Geonorge.Kartografi.Models.CartographyDbContext context)
        {
            //  This method will be called after migrating to the latest version.

        }
    }
}
