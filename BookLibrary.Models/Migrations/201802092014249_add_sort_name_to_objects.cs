namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_sort_name_to_objects : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Albums", "SortTitle", c => c.String());
            AddColumn("dbo.Artists", "SortName", c => c.String());
            AddColumn("dbo.Books", "SortTitle", c => c.String());
            AddColumn("dbo.Magazines", "SortTitle", c => c.String());
            AddColumn("dbo.Movies", "SortTitle", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "SortTitle");
            DropColumn("dbo.Magazines", "SortTitle");
            DropColumn("dbo.Books", "SortTitle");
            DropColumn("dbo.Artists", "SortName");
            DropColumn("dbo.Albums", "SortTitle");
        }
    }
}
