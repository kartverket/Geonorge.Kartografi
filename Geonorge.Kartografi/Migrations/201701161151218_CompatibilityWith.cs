namespace Geonorge.Kartografi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompatibilityWith : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Compatibilities",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Key = c.String(),
                        CartographyFile_SystemId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CartographyFiles", t => t.CartographyFile_SystemId, cascadeDelete: true)
                .Index(t => t.CartographyFile_SystemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Compatibilities", "CartographyFile_SystemId", "dbo.CartographyFiles");
            DropIndex("dbo.Compatibilities", new[] { "CartographyFile_SystemId" });
            DropTable("dbo.Compatibilities");
        }
    }
}
