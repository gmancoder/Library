namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_fields_to_book_1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "PublicationDate", c => c.DateTime());
            AddColumn("dbo.Books", "Publisher", c => c.String());
            AddColumn("dbo.Books", "ReleaseDate", c => c.DateTime());
            AddColumn("dbo.Books", "ASIN", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "ASIN");
            DropColumn("dbo.Books", "ReleaseDate");
            DropColumn("dbo.Books", "Publisher");
            DropColumn("dbo.Books", "PublicationDate");
        }
    }
}
