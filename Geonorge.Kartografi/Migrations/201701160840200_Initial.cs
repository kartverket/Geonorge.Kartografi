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
                        OwnerOrganization = c.String(),
                        OwnerPerson = c.String(),
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
                        DateAccepted = c.DateTime(nullable: false),
                        AcceptedComment = c.String(),
                        OfficialStatus = c.Boolean(nullable: false),
                        Properties = c.String(),
                        Theme = c.String(),
                    })
                .PrimaryKey(t => t.SystemId)
                .ForeignKey("dbo.Versions", t => t.versioningId, cascadeDelete: true)
                .Index(t => t.versioningId);
            
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
            DropIndex("dbo.CartographyFiles", new[] { "versioningId" });
            DropTable("dbo.Versions");
            DropTable("dbo.CartographyFiles");
        }
    }
}
