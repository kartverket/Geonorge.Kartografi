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
                        SystemId = c.Guid(nullable: false, identity: true),
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
                        DateChanged = c.DateTime(nullable: false),
                        Status = c.String(),
                        DateAccepted = c.DateTime(nullable: false),
                        AcceptedComment = c.String(),
                        OfficialStatus = c.String(),
                        Properties = c.String(),
                        Theme = c.String(),
                    })
                .PrimaryKey(t => t.SystemId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CartographyFiles");
        }
    }
}
