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
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
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
                        DateChanged = c.DateTime(nullable: false),
                        Status = c.String(),
                        DateAccepted = c.DateTime(nullable: false),
                        AcceptedComment = c.String(),
                        OfficialStatus = c.String(),
                        Properties = c.String(),
                        Theme = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Symbols",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        EksternalSymbolID = c.String(),
                        OwnerOrganization = c.String(),
                        OwnerPerson = c.String(),
                        LastEditedBy = c.String(),
                        Type = c.String(),
                        DateChanged = c.DateTime(nullable: false),
                        Status = c.String(),
                        DateAccepted = c.DateTime(nullable: false),
                        OfficialStatus = c.String(),
                        Theme = c.String(),
                        Source = c.String(),
                        SourceUrl = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SymbolFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        Format = c.String(),
                        Type = c.String(),
                        Color = c.String(),
                        Size = c.String(),
                        Symbol_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Symbols", t => t.Symbol_Id, cascadeDelete : true)
                .Index(t => t.Symbol_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SymbolFiles", "Symbol_Id", "dbo.Symbols");
            DropIndex("dbo.SymbolFiles", new[] { "Symbol_Id" });
            DropTable("dbo.SymbolFiles");
            DropTable("dbo.Symbols");
            DropTable("dbo.CartographyFiles");
        }
    }
}
