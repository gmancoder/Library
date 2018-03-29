namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_magazines_to_library : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MagazineIssues",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MagazineId = c.Guid(nullable: false),
                        ReleaseDate = c.DateTime(nullable: false),
                        PdfId = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Magazines", t => t.MagazineId, cascadeDelete: true)
                .Index(t => t.MagazineId);
            
            CreateTable(
                "dbo.Magazines",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PdfCategoryFolderId = c.Int(nullable: false),
                        Title = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MagazineIssues", "MagazineId", "dbo.Magazines");
            DropIndex("dbo.MagazineIssues", new[] { "MagazineId" });
            DropTable("dbo.Magazines");
            DropTable("dbo.MagazineIssues");
        }
    }
}
