namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class replace_book_categories_with_object_categories : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BookCategories", "BookId", "dbo.Books");
            RenameTable(name: "dbo.BookCategories", newName: "ObjectCategories");
            
            DropForeignKey("dbo.Magazines", "CategoryId", "dbo.Categories");
            DropIndex("dbo.ObjectCategories", new[] { "BookId" });
            DropIndex("dbo.Magazines", new[] { "CategoryId" });
            AddColumn("dbo.ObjectCategories", "ObjectType", c => c.String());
            AddColumn("dbo.ObjectCategories", "ObjectId", c => c.Guid(nullable: false));
            DropColumn("dbo.Magazines", "CategoryId");
            DropColumn("dbo.ObjectCategories", "BookId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ObjectCategories", "BookId", c => c.Guid(nullable: false));
            AddColumn("dbo.Magazines", "CategoryId", c => c.Guid(nullable: false));
            DropColumn("dbo.ObjectCategories", "ObjectId");
            DropColumn("dbo.ObjectCategories", "ObjectType");
            CreateIndex("dbo.Magazines", "CategoryId");
            CreateIndex("dbo.ObjectCategories", "BookId");
            AddForeignKey("dbo.Magazines", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookCategories", "BookId", "dbo.Books", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.ObjectCategories", newName: "BookCategories");
        }
    }
}
