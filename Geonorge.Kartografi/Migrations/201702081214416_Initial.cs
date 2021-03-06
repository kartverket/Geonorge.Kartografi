namespace Geonorge.Kartografi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartographyFiles",
                c => new
                    {
                        SystemId = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Description = c.String(),
                        Owner = c.String(),
                        OwnerDataset = c.String(),
                        LastEditedBy = c.String(),
                        FileName = c.String(),
                        Format = c.String(),
                        Use = c.String(),
                        DatasetUuid = c.String(),
                        DatasetName = c.String(),
                        ServiceUuid = c.String(),
                        ServiceName = c.String(),
                        PreviewImage = c.String(),
                        VersionId = c.Int(nullable: false),
                        versioningId = c.Guid(nullable: false),
                        DateChanged = c.DateTime(nullable: false),
                        Status = c.String(),
                        DateAccepted = c.DateTime(),
                        AcceptedComment = c.String(),
                        OfficialStatus = c.Boolean(nullable: false),
                        Properties = c.String(),
                        Theme = c.String(),
                    })
                .PrimaryKey(t => t.SystemId)
                .ForeignKey("dbo.Versions", t => t.versioningId, cascadeDelete: true)
                .Index(t => t.versioningId);
            
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
            
            CreateTable(
                "dbo.Versions",
                c => new
                    {
                        SystemId = c.Guid(nullable: false),
                        CurrentVersion = c.Guid(nullable: false),
                        LastVersionNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SystemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartographyFiles", "versioningId", "dbo.Versions");
            DropForeignKey("dbo.Compatibilities", "CartographyFile_SystemId", "dbo.CartographyFiles");
            DropIndex("dbo.Compatibilities", new[] { "CartographyFile_SystemId" });
            DropIndex("dbo.CartographyFiles", new[] { "versioningId" });
            DropTable("dbo.Versions");
            DropTable("dbo.Compatibilities");
            DropTable("dbo.CartographyFiles");
        }
    }
}
