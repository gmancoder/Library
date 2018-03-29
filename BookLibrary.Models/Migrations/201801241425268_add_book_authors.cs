namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_book_authors : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Books", "AuthorId", "dbo.Authors");
            DropIndex("dbo.Books", new[] { "AuthorId" });
            CreateTable(
                "dbo.BookAuthors",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BookId = c.Guid(nullable: false),
                        BookTitle = c.String(),
                        AuthorId = c.Guid(nullable: false),
                        AuthorName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Authors", t => t.AuthorId, cascadeDelete: true)
                .ForeignKey("dbo.Books", t => t.BookId, cascadeDelete: true)
                .Index(t => t.BookId)
                .Index(t => t.AuthorId);
            
            AddColumn("dbo.Books", "Authors", c => c.String());
            DropColumn("dbo.Books", "AuthorId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Books", "AuthorId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.BookAuthors", "BookId", "dbo.Books");
            DropForeignKey("dbo.BookAuthors", "AuthorId", "dbo.Authors");
            DropIndex("dbo.BookAuthors", new[] { "AuthorId" });
            DropIndex("dbo.BookAuthors", new[] { "BookId" });
            DropColumn("dbo.Books", "Authors");
            DropTable("dbo.BookAuthors");
            CreateIndex("dbo.Books", "AuthorId");
            AddForeignKey("dbo.Books", "AuthorId", "dbo.Authors", "Id", cascadeDelete: true);
        }
    }
}
