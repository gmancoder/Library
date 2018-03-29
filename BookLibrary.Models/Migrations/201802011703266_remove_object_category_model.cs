namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class remove_object_category_model : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ObjectCategories", "CategoryId", "dbo.Categories");
            DropIndex("dbo.ObjectCategories", new[] { "CategoryId" });
            DropTable("dbo.ObjectCategories");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ObjectCategories",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Id2 = c.Int(nullable: false),
                        BrowseNodeId = c.Long(nullable: false),
                        ObjectType = c.String(),
                        ObjectId = c.Guid(nullable: false),
                        CategoryId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ObjectCategories", "CategoryId");
            AddForeignKey("dbo.ObjectCategories", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
        }
    }
}
