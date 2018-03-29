namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_entry_type_to_book : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Books", "EntryType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Books", "EntryType");
        }
    }
}
