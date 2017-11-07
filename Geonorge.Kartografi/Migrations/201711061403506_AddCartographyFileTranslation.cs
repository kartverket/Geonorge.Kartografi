namespace Geonorge.Kartografi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCartographyFileTranslation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartographyFileTranslations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CartographyFileId = c.Guid(nullable: false),
                        Owner = c.String(),
                        OwnerDataset = c.String(),
                        DatasetName = c.String(),
                        ServiceName = c.String(),
                        Use = c.String(),
                        Properties = c.String(),
                        Theme = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        CultureName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CartographyFiles", t => t.CartographyFileId, cascadeDelete: true)
                .Index(t => t.CartographyFileId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartographyFileTranslations", "CartographyFileId", "dbo.CartographyFiles");
            DropIndex("dbo.CartographyFileTranslations", new[] { "CartographyFileId" });
            DropTable("dbo.CartographyFileTranslations");
        }
    }
}
