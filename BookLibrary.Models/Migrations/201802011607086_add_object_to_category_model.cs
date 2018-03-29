namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_object_to_category_model : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ObjectToCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BrowseNodeId = c.Long(nullable: false),
                        ObjectType = c.String(),
                        ObjectId = c.Guid(nullable: false),
                        CategoryId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ObjectToCategories", "CategoryId", "dbo.Categories");
            DropIndex("dbo.ObjectToCategories", new[] { "CategoryId" });
            DropTable("dbo.ObjectToCategories");
        }
    }
}
