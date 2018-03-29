namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_pdf_id_to_book : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "PdfId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "PdfId");
        }
    }
}
